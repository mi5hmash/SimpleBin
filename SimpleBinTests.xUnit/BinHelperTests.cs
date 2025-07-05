using Microsoft.VisualBasic.FileIO;
using SimpleBin;
using Xunit.Abstractions;

namespace SimpleBinTests.xUnit
{
    public sealed class BinHelperTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public BinHelperTests(ITestOutputHelper output)
        {
            _output = output;
            _output.WriteLine("SETUP");
        }

        public void Dispose()
        {
            _output.WriteLine("CLEANUP");
        }

        [Fact]
        public void ClearBin_ShouldReturnFalse()
        {
            // Arrange
            var testFilePath = Path.Combine(Path.GetTempPath(), "testfile.txt");
            File.WriteAllText(testFilePath, "This is a test file.");
            FileSystem.DeleteFile(
                testFilePath,
                UIOption.OnlyErrorDialogs,
                RecycleOption.SendToRecycleBin
            );
            var _binHelper = new BinHelper();

            // Act
            _ = BinHelper.ClearBin();
            var result = _binHelper.IsBinEmpty();
            _binHelper.Dispose(); // Clean up after test

            // Assert
            Assert.True(result, "Bin should be empty after clearing.");
        }

        [Theory]
        [InlineData(1, 20, "filename1")]
        [InlineData(2, 40, "filename2")]
        public void GetBinSize_ShouldBeEqualToExpected(int expectedElements, int expectedSize, string filename)
        {
            // Arrange
            _ = BinHelper.ClearBin();
            for (int i = 0; i < expectedElements; i++)
            {
                var testFilePath = Path.Combine(Path.GetTempPath(), $"{filename}.txt");
                File.WriteAllText(testFilePath, $"This is a test file.");
                FileSystem.DeleteFile(
                    testFilePath,
                    UIOption.OnlyErrorDialogs,
                    RecycleOption.SendToRecycleBin
                );
            }
            var _binHelper = new BinHelper();

            // Act
            var (biteSize, itemCount) = _binHelper.GetBinSize();
            _output.WriteLine($"Bin size: {biteSize} bytes, Items: {itemCount}");
            _binHelper.Dispose(); // Clean up after test

            // Assert
            Assert.Equal(expectedElements, itemCount);
            Assert.Equal(expectedSize, biteSize);
        }
    }
}
