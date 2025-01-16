using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Git;

/// <summary>
/// Provides functionality for interacting with Git repositories.
/// </summary>
public class GitService
{
    private readonly ILogger<GitService> logger;
    private readonly GitCredentials gitCredentials;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging.</param>
    /// <param name="gitCredentials">The Git credentials manager.</param>
    public GitService(ILogger<GitService> logger, GitCredentials gitCredentials)
    {
        this.logger = logger;
        this.gitCredentials = gitCredentials;
    }

    /// <summary>
    /// Clones a Git repository to the specified directory and checks out the specified commit.
    /// </summary>
    /// <param name="targetDir">The target directory to clone the repository into.</param>
    /// <param name="url">The URL of the Git repository.</param>
    /// <param name="commitHash">The commit hash to check out.</param>
    /// <returns>The target directory containing the cloned repository.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified commit is not found.</exception>
    public DirectoryInfo CloneRepo(DirectoryInfo targetDir, string url, string commitHash, IProgress<CloneRepositoryProgress> progress, CancellationToken cancellationToken)
    {
        var cloneProgress = new CloneRepositoryProgress();

        try
        {
            if (!targetDir.Exists) targetDir.Create();

            logger.LogDebug("Cloning repository {RepoUrl}", url);
            var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Repository.Clone(url, tempDir.FullName, GetCloneOptions(url, progress, cancellationToken, cloneProgress));
            using (var repo = new Repository(tempDir.FullName))
            {
                var commit = repo.Lookup<Commit>(commitHash);
                if (commit == null)
                {
                    logger.LogError("Commit {CommitHash} not found in repository {RepoUrl}", commitHash, url);
                    throw new InvalidOperationException($"Commit {commitHash} not found");
                }

                var checkoutOptions = new CheckoutOptions();
                checkoutOptions.OnCheckoutProgress = (path, completedSteps, totalSteps) =>
                {
                    cloneProgress.CheckoutProgress = GetProgress(completedSteps, totalSteps);
                    cloneProgress.Path = path;
                    progress.Report(cloneProgress);
                };
                Commands.Checkout(repo, commit);
            }

            CopyFilesRecursively(tempDir, targetDir);
            cloneProgress.Message = "Repository cloned successfully";
            progress.Report(cloneProgress);
            return targetDir;
        }
        catch (Exception ex)
        {
            cloneProgress.Message = $"Error cloning repository {url}: {ex.Message}";
            logger.LogError(ex, "Error cloning repository {RepoUrl}", url);
            throw;
        }
    }

    /// <summary>
    /// Gets the clone options for the specified repository URL.
    /// </summary>
    /// <param name="url">The URL of the Git repository.</param>
    /// <returns>The clone options.</returns>
    private CloneOptions GetCloneOptions(string url, IProgress<CloneRepositoryProgress> progress, CancellationToken cancellationToken, CloneRepositoryProgress? cloneProgress = null)
    {
        var options = new CloneOptions();
        if(cloneProgress == null)
            cloneProgress = new CloneRepositoryProgress();
        options.OnCheckoutProgress = (path, completedSteps, totalSteps) =>
        {
            cloneProgress.Message = "Checking out files...";
            cloneProgress.CheckoutProgress = GetProgress(completedSteps, totalSteps);
            cloneProgress.Path = path;
            progress.Report(cloneProgress);
        };
        options.FetchOptions.OnTransferProgress = (transferProgress) =>
        {
            cloneProgress.FetchProgress =  GetProgress(transferProgress.ReceivedObjects, transferProgress.TotalObjects);
            cloneProgress.Message = $"Transferring objects... ({transferProgress.ReceivedBytes} bytes received)";
            progress.Report(cloneProgress);
            
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
        return (current / Math.Max(total, 1) * 100);
    }

    /// <summary>
    /// Recursively copies files and directories from the source directory to the target directory.
    /// </summary>
    /// <param name="source">The source directory.</param>
    /// <param name="target">The target directory.</param>
    private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
    {
        foreach (var dir in source.GetDirectories())
        {
            if (dir.Name == ".git") continue;
            var targetSubDir = target.CreateSubdirectory(dir.Name);
            CopyFilesRecursively(dir, targetSubDir);
        }

        foreach (var file in source.GetFiles())
            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
    }
}