namespace BdziamPak.NuGetPackages.Model;

/// <summary>
/// Represents the progress of a NuGet package download.
/// </summary>
/// <param name="Message">The message describing the current state of the download.</param>
/// <param name="percent">The percentage of the download completed, if available.</param>
public record NuGetDownloadProgress(string Message, int? percent = null);