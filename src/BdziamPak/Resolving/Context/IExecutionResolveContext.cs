using BdziamPak.Packages.Packaging.Model;

namespace BdziamPak.Resolving;

public interface IExecutionResolveContext
{
    public BdziamPakMetadata BdziamPakMetadata { get; }
    DirectoryInfo ResolveDirectory { get; }
    void Fail(string message);
    void Skip(string message);
    void Complete();
    void UpdateStatus(string message, int? percent = null);
    FileInfo GetFile(string relativePath);
    DirectoryInfo GetDirectory(string relativePath);
    public T? GetMetadata<T>(string key);
}