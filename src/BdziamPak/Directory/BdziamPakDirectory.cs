using BdziamPak.Configuration;
using System.IO;

namespace BdziamPak.Structure;

/// <summary>
///     Manages specified BdziamPak Directory
/// </summary>
/// <remarks>
/// ```plaintext
/// Directory Structure
/// │.{directoryName}/
/// ├── Sources/                     # Directory for files
/// │   ├── Sources.json             # File that contains all Sources registered
/// │   ├── {SourceName}.json        # Cached index data for each source
/// ├── Cache/                       # Cache for downloaded NuGet packages
/// ├── Paks/                        # Extracted BdziamPaks
/// │   ├── {BdziamPakId}@{Version}/ #  folder for a specific BdziamPak (contains cloned repo from git and Lib folder
/// with extracted dependencies
/// │       ├── Lib/            # Contains extracted nuget packages assigned to a given BdziamPak
/// │       ├── (other git contents)           # Contains extracted nuget packages assigned to a given BdziamPak
/// │       ├── pak.json      # Contains Metadata about given Pak
/// ├── GitCredentials.json # Contains Git credentials
/// ```
/// </remarks>
public class BdziamPakDirectory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="BdziamPakDirectory"/> class.
    /// </summary>
    /// <param name="bdziamPakConfiguration">The configuration for BdziamPak.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public BdziamPakDirectory(BdziamPakConfiguration bdziamPakConfiguration, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        RootDirectory = new DirectoryInfo(bdziamPakConfiguration.BdziamPakPath);
        if (!RootDirectory.Exists)
            RootDirectory.Create();

        CacheDirectory = new DirectoryInfo(Path.Combine(RootDirectory.FullName, "Cache"));
        SourcesDirectory = new DirectoryInfo(Path.Combine(RootDirectory.FullName, "Sources"));
        PaksDirectory = new DirectoryInfo(Path.Combine(RootDirectory.FullName, "Paks"));

        if (!CacheDirectory.Exists)
            CacheDirectory.Create();
        if (!SourcesDirectory.Exists)
            SourcesDirectory.Create();
        if (!PaksDirectory.Exists)
            PaksDirectory.Create();
    }

    /// <summary>
    ///     Directory that contains all sources
    /// </summary>
    public DirectoryInfo SourcesDirectory { get; }

    /// <summary>
    ///     Directory that contains all cached NuGet packages
    /// </summary>
    public DirectoryInfo CacheDirectory { get; }

    /// <summary>
    ///     Directory that contains all extracted BdziamPaks
    /// </summary>
    public DirectoryInfo PaksDirectory { get; }

    /// <summary>
    ///     Root directory containing all BdziamPak files and folders
    /// </summary>
    public DirectoryInfo RootDirectory { get; }
}