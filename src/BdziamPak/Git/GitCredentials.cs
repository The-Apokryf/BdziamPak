using System.Text.Json;
using BdziamPak.Git.Model;
using BdziamPak.Structure;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Git;

public class GitCredentials
{
    private readonly string _credentialsFilePath;
    private readonly object _lockObject = new();
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
                SaveCredentials(new Dictionary<string, GitCredential>());
                return;
            }

            _logger.LogDebug("Initialization complete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing GitCredentials");
            throw;
        }
    }

    private void SaveCredentials(Dictionary<string, GitCredential> credentials)
    {
        try
        {
            _logger.LogDebug("Saving credentials to file {FilePath}", _credentialsFilePath);
            lock (_lockObject)
            {
                using (var fs = new FileStream(_credentialsFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fs))
                {
                    var json = JsonSerializer.Serialize(credentials,
                        new JsonSerializerOptions { WriteIndented = true });
                    writer.Write(json);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving credentials to file {FilePath}", _credentialsFilePath);
            throw;
        }
    }

    private Dictionary<string, GitCredential> LoadCredentials()
    {
        try
        {
            _logger.LogDebug("Loading credentials from file {FilePath}", _credentialsFilePath);
            lock (_lockObject)
            {
                using (var fs = new FileStream(_credentialsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(fs))
                {
                    var fileContent = reader.ReadToEnd();
                    return JsonSerializer.Deserialize<Dictionary<string, GitCredential>>(fileContent)
                           ?? new Dictionary<string, GitCredential>();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading credentials from file {FilePath}", _credentialsFilePath);
            throw;
        }
    }

    public GitCredential? GetCredentialsForRepo(string url)
    {
        try
        {
            _logger.LogDebug("Fetching credentials for repo {RepoUrl}", url);
            var credentials = LoadCredentials();

            if (credentials.TryGetValue(url, out var creds))
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