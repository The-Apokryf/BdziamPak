namespace BdziamPak.Git.Model;

/// <summary>
///     Represents Git credentials with a username and password.
/// </summary>
public class GitCredential
{
    /// <summary>
    ///     Gets or sets the username for the Git credential.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    ///     Gets or sets the password for the Git credential.
    /// </summary>
    public string Password { get; set; }
}