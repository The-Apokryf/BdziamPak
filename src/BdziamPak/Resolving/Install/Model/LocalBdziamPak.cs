namespace BdziamPak.Packaging.Install.Model;

public class LocalBdziamPak
{
    public string BdziamPakId { get; set; }
    public string Version { get; set; }
    public LocalBdziamPak[] Dependencies { get; set; }
}