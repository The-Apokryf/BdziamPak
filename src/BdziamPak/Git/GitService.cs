using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Structure;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Git;

public class GitService(ILogger<GitService> logger, BdziamPakDirectory bdziamPakDirectory, GitCredentials gitCredentials)
{
    public DirectoryInfo CloneRepo(BdziamPakMetadata metadata)
    {
        try
        {
            var targetDir = new DirectoryInfo(Path.Combine(bdziamPakDirectory.PaksDirectory.FullName,
                $"{metadata.BdziamPakId}@{metadata.Version}"));
            if (!targetDir.Exists)
            {
                targetDir.Create();
            }

            if (metadata.Repository == null) return targetDir;

            logger.LogDebug("Cloning repository {RepoUrl}", metadata.Repository.Url);
            var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Repository.Clone(metadata.Repository.Url, tempDir.FullName, GetCloneOptions(metadata.Repository));

            using (var repo = new Repository(tempDir.FullName))
            {
                var commit = repo.Lookup<Commit>(metadata.Repository.CommitHash);
                if (commit == null)
                {
                    logger.LogError("Commit {CommitHash} not found in repository {RepoUrl}",
                        metadata.Repository.CommitHash, metadata.Repository.Url);
                    throw new InvalidOperationException($"Commit {metadata.Repository.CommitHash} not found");
                }

                Commands.Checkout(repo, commit);
            }

            CopyFilesRecursively(tempDir, targetDir);
            logger.LogDebug("Successfully cloned repository {RepoUrl} to {TargetDir}", metadata.Repository.Url,
                targetDir.FullName);

            return targetDir;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cloning repository {RepoUrl}", metadata.Repository.Url);
            throw;
        }
    }

    private CloneOptions GetCloneOptions(BdziamPakRepositoryReference reference)
    {
        var options = new CloneOptions();

        var credentials = gitCredentials.GetCredentialsForRepo(reference.Url);
        if (credentials != null)
        {
            options.FetchOptions.CredentialsProvider = (url, user, cred) =>
                new UsernamePasswordCredentials
                {
                    Username = credentials.Username,
                    Password = credentials.Password
                };
        }

        return options;
    }

    private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
    {
        foreach (var dir in source.GetDirectories())
        {
            if(dir.Name == ".git") continue;
            var targetSubDir = target.CreateSubdirectory(dir.Name);
            CopyFilesRecursively(dir, targetSubDir);
        }

        foreach (var file in source.GetFiles())
        {
            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }
    }
}