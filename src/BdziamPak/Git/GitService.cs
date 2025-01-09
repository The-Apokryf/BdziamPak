using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Git;

public class GitService(
    ILogger<GitService> logger,
    GitCredentials gitCredentials)
{
    public DirectoryInfo CloneRepo(DirectoryInfo targetDir, string url, string commitHash)
    {
        try
        {
            if (!targetDir.Exists) targetDir.Create();

            logger.LogDebug("Cloning repository {RepoUrl}", url);
            var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Repository.Clone(url, tempDir.FullName, GetCloneOptions(url));

            using (var repo = new Repository(tempDir.FullName))
            {
                var commit = repo.Lookup<Commit>(commitHash);
                if (commit == null)
                {
                    logger.LogError("Commit {CommitHash} not found in repository {RepoUrl}",
                        commitHash, url);
                    throw new InvalidOperationException($"Commit {commitHash} not found");
                }

                Commands.Checkout(repo, commit);
            }

            CopyFilesRecursively(tempDir, targetDir);
            logger.LogDebug("Successfully cloned repository {RepoUrl} to {TargetDir}", url,
                targetDir.FullName);

            return targetDir;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cloning repository {RepoUrl}", url);
            throw;
        }
    }

    private CloneOptions GetCloneOptions(string url)
    {
        var options = new CloneOptions();

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

    private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
    {
        foreach (var dir in source.GetDirectories())
        {
            if (dir.Name == ".git") continue;
            var targetSubDir = target.CreateSubdirectory(dir.Name);
            CopyFilesRecursively(dir, targetSubDir);
        }

        foreach (var file in source.GetFiles()) file.CopyTo(Path.Combine(target.FullName, file.Name), true);
    }
}