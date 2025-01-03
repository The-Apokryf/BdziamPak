using BdziamPak.Git;
using BdziamPak.Structure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using BdziamPak.Packages.Packaging.Model;
using Xunit;
using Xunit.Abstractions;

namespace BdziamPak.Tests
{
    public class GitServiceTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITestOutputHelper _outputHelper;
        private readonly ILogger<GitService> _logger;
        private readonly BdziamPakMetadata _metadata = TestData.BdziamPakMetadata;
        public GitServiceTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _logger = new TestLogger<GitService>(_outputHelper);
            _serviceProvider = TestServiceProvider.CreateServiceProvider(config =>
            {
                config.BdziamPakPath = ".testbdziampak";
            }, services => services.AddSingleton(_logger));
        }

        [Fact]
        public void ExampleTest()
        {
            var gitService = _serviceProvider.GetRequiredService<GitService>();

            Assert.NotNull(gitService);
        }

        [Fact]
        public void CloneRepo_SuccessfulClone()
        {
  

            var targetDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "testPak@1.0.0"));

            // Act
            var gitService = _serviceProvider.GetRequiredService<GitService>();
            var result = gitService.CloneRepo(_metadata);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(targetDir.FullName, result.FullName);
        }

        [Fact]
        public void CloneRepo_CommitNotFound()
        {
            // Act
            var gitService = _serviceProvider.GetRequiredService<GitService>();
            var exception = Assert.Throws<InvalidOperationException>(() => gitService.CloneRepo(_metadata));

            // Assert
            Assert.Contains("Commit nonexistent not found", exception.Message);
        }

        [Fact]
        public void CloneRepo_RepositoryIsNull()
        {

            var targetDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "testPak@1.0.0"));

            // Act
            var gitService = _serviceProvider.GetRequiredService<GitService>();
            var result = gitService.CloneRepo(_metadata);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(targetDir.FullName, result.FullName);
        }

        [Fact]
        public void CloneRepo_ErrorDuringCloning()
        {
            // Arrange
        
            var targetDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "testPak@1.0.0"));

            // Simulate an error by providing an invalid repository URL
            _metadata.Repository.Url = "https://invalid-url/repo.git";

            // Act
            var gitService = _serviceProvider.GetRequiredService<GitService>();
            var exception = Assert.Throws<Exception>(() => gitService.CloneRepo(_metadata));

            // Assert
            Assert.Contains("Error cloning repository", exception.Message);
        }
    }
}