using BdziamPak.PackageModel;

namespace BdziamPak.Tests;

/// <summary>
/// Provides test data for BdziamPak unit tests.
/// </summary>
public static class TestData
{
    /// <summary>
    /// Gets the metadata representing a good BdziamPak.
    /// </summary>
    public static object GoodMetadata => new
    {
        Name = "testPak",
        Author = "TestAuthor",
        Versions = new object[]
        {
            new
            {
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
            }
        }
    };

    /// <summary>
    /// Gets the metadata representing a bad BdziamPak.
    /// </summary>
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

    /// <summary>
    /// Gets the source index data.
    /// </summary>
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