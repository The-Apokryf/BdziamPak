namespace BdziamPak.Tests;

public static class TestData
{
    public static object GoodMetadata => new
    {
        Name = "testPak",
        Author = "TestAuthor",
        Version = "1.0.0",
        Repository = new
        {
            Url = "https://github.com/pmikstacki/SliccDB.git",
            CommitHash = "f5c1e7c"
        },
        NuGetPackage = new
        {
            PackageId = "SliccDB",
            PackageVersion = "0.1.1.4"
        }
    };


    public static object BadMetadata => new
    {
        Name = "testPak",
        Author = "TestAuthor",
        Version = "1.0.0",
        Repository = new
        {
            Url = "https://github.com/pmikstacki/SliccDB.git",
            CommitHash = "sdsdsd"
        },
        NuGetPackage = new
        {
            PackageId = "SliccDB",
            PackageVersion = "0.1.1.4"
        }
    };

    public static object SourceIndex => new
    {
        Name = "TestIndex",
        Description = "TestDescription",
        Paks = new List<object>
        {
            GoodMetadata
        }
    };
}