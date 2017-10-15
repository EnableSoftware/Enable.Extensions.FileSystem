using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Enable.IO.Abstractions.Test
{
    public class FileSystemTests : IDisposable
    {
        private readonly string _directory;

        private readonly FileSystem _sut;

        private bool _disposed;

        public FileSystemTests()
        {
            _directory = CreateTestDirectory();

            _sut = new FileSystem(_directory);
        }

        [Fact]
        public async Task CopyFileAsync_SucceedsIfSourceFileExists()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            CreateTestFile(_directory, source);

            // Act
            await _sut.CopyFileAsync(source, target);

            // Assert
            Assert.True(await ExistsAsync(source));
            Assert.True(await ExistsAsync(target));
        }

        [Fact]
        public async Task CopyFileAsync_ThrowsIfSourceFileDoesNotExist()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            // Act
            var exception = await Record.ExceptionAsync(() => _sut.CopyFileAsync(source, target));

            // Assert
            Assert.IsAssignableFrom<FileNotFoundException>(exception);
        }

        [Fact]
        public async Task DeleteFileAsync_SucceedsIfFileExists()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            CreateTestFile(_directory, fileName);

            // Act
            await _sut.DeleteFileAsync(fileName);

            // Assert
            Assert.False(await ExistsAsync(fileName));
        }

        [Fact]
        public async Task DeleteFileAsync_DoesNotThrowIfFileDoesNotExist()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            // Act
            await _sut.DeleteFileAsync(fileName);
        }

        [Fact]
        public async Task GetDirectoryContentsAsync_ReturnsEmptyListForEmptyDirectory()
        {
            // Act
            var result = await _sut.GetDirectoryContentsAsync(string.Empty);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDirectoryContentsAsync_ReturnsFileList()
        {
            // Arrange
            var filesCount = CreateRandomNumber();
            CreateTestFiles(_directory, filesCount);

            // Act
            var result = await _sut.GetDirectoryContentsAsync(string.Empty);

            // Assert
            Assert.Equal(filesCount, result.Count());
        }

        [Fact]
        public async Task GetDirectoryContentsAsync_ReturnsNotFoundDirectoryIfDirectoryDoesNotExist()
        {
            // Arrange
            var path = Path.GetRandomFileName();

            // Act
            var result = await _sut.GetDirectoryContentsAsync(path);

            // Assert
            Assert.False(result.Exists);
        }

        [Fact]
        public async Task GetFileInfoAsync_ReturnsFileInfoIfFileExists()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            CreateTestFile(_directory, fileName);

            var expectedFileInfo = new FileInfo(Path.Combine(_directory, fileName));

            // Act
            var result = await _sut.GetFileInfoAsync(fileName);

            // Assert
            Assert.True(result.Exists);
            Assert.False(result.IsDirectory);
            Assert.Equal(expectedFileInfo.LastWriteTimeUtc, result.LastModified);
            Assert.Equal(expectedFileInfo.Length, result.Length);
            Assert.Equal(expectedFileInfo.Name, result.Name);
            Assert.Equal(expectedFileInfo.FullName, result.Path);
        }

        [Fact]
        public async Task GetFileInfoAsync_ReturnsNotFoundFileIfFileDoesNotExist()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            // Act
            var result = await _sut.GetFileInfoAsync(fileName);

            // Assert
            Assert.False(result.Exists);
        }

        [Fact]
        public async Task GetFileStreamAsync_ReturnsFileStreamIfFileExists()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            var expectedContents = CreateRandomString();

            CreateTestFile(_directory, fileName, expectedContents);

            // Act
            using (var stream = await _sut.GetFileStreamAsync(fileName))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var contents = reader.ReadToEnd();

                // Assert
                Assert.Equal(expectedContents, contents);
            }
        }

        [Fact]
        public async Task GetFileStreamAsync_ThrowsIfFileDoesNotExist()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            // Act
            var exception = await Record.ExceptionAsync(() => _sut.GetFileStreamAsync(fileName));

            // Assert
            Assert.IsAssignableFrom<FileNotFoundException>(exception);
        }

        [Fact]
        public async Task GetFileStreamAsync_ThrowsIfFileIsNotSpecified()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            // Act
            var exception = await Record.ExceptionAsync(() => _sut.GetFileStreamAsync(fileName));

            // Assert
            Assert.IsAssignableFrom<FileNotFoundException>(exception);
        }

        [Fact]
        public async Task RenameFileAsync_SucceedsIfSourceFileExists()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            CreateTestFile(_directory, source);

            // Act
            await _sut.RenameFileAsync(source, target);

            // Assert
            Assert.False(await ExistsAsync(source));
            Assert.True(await ExistsAsync(target));
        }

        [Fact]
        public async Task RenameFileAsync_ThrowsIfFileDoesNotExist()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            // Act
            var exception = await Record.ExceptionAsync(() => _sut.RenameFileAsync(source, target));

            // Assert
            Assert.IsAssignableFrom<FileNotFoundException>(exception);
        }

        [Fact]
        public async Task SaveFileAsync_Succeeds()
        {
            // Arrange
            var contents = CreateRandomString();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            {
                var fileName = Path.GetRandomFileName();

                // Act
                await _sut.SaveFileAsync(fileName, stream);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                try
                {
                    // Make a best effort to remove our temporary test directory.
                    Directory.Delete(_directory, recursive: true);
                }
                catch
                {
                }

                _sut.Dispose();

                _disposed = true;
            }
        }

        private static string CreateTestDirectory()
        {
            var tempdirectory = Path.GetTempPath();
            var directoryName = Path.GetRandomFileName();

            var directory = Path.GetFullPath(Path.Combine(tempdirectory, directoryName));

            Directory.CreateDirectory(directory);

            return directory;
        }

        private static string CreateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        private static int CreateRandomNumber()
        {
            var rng = new Random();
            return rng.Next(byte.MaxValue);
        }

        private static void CreateTestFiles(string directory, int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateTestFile(directory);
            }
        }

        private static void CreateTestFile(string directory)
        {
            var fileName = Path.GetRandomFileName();

            CreateTestFile(directory, fileName);
        }

        private static void CreateTestFile(string directory, string fileName)
        {
            var contents = CreateRandomString();

            CreateTestFile(directory, fileName, contents);
        }

        private static void CreateTestFile(string directory, string fileName, string contents)
        {
            var path = Path.Combine(directory, fileName);

            File.WriteAllText(path, contents);
        }

        private async Task<bool> ExistsAsync(string path)
        {
            var fileInfo = await _sut.GetFileInfoAsync(path);

            return fileInfo.Exists;
        }
    }
}
