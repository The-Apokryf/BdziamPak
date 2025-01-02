namespace BdziamPak.Packages.Index.Model;

public class BdziamPakIndex
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<BdziamPakReference> Paks { get; set; }
}