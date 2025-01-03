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
            logger.LogDebug("Cloning repository {RepoUrl}", metadata.RepositoryUrl);
            var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            Repository.Clone(metadata.RepositoryUrl, tempDir.FullName, GetCloneOptions(metadata.RepositoryUrl));

            if (metadata == null)
            {
                logger.LogError("Failed to get package metadata from repository {RepoUrl}", metadata.RepositoryUrl);
                throw new InvalidOperationException("Failed to get package metadata");
            }

            var targetDir = new DirectoryInfo(Path.Combine(bdziamPakDirectory.PaksDirectory.FullName, $"{metadata.BdziamPakId}@{metadata.Version}"));
            if (!targetDir.Exists)
            {
                targetDir.Create();
            }

            CopyFilesRecursively(tempDir, targetDir);
            logger.LogDebug("Successfully cloned repository {RepoUrl} to {TargetDir}",  metadata.RepositoryUrl, targetDir.FullName);

            return targetDir;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cloning repository {RepoUrl}",  metadata.RepositoryUrl);
            throw;
        }
    }

    private CloneOptions GetCloneOptions(string repoUrl)
    {
        var options = new CloneOptions();

        var credentials = gitCredentials.GetCredentialsForRepo(repoUrl);
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
            var targetSubDir = target.CreateSubdirectory(dir.Name);
            CopyFilesRecursively(dir, targetSubDir);
        }

        foreach (var file in source.GetFiles())
        {
            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }
    }
}