using BdziamPak.Resolving.ResolveSteps;

namespace BdziamPak.Resolving;

public interface ICheckResolveContext
{
    public bool WasCompleted<TStep>() where TStep : BdziamPakResolveStep;
    public bool FileExists(string relativePath);
    public bool DirectoryExists(string relativePath);
    public bool HasMetadata(string key);
    public T? GetMetadata<T>(string key);
}