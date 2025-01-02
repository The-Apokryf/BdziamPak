namespace BdziamPak.Packages.Packaging.Model;

public class BdziamPakNuGetDependency
{
    public string PackageId { get; set; }
    public string PackageVersion { get; set; }
    public string? NuGetFeedUrl { get; set; }
    public bool Prerelease { get; set; } = false;
}