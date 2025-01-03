using BdziamPak.Packages.Packaging.Model;

namespace BdziamPak.Tests;

public static class TestData
{
    public static BdziamPakMetadata BdziamPakMetadata => new ()
    {
        Name = "testPak",
        Author = "TestAuthor",
        Version = "1.0.0",
        Repository = new BdziamPakRepositoryReference
        {
            Url = "https://github.com/pmikstacki/SliccDB.git",
            CommitHash = "f5c1e7c"
        },
        NuGetPackage = new BdziamPakNuGetDependency()
        {
            PackageId = "Netwonsoft.Json",
            PackageVersion = "13.0.3"
        }
    };
    
}