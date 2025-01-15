namespace BdziamPak.NuGetPackages.Download.Model;

/// <summary>
/// Represents the progress of a NuGet package download.
/// </summary>
/// <param name="Message">The message describing the current state of the download.</param>
/// <param name="Percent">The percentage of the download completed, if available.</param>
public record NuGetDownloadProgress(string Message, int? Percent = null);