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
        private readonly CloudBlobClient _storageClient;
        private readonly string _containerName;

        private AzureBlobStorage _sut;
        private bool _disposed;

        public AzureBlobStorageTests(AzureStorageTestFixture fixture)
        {
            var storageAccount = fixture.StorageAccount;

            _storageClient = storageAccount.CreateCloudBlobClient();

            _containerName = Guid.NewGuid().ToString();

            _container = _storageClient.GetContainerReference(_containerName);

            _container.CreateIfNotExistsAsync()
                .GetAwaiter()
                .GetResult();

            _sut = new AzureBlobStorage(_storageClient, _containerName);
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task CopyFileAsync_SucceedsIfSourceFileExists(BlobType blobType)
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, source);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            await _sut.CopyFileAsync(source, target);

            // Assert
            Assert.True(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, source));
            Assert.True(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, target));
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task CopyFileAsync_CanMoveAcrossSubDirectories(BlobType blobType)
        {
            // Arrange
            var source = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());
            var target = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, source);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            await _sut.CopyFileAsync(source, target);

            // Assert
            Assert.True(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, source));
            Assert.True(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, target));
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

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task DeleteFileAsync_SucceedsIfFileExists(BlobType blobType)
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, fileName);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            await _sut.DeleteFileAsync(fileName);

            // Assert
            Assert.False(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, fileName));
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task DeleteDirectoryAsync_CanDeleteFromSubDirectory(BlobType blobType)
        {
            // Arrange
            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());
            var directoryName = Path.GetRandomFileName();

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, Path.Combine(directoryName, fileName));

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            await _sut.DeleteDirectoryAsync(directoryName);

            // Assert
            Assert.False(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, Path.Combine(directoryName, fileName)));
        }

        [Fact]
        public async Task DeleteDirectoryAsync_DoesNotThrowIfDirectoryDoesNotExist()
        {
            // Arrange
            var directoryName = Path.GetRandomFileName();

            // Act
            await _sut.DeleteDirectoryAsync(directoryName);
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task DeleteDirectoryAsync_SucceedsIfDirectoryExists(BlobType blobType)
        {
            // Arrange
            var directoryName = Path.GetRandomFileName();

            var numberOfFilesToCreate = AzureBlobStorageTestHelper.CreateRandomNumber(
                minValue: 1, // Ensure that we always create at least one test file.
                maxValue: byte.MaxValue);

            await AzureBlobStorageTestHelper.CreateTestFilesAsync(_container, blobType, numberOfFilesToCreate, directoryName);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            await _sut.DeleteDirectoryAsync(directoryName);

            // Assert
            Assert.False(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, directoryName));
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task DeleteFileAsync_CanDeleteFromSubDirectory(BlobType blobType)
        {
            // Arrange
            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, fileName);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            await _sut.DeleteFileAsync(fileName);

            // Assert
            Assert.False(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, fileName));
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

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task GetDirectoryContentsAsync_ReturnsFileList(BlobType blobType)
        {
            // Arrange
            var filesCount = AzureBlobStorageTestHelper.CreateRandomNumber(10);

            await AzureBlobStorageTestHelper.CreateTestFilesAsync(_container, blobType, filesCount);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            var result = await _sut.GetDirectoryContentsAsync(string.Empty);

            // Assert
            Assert.Equal(filesCount, result.Count());
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task GetDirectoryContentsAsync_ReturnsFileListForSubDirectory(BlobType blobType)
        {
            // Arrange
            var subpath = Path.GetRandomFileName();

            var filesCount = AzureBlobStorageTestHelper.CreateRandomNumber();

            await AzureBlobStorageTestHelper.CreateTestFilesAsync(
                _container,
                blobType,
                filesCount,
                subpath);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            var result = await _sut.GetDirectoryContentsAsync(subpath);

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

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task GetFileInfoAsync_ReturnsFileInfoIfFileExists(BlobType blobType)
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            var expectedFileName = Path.GetFileName(fileName);
            var expectedFilePath = fileName;

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, fileName);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            var result = await _sut.GetFileInfoAsync(fileName);

            // Assert
            Assert.True(result.Exists);
            Assert.False(result.IsDirectory);
            Assert.Equal(expectedFileName, result.Name);
            Assert.Equal(expectedFilePath, result.Path);
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task GetFileInfoAsync_ReturnsFileInfoForFileInSubDirectory(BlobType blobType)
        {
            // Arrange
            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            var expectedFileName = Path.GetFileName(fileName);
            var expectedFilePath = fileName;

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, fileName);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            var result = await _sut.GetFileInfoAsync(fileName);

            // Assert
            Assert.True(result.Exists);
            Assert.False(result.IsDirectory);
            Assert.Equal(expectedFileName, result.Name);
            Assert.Equal(expectedFilePath, result.Path);
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
            Assert.Equal(fileName, result.Name);
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task GetFileStreamAsync_ReturnsFileStreamIfFileExists(BlobType blobType)
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            var expectedContents = AzureBlobStorageTestHelper.CreateRandomString(blobType);

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, fileName, expectedContents);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            using (var stream = await _sut.GetFileStreamAsync(fileName))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var contents = reader.ReadToEnd();

                // Assert
                Assert.Equal(expectedContents, contents);
            }
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task GetFileStreamAsync_ReturnsFileStreamForFileInSubDirectory(BlobType blobType)
        {
            // Arrange
            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            var expectedContents = AzureBlobStorageTestHelper.CreateRandomString(blobType);

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, fileName, expectedContents);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
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

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task RenameFileAsync_SucceedsIfSourceFileExists(BlobType blobType)
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, source);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            await _sut.RenameFileAsync(source, target);

            // Assert
            Assert.False(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, source));
            Assert.True(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, target));
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task RenameFileAsync_CanRenameAcrossSubDirectories(BlobType blobType)
        {
            // Arrange
            var source = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());
            var target = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            await AzureBlobStorageTestHelper.CreateTestFileAsync(_container, blobType, source);

            // Act
            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());
            await _sut.RenameFileAsync(source, target);

            // Assert
            Assert.False(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, source));
            Assert.True(await AzureBlobStorageTestHelper.ExistsAsync(_container, blobType, target));
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
            var fileName = Path.GetRandomFileName();

            var contents = AzureBlobStorageTestHelper.CreateRandomString();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            {
                // Act
                await _sut.SaveFileAsync(fileName, stream);
            }
        }

        [Fact]
        public async Task SaveFileAsync_SucceedsForFileInSubDirectory()
        {
            // Arrange
            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            var contents = AzureBlobStorageTestHelper.CreateRandomString();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            {
                // Act
                await _sut.SaveFileAsync(fileName, stream);
            }
        }

        [InlineData(BlobType.BlockBlob)]
        [InlineData(BlobType.PageBlob)]
        [InlineData(BlobType.AppendBlob)]
        [Theory]
        public async Task SaveFileAsync_SavesCorrectContent(BlobType blobType)
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            var expectedContents = AzureBlobStorageTestHelper.CreateRandomString(blobType);
            var byteArray = Encoding.UTF8.GetBytes(expectedContents);

            _sut = new AzureBlobStorage(_storageClient, _containerName, blobType.ToString());

            using (var stream = new MemoryStream(byteArray))
            {
                // Act
                await _sut.SaveFileAsync(fileName, stream);
            }

            // Assert
            var actualContents = await AzureBlobStorageTestHelper.ReadFileContents(_container, blobType, fileName);

            Assert.Equal(expectedContents, actualContents);
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
                    _container.DeleteIfExistsAsync()
                        .GetAwaiter()
                        .GetResult();
                }
                catch
                {
                }

                _sut.Dispose();

                _disposed = true;
            }
        }
    }
}
