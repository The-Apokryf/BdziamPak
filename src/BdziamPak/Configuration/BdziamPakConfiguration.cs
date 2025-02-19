namespace BdziamPak.Configuration;

/// <summary>
///     Represents the configuration for BdziamPak.
/// </summary>
/// <param name="bdziamPakPath">The path to the BdziamPak directory.</param>
public class BdziamPakConfiguration(string bdziamPakPath)
{
    /// <summary>
    ///     Gets or sets the path to the BdziamPak directory.
    /// </summary>
    public string BdziamPakPath { get; set; } = bdziamPakPath;
}