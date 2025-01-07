using BdziamPak.Git;
using BdziamPak.Structure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using BdziamPak.Packages.Packaging.Model;
using LibGit2Sharp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace BdziamPak.Tests
{
    public class GitServiceTests(ITestOutputHelper outputHelper) : BdziamTestBase(outputHelper)
    {
        [Fact]
        public void ExampleTest()
        {
            var gitService = _serviceProvider.GetRequiredService<GitService>();

            Assert.NotNull(gitService);
        }

        [Fact]
        public void CloneRepo_SuccessfulClone()
        {
            var targetDir = new DirectoryInfo(Path.Combine(_serviceProvider.GetRequiredService<BdziamPakDirectory>().PaksDirectory.FullName, "TestAuthor.testPak@1.0.0"));
            var metadata = _goodMetadata;
            // Act
            var gitService = _serviceProvider.GetRequiredService<GitService>();
            var result = gitService.CloneRepo(metadata);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(targetDir.FullName, result.FullName);
        }

        [Fact]
        public void CloneRepo_CommitNotFound()
        {
            // Act
            var gitService = _serviceProvider.GetRequiredService<GitService>();
            var metadata = _badMetadata;
            var exception = Assert.Throws<InvalidOperationException>(() => gitService.CloneRepo(metadata));

            // Assert
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public void CloneRepo_RepositoryIsNull()
        {
            
            var targetDir = new DirectoryInfo(Path.Combine(_serviceProvider.GetRequiredService<BdziamPakDirectory>().PaksDirectory.FullName, "TestAuthor.testPak@1.0.0"));

            // Act
            var gitService = _serviceProvider.GetRequiredService<GitService>();
            var result = gitService.CloneRepo(_goodMetadata);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(targetDir.FullName, result.FullName);
        }

        [Fact]
        public void CloneRepo_ErrorDuringCloning()
        {
            // Arrange
        
            var targetDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "testPak@1.0.0"));
            var metadata = _goodMetadata;
            // Simulate an error by providing an invalid repository URL
            metadata.Repository.Url = "https://invalid-url/repo.git";

            // Act
            var gitService = _serviceProvider.GetRequiredService<GitService>();
            var exception = Assert.Throws<LibGit2SharpException>(() => gitService.CloneRepo(metadata));

            // Assert
            Assert.Contains("failed to resolve", exception.Message);
        }
    }
}