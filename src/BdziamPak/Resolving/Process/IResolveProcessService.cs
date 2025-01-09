using BdziamPak.Packages.Packaging.Model;

namespace BdziamPak.Resolving;

public interface IResolveProcessService
{
    Task ResolveAsync(BdziamPakMetadata bdziamPakMetadata, IProgress<ResolveStatusLog>? progress);
}