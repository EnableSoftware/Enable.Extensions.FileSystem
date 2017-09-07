using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Enable.IO.Abstractions;
using Xunit;

namespace Enable.Transactions
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
        public async Task CanCopyFileAsync()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            CreateTestFile(_directory, source);

            // Act
            await _sut.CopyFileAsync(source, target);

            // Assert
            Assert.True(await _sut.ExistsAsync(source));
            Assert.True(await _sut.ExistsAsync(target));
        }

        [Fact]
        public async Task CopyFileAsync_ThrowsIfFileDoesNotExist()
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
        public async Task CanDeleteFileAsync()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            CreateTestFile(_directory, fileName);

            // Act
            await _sut.DeleteFileAsync(fileName);

            // Assert
            Assert.False(await _sut.ExistsAsync(fileName));
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
        public async Task CanCheckExistsAsync()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            CreateTestFile(_directory, fileName);

            // Act
            var result = await _sut.ExistsAsync(fileName);

            // Asert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalseIfFileDoesNotExist()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            // Act
            var result = await _sut.ExistsAsync(fileName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetFileInfoAsync()
        {
            // TODO
            await _sut.GetFileInfoAsync(string.Empty);
        }

        [Fact]
        public async Task GetFileListAsync_ReturnsEmptyList()
        {
            // Act
            var result = await _sut.GetFileListAsync("*");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task CanGetFileListAsync()
        {
            // Arrange
            // TODO Generate random number of files.
            var fileName = Path.GetRandomFileName();

            CreateTestFile(_directory, fileName);

            // Act
            var result = await _sut.GetFileListAsync("*");

            // Assert
            Assert.Collection(
                result,
                (file) => Assert.EndsWith(fileName, file.Path));
        }

        [Fact]
        public async Task CanSearchFileListAsync()
        {
            // Arrange
            // TODO Generate random number of files.
            var fileName = Path.GetRandomFileName();

            CreateTestFile(_directory, fileName);

            var searchTerm = string.Empty;

            // Act
            var result = await _sut.GetFileListAsync(searchTerm);

            // Assert
            Assert.Collection(
                result,
                (file) => Assert.EndsWith(fileName, file.Path));
        }

        [Fact]
        public async Task CanGetFileStreamAsync()
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
        public async Task CanRenameFileAsync()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            CreateTestFile(_directory, source);

            // Act
            await _sut.RenameFileAsync(source, target);

            // Assert
            Assert.False(await _sut.ExistsAsync(source));
            Assert.True(await _sut.ExistsAsync(target));
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
        public async Task CanSaveFileAsync()
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
    }
}
