using NuGet.Frameworks;

namespace BdziamPak.Configuration;

public class BdziamPakConfiguration(string bdziamPakPath)
{
    public string BdziamPakPath { get; set; } = bdziamPakPath;
}