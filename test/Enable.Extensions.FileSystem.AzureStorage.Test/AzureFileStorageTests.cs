using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Xunit;

namespace Enable.Extensions.FileSystem.Test
{
    /// <summary>
    /// Tests for the Azure File Storage file storage implementation.
    /// </summary>
    /// <remarks>
    /// These tests require a connection to an Azure Storage account, since the
    /// Azure Storage Emulator does not yet support Azure File Storage.
    /// </remarks>
    public class AzureFileStorageTests : IClassFixture<AzureStorageTestFixture>, IDisposable
    {
        private readonly CloudFileShare _fileShare;
        private readonly AzureFileStorage _sut;

        private bool _disposed;

        public AzureFileStorageTests(AzureStorageTestFixture fixture)
        {
            var storageAccount = fixture.StorageAccount;

            var storageClient = storageAccount.CreateCloudFileClient();

            var fileShareName = Guid.NewGuid().ToString();

            _fileShare = storageClient.GetShareReference(fileShareName);

            _fileShare.CreateIfNotExists();

            _sut = new AzureFileStorage(storageClient, fileShareName);
        }

        [Fact]
        public async Task CopyFileAsync_SucceedsIfSourceFileExists()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            await CreateTestFileAsync(_fileShare, source);

            // Act
            await _sut.CopyFileAsync(source, target);

            // Assert
            Assert.True(await ExistsAsync(_fileShare, source));
            Assert.True(await ExistsAsync(_fileShare, target));
        }

        [Fact]
        public async Task CopyFileAsync_ThrowsIfFileDoesNotExist()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            // Act
            var exception = await Record.ExceptionAsync(
                () => _sut.CopyFileAsync(source, target));

            // Assert
            Assert.IsAssignableFrom<StorageException>(exception);
        }

        [Fact]
        public async Task DeleteFileAsync_SucceedsIfFileExists()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            await CreateTestFileAsync(_fileShare, fileName);

            // Act
            await _sut.DeleteFileAsync(fileName);

            // Assert
            Assert.False(await ExistsAsync(_fileShare, fileName));
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

            await CreateTestFilesAsync(_fileShare, filesCount);

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

            await CreateTestFileAsync(_fileShare, fileName);

            // Act
            var result = await _sut.GetFileInfoAsync(fileName);

            // Assert
            Assert.True(result.Exists);
            Assert.False(result.IsDirectory);
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
        public async Task GetFGetFileStreamAsync_ReturnsFileStreamIfFileExistsileStreamAsync()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            var expectedContents = CreateRandomString();

            await CreateTestFileAsync(_fileShare, fileName, expectedContents);

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
            Assert.IsAssignableFrom<StorageException>(exception);
        }

        [Fact]
        public async Task GetFileStreamAsync_ThrowsIfFileIsNotSpecified()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            // Act
            var exception = await Record.ExceptionAsync(
                () => _sut.GetFileStreamAsync(fileName));

            // Assert
            Assert.IsAssignableFrom<StorageException>(exception);
        }

        [Fact]
        public async Task RenameFileAsync_SucceedsIfSourceFileExists()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            await CreateTestFileAsync(_fileShare, source);

            // Act
            await _sut.RenameFileAsync(source, target);

            // Assert
            Assert.False(await ExistsAsync(_fileShare, source));
            Assert.True(await ExistsAsync(_fileShare, target));
        }

        [Fact]
        public async Task RenameFileAsync_ThrowsIfFileDoesNotExist()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            // Act
            var exception = await Record.ExceptionAsync(
                () => _sut.RenameFileAsync(source, target));

            // Assert
            Assert.IsAssignableFrom<StorageException>(exception);
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
                    // Make a best effort to remove our temporary test share.
                    _fileShare.DeleteIfExists();
                }
                catch
                {
                }

                _sut.Dispose();

                _disposed = true;
            }
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

        private static Task CreateTestFilesAsync(CloudFileShare fileShare, int count)
        {
            var tasks = Enumerable.Range(0, count)
                .Select(o => CreateTestFileAsync(fileShare))
                .ToArray();

            return Task.WhenAll(tasks);
        }

        private static Task CreateTestFileAsync(CloudFileShare fileShare)
        {
            var fileName = Path.GetRandomFileName();

            return CreateTestFileAsync(fileShare, fileName);
        }

        private static Task CreateTestFileAsync(CloudFileShare fileShare, string fileName)
        {
            var contents = CreateRandomString();

            return CreateTestFileAsync(fileShare, fileName, contents);
        }

        private static Task CreateTestFileAsync(CloudFileShare fileShare, string fileName, string contents)
        {
            var rootDirectory = fileShare.GetRootDirectoryReference();

            var file = rootDirectory.GetFileReference(fileName);

            return file.UploadTextAsync(contents, Encoding.UTF8, null, null, null);
        }

        private static Task<bool> ExistsAsync(CloudFileShare fileShare, string fileName)
        {
            var rootDirectory = fileShare.GetRootDirectoryReference();

            var file = rootDirectory.GetFileReference(fileName);

            return file.ExistsAsync();
        }
    }
}
