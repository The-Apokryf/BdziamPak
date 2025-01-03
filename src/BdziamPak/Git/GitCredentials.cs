using System.Text.Json;
using BdziamPak.Git.Model;
using BdziamPak.Structure;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Git;

public class GitCredentials
{
    private Dictionary<string, GitCredential> _credentials = new();
    private readonly string _credentialsFilePath;
    private readonly ILogger _logger;

    public GitCredentials(BdziamPakDirectory directory, ILogger<GitCredentials> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("Initializing Git Credentials");
            _credentialsFilePath = Path.Combine(directory.RootDirectory.FullName, "GitCredentials.json");
            if (!File.Exists(_credentialsFilePath))
            {
                _logger.LogDebug("Credentials file does not exist. Creating a new one.");
                SaveCredentials();
                return;
            }
            ReloadCredentials();
            _logger.LogDebug("Initialization complete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing GitCredentials");
            throw;
        }
    }

    private void SaveCredentials()
    {
        try
        {
            _logger.LogDebug("Saving credentials to file {FilePath}", _credentialsFilePath);
            File.WriteAllText(_credentialsFilePath, JsonSerializer.Serialize(_credentials, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving credentials to file {FilePath}", _credentialsFilePath);
            throw;
        }
    }

    private void ReloadCredentials()
    {
        try
        {
            _logger.LogDebug("Reloading credentials from file {FilePath}", _credentialsFilePath);
            var fileContent = File.ReadAllText(_credentialsFilePath);
            _credentials = JsonSerializer.Deserialize<Dictionary<string, GitCredential>>(fileContent) ?? new Dictionary<string, GitCredential>();
            _logger.LogDebug("Successfully reloaded credentials");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading credentials from file {FilePath}", _credentialsFilePath);
            throw;
        }
    }

    public GitCredential? GetCredentialsForRepo(string url)
    {
        try
        {
            _logger.LogDebug("Fetching credentials for repo {RepoUrl}", url);
            ReloadCredentials();
            if (_credentials.TryGetValue(url, out var creds))
            {
                _logger.LogDebug("Credentials found for repo {RepoUrl}", url);
                return creds;
            }

            _logger.LogError("Credentials for repo: {RepoUrl} not found!", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching credentials for repo {RepoUrl}", url);
            throw;
        }
    }
}