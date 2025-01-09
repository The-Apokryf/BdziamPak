using BdziamPak.NuGetPackages.Model;

namespace BdziamPak.Packaging.Install.Model;

public class BdziamPakInstallProgress
{
    public string Message { get; set; }
    public List<IProgress<NuGetDownloadProgress>> NuGetProgresses { get; } = new();
    public List<IProgress<BdziamPakInstallProgress>> DependencyProgresses { get; } = new();
    public int TotalPackages { get; set; }
    public int CompletedPackages { get; set; }
    public decimal ProgressPercentage => TotalPackages == 0 ? 0 : CompletedPackages * 100M / TotalPackages;
}