using BdziamPak.NuGetPackages.Cache;
using BdziamPak.Packages.Index.Local;

namespace BdziamPak.Structure;
///  <summary>
///  Manages specified BdziamPak Directory
///  </summary>
///  <remarks>
///  Directory Structure
/// │.{directoryName}/
/// ├── Sources/                     # Directory for files
///     │   ├── Sources.json             # File that contains all Sources registered
///     │   ├── {SourceName}.json        # Cached index data for each source
///     ├── Cache/                       # Cache for downloaded NuGet packages
///     │   ├── {BdziamPakName}@{Version}/ #  folder for a specific BdziamPak 
///     │       ├── Lib/            # Contains extracted nuget packages assigned to a given BdziamPak
///     ├── Paks/                        # Extracted BdziamPaks
///     │   ├── {BdziamPakName}@{Version}/ #  folder for a specific BdziamPak (contains cloned repo from git and Lib folder with extracted dependencies
///     │       ├── Lib/            # Contains extracted nuget packages assigned to a given BdziamPak
///     │       ├── (other git contents)           # Contains extracted nuget packages assigned to a given BdziamPak
///     │       ├── pak.json      # Contains Metadata about given Pak
///  </remarks>
public class BdziamPakDirectory
{

    public BdziamPakDirectory(string directoryName)
    {
    }

    /// <summary>
    /// Directory that contains all sources
    /// </summary>
    public DirectoryInfo SourcesDirectory { get; }
    /// <summary>
    /// Directory that contains all cached NuGet packages
    /// </summary>
    public DirectoryInfo CacheDirectory { get; }
    /// <summary>
    /// Directory that contains all extracted BdziamPaks
    /// </summary>
    public DirectoryInfo PaksDirectory { get; }
    /// <summary>
    /// Allows to manage sources of the directory
    /// </summary>
    public Sources Sources { get; }
    /// <summary>
    /// Nuget Cache
    /// </summary>
    public NuGetCache NuGetCache { get; }
}