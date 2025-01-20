using BdziamPak.Operations.Reporting.Progress;
using BdziamPak.Operations.Reporting.States;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Git;

/// <summary>
///     Provides functionality for interacting with Git repositories.
/// </summary>
public class GitService
{
    private readonly GitCredentials gitCredentials;
    private readonly ILogger<GitService> logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GitService" /> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging.</param>
    /// <param name="gitCredentials">The Git credentials manager.</param>
    public GitService(ILogger<GitService> logger, GitCredentials gitCredentials)
    {
        this.logger = logger;
        this.gitCredentials = gitCredentials;
    }

    /// <summary>
    ///     Clones a Git repository to the specified directory and checks out the specified commit.
    /// </summary>
    /// <param name="targetDir">The target directory to clone the repository into.</param>
    /// <param name="url">The URL of the Git repository.</param>
    /// <param name="commitHash">The commit hash to check out.</param>
    /// <returns>The target directory containing the cloned repository.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified commit is not found.</exception>
    public void CloneRepo(DirectoryInfo targetDir, string url, string commitHash, StepProgress progress,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!targetDir.Exists) targetDir.Create();

            logger.LogDebug("Cloning repository {RepoUrl}", url);

            var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Repository.Clone(url, tempDir.FullName, GetCloneOptions(url, progress, cancellationToken));
            using (var repo = new Repository(tempDir.FullName))
            {
                var commit = repo.Lookup<Commit>(commitHash);
                if (commit == null)
                {
                    logger.LogError("Commit {CommitHash} not found in repository {RepoUrl}", commitHash, url);
                    progress.Finish($"Commit {commitHash} not found in repository {url}", true);
                    throw new InvalidOperationException($"Commit {commitHash} not found");
                }

                var checkoutOptions = new CheckoutOptions();
                checkoutOptions.OnCheckoutProgress = (path, completedSteps, totalSteps) =>
                {
                    progress.Determinate("Checkout", completedSteps, totalSteps);
                    progress.Status($"({completedSteps}/{totalSteps}) Checking out {path}...");
                };
                Commands.Checkout(repo, commit, checkoutOptions);
            }
            System.IO.Directory.Delete(Path.Combine(tempDir.FullName, ".git"), true);
            progress.Determinate("Copy", 0, tempDir.GetFiles("*.*", SearchOption.AllDirectories).Length);
            progress.Status($"Copying files to {targetDir.FullName}...");

            CopyFilesRecursively(tempDir, targetDir, progress);
            progress.Finish("Cloning Complete");
        }
        catch (Exception ex)
        {
            progress.Finish($"Error during Clone: {ex.Message}", true);
            logger.LogError(ex, "Error cloning repository {RepoUrl}", url);
            return;
        }
    }

    /// <summary>
    ///     Gets the clone options for the specified repository URL.
    /// </summary>
    /// <param name="url">The URL of the Git repository.</param>
    /// <returns>The clone options.</returns>
    private CloneOptions GetCloneOptions(string url, StepProgress progress, CancellationToken cancellationToken)
    {
        progress.Status($"Preparing to clone from {url}...");
        var options = new CloneOptions();
        options.FetchOptions.OnTransferProgress = transferProgress =>
        {
            progress.Determinate("Fetch", transferProgress.ReceivedObjects, transferProgress.TotalObjects);
            progress.Status($"Fetching objects: {transferProgress.ReceivedObjects}/{transferProgress.TotalObjects}...");
            if (cancellationToken.IsCancellationRequested)
                return false;
            return true;
        };

        var credentials = gitCredentials.GetCredentialsForRepo(url);
        if (credentials != null)
            options.FetchOptions.CredentialsProvider = (url, user, cred) =>
                new UsernamePasswordCredentials
                {
                    Username = credentials.Username,
                    Password = credentials.Password
                };

        return options;
    }

    private int GetProgress(int current, int total)
    {
        return (int)(current / Math.Max(total, 1.0d) * 100);
    }

    /// <summary>
    ///     Recursively copies files and directories from the source directory to the target directory.
    /// </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="target">The target directory.</param>
    private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, StepProgress progress)
    {
        foreach (var dir in source.GetDirectories())
        {
            if (dir.Name == ".git") continue;
            var targetSubDir = target.CreateSubdirectory(dir.Name);
            CopyFilesRecursively(dir, targetSubDir, progress);
        }

        foreach (var file in source.GetFiles())
        {
            progress.Increment("Copy");
            progress.Status($"Copying {file.Name}...");

            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }
    }
}