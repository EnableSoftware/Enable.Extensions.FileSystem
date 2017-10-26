using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Xunit;

namespace Enable.Extensions.FileSystem.Test
{
    /// <summary>
    /// Tests for the Azure Blob Storage file storage implementation.
    /// </summary>
    /// <remarks>
    /// These tests require a connection to an Azure Storage account, or
    /// the Azure Storage Emulator to be running if running a debug build.
    /// </remarks>
    public class AzureBlobStorageTests : IClassFixture<AzureStorageTestFixture>, IDisposable
    {
        private readonly CloudBlobContainer _container;
        private readonly AzureBlobStorage _sut;

        private bool _disposed;

        public AzureBlobStorageTests(AzureStorageTestFixture fixture)
        {
            var storageAccount = fixture.StorageAccount;

            var storageClient = storageAccount.CreateCloudBlobClient();

            var containerName = Guid.NewGuid().ToString();

            _container = storageClient.GetContainerReference(containerName);

            _container.CreateIfNotExists();

            _sut = new AzureBlobStorage(storageClient, containerName);
        }

        [Fact]
        public async Task CopyFileAsync_SucceedsIfSourceFileExists()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            await CreateTestFileAsync(_container, source);

            // Act
            await _sut.CopyFileAsync(source, target);

            // Assert
            Assert.True(await ExistsAsync(_container, source));
            Assert.True(await ExistsAsync(_container, target));
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

            await CreateTestFileAsync(_container, fileName);

            // Act
            await _sut.DeleteFileAsync(fileName);

            // Assert
            Assert.False(await ExistsAsync(_container, fileName));
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

            await CreateTestFilesAsync(_container, filesCount);

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

            await CreateTestFileAsync(_container, fileName);

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
        public async Task GetFileStreamAsync_ReturnsFileStreamIfFileExists()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            var expectedContents = CreateRandomString();

            await CreateTestFileAsync(_container, fileName, expectedContents);

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

            await CreateTestFileAsync(_container, source);

            // Act
            await _sut.RenameFileAsync(source, target);

            // Assert
            Assert.False(await ExistsAsync(_container, source));
            Assert.True(await ExistsAsync(_container, target));
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
                    // Make a best effort to remove our temporary test container.
                    _container.DeleteIfExists();
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

        private static Task CreateTestFilesAsync(CloudBlobContainer container, int count)
        {
            var tasks = Enumerable.Range(0, count)
                .Select(o => CreateTestFileAsync(container))
                .ToArray();

            return Task.WhenAll(tasks);
        }

        private static Task CreateTestFileAsync(CloudBlobContainer container)
        {
            var blobName = Path.GetRandomFileName();

            return CreateTestFileAsync(container, blobName);
        }

        private static Task CreateTestFileAsync(CloudBlobContainer container, string blobName)
        {
            var contents = CreateRandomString();

            return CreateTestFileAsync(container, blobName, contents);
        }

        private static Task CreateTestFileAsync(CloudBlobContainer container, string blobName, string contents)
        {
            var blob = container.GetBlockBlobReference(blobName);

            return blob.UploadTextAsync(contents, Encoding.UTF8, null, null, null);
        }

        private static Task<bool> ExistsAsync(CloudBlobContainer container, string blobName)
        {
            var blob = container.GetBlockBlobReference(blobName);

            return blob.ExistsAsync();
        }
    }
}
