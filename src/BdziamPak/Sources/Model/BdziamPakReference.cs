namespace BdziamPak.Packages.Index.Model;

public class BdziamPakReference
{
    /// <summary>
    /// id of the package.
    /// Consists of {Author}.{Name}
    /// </summary>
    public string BdziamPakId { get; set; }
    
    /// <summary>
    /// Url of the BdziamPak Git Repository
    /// </summary>
    public string RepositoryUrl { get; set; }
}