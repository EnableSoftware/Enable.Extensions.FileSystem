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

            _container.CreateIfNotExistsAsync()
                .GetAwaiter()
                .GetResult();

            _sut = new AzureBlobStorage(storageClient, containerName);
        }

        [Fact]
        public async Task CopyFileAsync_SucceedsIfSourceFileExists()
        {
            // Arrange
            var source = Path.GetRandomFileName();
            var target = Path.GetRandomFileName();

            await AzureStorageTestHelper.CreateTestFileAsync(_container, source);

            // Act
            await _sut.CopyFileAsync(source, target);

            // Assert
            Assert.True(await AzureStorageTestHelper.ExistsAsync(_container, source));
            Assert.True(await AzureStorageTestHelper.ExistsAsync(_container, target));
        }

        [Fact]
        public async Task CopyFileAsync_CanMoveAcrossSubDirectories()
        {
            // Arrange
            var source = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());
            var target = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            await AzureStorageTestHelper.CreateTestFileAsync(_container, source);

            // Act
            await _sut.CopyFileAsync(source, target);

            // Assert
            Assert.True(await AzureStorageTestHelper.ExistsAsync(_container, source));
            Assert.True(await AzureStorageTestHelper.ExistsAsync(_container, target));
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

            await AzureStorageTestHelper.CreateTestFileAsync(_container, fileName);

            // Act
            await _sut.DeleteFileAsync(fileName);

            // Assert
            Assert.False(await AzureStorageTestHelper.ExistsAsync(_container, fileName));
        }

        [Fact]
        public async Task DeleteDirectoryAsync_CanDeleteFromSubDirectory()
        {
            // Arrange
            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());
            var directoryName = Path.GetRandomFileName();

            await AzureStorageTestHelper.CreateTestFileAsync(_container, Path.Combine(directoryName, fileName));

            // Act
            await _sut.DeleteDirectoryAsync(directoryName);

            // Assert
            Assert.False(await AzureStorageTestHelper.ExistsAsync(_container, Path.Combine(directoryName, fileName)));
        }

        [Fact]
        public async Task DeleteDirectoryAsync_DoesNotThrowIfDirectoryDoesNotExist()
        {
            // Arrange
            var directoryName = Path.GetRandomFileName();

            // Act
            await _sut.DeleteDirectoryAsync(directoryName);
        }

        [Fact]
        public async Task DeleteDirectoryAsync_SucceedsIfDirectoryExists()
        {
            // Arrange
            var fileNames = Enumerable.Range(0, AzureStorageTestHelper.CreateRandomNumber())
                .Select(o => Path.GetRandomFileName())
                .ToArray();

            var directoryName = Path.GetRandomFileName();

            await AzureStorageTestHelper.CreateTestFilesAsync(_container, fileNames, directoryName);

            // Act
            await _sut.DeleteDirectoryAsync(directoryName);

            // Assert
            var filesRemaining = 0;
            foreach (var fileName in fileNames)
            {
                if (await AzureStorageTestHelper.ExistsAsync(_container, Path.Combine(directoryName, fileName)))
                {
                    filesRemaining++;
                }
            }

            Assert.Equal(0, filesRemaining);
        }

        [Fact]
        public async Task DeleteFileAsync_CanDeleteFromSubDirectory()
        {
            // Arrange
            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            await AzureStorageTestHelper.CreateTestFileAsync(_container, fileName);

            // Act
            await _sut.DeleteFileAsync(fileName);

            // Assert
            Assert.False(await AzureStorageTestHelper.ExistsAsync(_container, fileName));
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
            var filesCount = AzureStorageTestHelper.CreateRandomNumber();

            await AzureStorageTestHelper.CreateTestFilesAsync(_container, filesCount);

            // Act
            var result = await _sut.GetDirectoryContentsAsync(string.Empty);

            // Assert
            Assert.Equal(filesCount, result.Count());
        }

        [Fact]
        public async Task GetDirectoryContentsAsync_ReturnsFileListForSubDirectory()
        {
            // Arrange
            var subpath = Path.GetRandomFileName();

            var filesCount = AzureStorageTestHelper.CreateRandomNumber();

            await AzureStorageTestHelper.CreateTestFilesAsync(
                _container,
                filesCount,
                subpath);

            // Act
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

        [Fact]
        public async Task GetFileInfoAsync_ReturnsFileInfoIfFileExists()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            var expectedFileName = Path.GetFileName(fileName);
            var expectedFilePath = fileName;

            await AzureStorageTestHelper.CreateTestFileAsync(_container, fileName);

            // Act
            var result = await _sut.GetFileInfoAsync(fileName);

            // Assert
            Assert.True(result.Exists);
            Assert.False(result.IsDirectory);
            Assert.Equal(expectedFileName, result.Name);
            Assert.Equal(expectedFilePath, result.Path);
        }

        [Fact]
        public async Task GetFileInfoAsync_ReturnsFileInfoForFileInSubDirectory()
        {
            // Arrange
            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            var expectedFileName = Path.GetFileName(fileName);
            var expectedFilePath = fileName;

            await AzureStorageTestHelper.CreateTestFileAsync(_container, fileName);

            // Act
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

        [Fact]
        public async Task GetFileStreamAsync_ReturnsFileStreamIfFileExists()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            var expectedContents = AzureStorageTestHelper.CreateRandomString();

            await AzureStorageTestHelper.CreateTestFileAsync(_container, fileName, expectedContents);

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
        public async Task GetFileStreamAsync_ReturnsFileStreamForFileInSubDirectory()
        {
            // Arrange
            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            var expectedContents = AzureStorageTestHelper.CreateRandomString();

            await AzureStorageTestHelper.CreateTestFileAsync(_container, fileName, expectedContents);

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

            await AzureStorageTestHelper.CreateTestFileAsync(_container, source);

            // Act
            await _sut.RenameFileAsync(source, target);

            // Assert
            Assert.False(await AzureStorageTestHelper.ExistsAsync(_container, source));
            Assert.True(await AzureStorageTestHelper.ExistsAsync(_container, target));
        }

        [Fact]
        public async Task RenameFileAsync_CanRenameAcrossSubDirectories()
        {
            // Arrange
            var source = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());
            var target = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

            await AzureStorageTestHelper.CreateTestFileAsync(_container, source);

            // Act
            await _sut.RenameFileAsync(source, target);

            // Assert
            Assert.False(await AzureStorageTestHelper.ExistsAsync(_container, source));
            Assert.True(await AzureStorageTestHelper.ExistsAsync(_container, target));
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

            var contents = AzureStorageTestHelper.CreateRandomString();

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

            var contents = AzureStorageTestHelper.CreateRandomString();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            {
                // Act
                await _sut.SaveFileAsync(fileName, stream);
            }
        }

        [Fact]
        public async Task SaveFileAsync_SavesCorrectContent()
        {
            // Arrange
            var fileName = Path.GetRandomFileName();

            var expectedContents = AzureStorageTestHelper.CreateRandomString();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContents)))
            {
                // Act
                await _sut.SaveFileAsync(fileName, stream);
            }

            // Assert
            var actualContents = await AzureStorageTestHelper.ReadFileContents(_container, fileName);

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
