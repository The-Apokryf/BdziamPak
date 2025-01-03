namespace BdziamPak.Packaging.Install.Model;


public class BdziamPakInstallResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<LocalBdziamPak> ResolvedDependencies { get; set; } = new();
    public int TotalResolved { get; set; }
}