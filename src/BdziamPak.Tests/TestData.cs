using BdziamPak.Packages.Index.Model;
using BdziamPak.Packages.Packaging.Model;

namespace BdziamPak.Tests;

public static class TestData
{
    public static BdziamPakMetadata GoodMetadata => new ()
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
            PackageId = "SliccDB",
            PackageVersion = "0.1.1.4"
        }
    };
    
    
    public static BdziamPakMetadata BadMetadata => new ()
    {
        Name = "testPak",
        Author = "TestAuthor",
        Version = "1.0.0",
        Repository = new BdziamPakRepositoryReference
        {
            Url = "https://github.com/pmikstacki/SliccDB.git",
            CommitHash = "sssdafwe"
        },
        NuGetPackage = new BdziamPakNuGetDependency()
        {
            PackageId = "Newtonsoft.Json",
            PackageVersion = "13.0.3"
        }
    };
    
    public static BdziamPakSourceIndex SourceIndex => new ()
    {
        Name = "TestIndex",
        Description = "TestDescription",
        Paks = new List<BdziamPakMetadata>
        {
            GoodMetadata
        }
    };
    
}