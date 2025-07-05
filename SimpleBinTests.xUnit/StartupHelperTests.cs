using Xunit.Abstractions;
using SimpleBin;

namespace SimpleBinTests.xUnit
{
    public sealed class StartupHelperTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public StartupHelperTests(ITestOutputHelper output)
        {
            _output = output;
            _output.WriteLine("SETUP");
        }

        public void Dispose()
        {
            _output.WriteLine("CLEANUP");
        }

        [Fact]
        public void RemoveFromStartup_ShouldReturnFalse()
        {
            // Arrange
            StartupHelper.RemoveFromStartup();

            // Act
            var result = StartupHelper.IsInStartup();

            // Assert
            Assert.False(result, "Startup entry should not exist after removal.");
        }

        [Fact]
        public void AddToStartup_ShouldReturnFalse()
        {
            // Arrange
            StartupHelper.AddToStartup();

            // Act
            var result = StartupHelper.IsInStartup();
            StartupHelper.RemoveFromStartup(); // Clean up after test

            // Assert
            Assert.True(result, "Startup entry should exist after creation.");
        }
    }
}
