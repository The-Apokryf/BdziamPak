namespace BdziamPak.NuGetPackages.Cache;

public class NuGetCache(DirectoryInfo cacheDirectory)
{
    public void ClearCache()
    {
        foreach (var file in cacheDirectory.EnumerateFiles())
        {
            file.Delete();
        }
    }
    
    
}