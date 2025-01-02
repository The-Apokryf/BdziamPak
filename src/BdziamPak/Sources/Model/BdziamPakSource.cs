namespace BdziamPak.Packages.Index.Model;

public class BdziamPakSource
{
    public string Name { get; set; }
    public string Url { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? LocalIndexPath { get; set; }
}