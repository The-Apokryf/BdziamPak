using BdziamPak.Packages.Packaging.Model;

namespace BdziamPak.Resolving;

/// <summary>
/// Defines the interface for a service that resolves BdziamPak packages.
/// </summary>
public interface IResolveProcessService
{
    /// <summary>
    /// Resolves the specified BdziamPak package metadata.
    /// </summary>
    /// <param name="bdziamPakMetadata">The metadata of the BdziamPak package to resolve.</param>
    /// <param name="progress">The progress reporter for the resolving process.</param>
    /// <returns>A task that represents the asynchronous resolve operation.</returns>
    Task ResolveAsync(BdziamPakMetadata bdziamPakMetadata, IProgress<ResolveStatusLog>? progress);
}