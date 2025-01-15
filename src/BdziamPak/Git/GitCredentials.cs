/// <summary>
/// Provides functionality for managing Git credentials.
/// </summary>

using BdziamPak.Directory;

namespace BdziamPak.Git
{
    using System.Text.Json;
    using BdziamPak.Git.Model;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Manages Git credentials stored in a JSON file.
    /// </summary>
    public class GitCredentials
    {
        private readonly string _credentialsFilePath;
        private readonly object _lockObject = new();
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitCredentials"/> class.
        /// </summary>
        /// <param name="directory">The directory where the credentials file is stored.</param>
        /// <param name="logger">The logger instance for logging.</param>
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

        /// <summary>
        /// Saves the provided credentials to the credentials file.
        /// </summary>
        /// <param name="credentials">The credentials to save.</param>
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
                        var json = JsonSerializer.Serialize(credentials, new JsonSerializerOptions { WriteIndented = true });
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

        /// <summary>
        /// Loads the credentials from the credentials file.
        /// </summary>
        /// <returns>A dictionary containing the loaded credentials.</returns>
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

        /// <summary>
        /// Gets the credentials for the specified repository URL.
        /// </summary>
        /// <param name="url">The repository URL.</param>
        /// <returns>The credentials for the repository, or null if not found.</returns>
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
}