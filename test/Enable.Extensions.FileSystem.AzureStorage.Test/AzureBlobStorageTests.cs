using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Enable.Extensions.FileSystem.Test
{
    /// <summary>
    /// Tests for the Azure Blob Storage file storage implementation.
    /// </summary>
    /// <remarks>
    /// These tests currently require the Azure Storage Emulator to be running.
    /// </remarks>
    public class AzureBlobStorageTests : IDisposable
    {
        private readonly AzureBlobStorage _sut;

        private bool _disposed;

        public AzureBlobStorageTests()
        {
            var connectionString = ConfigurationManager.AppSettings.Get("StorageConnectionString");

            _sut = new AzureBlobStorage(connectionString, "container");
        }

        [Fact]
        public Task CopyFileAsync_SucceedsIfSourceFileExists()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task CopyFileAsync_ThrowsIfFileDoesNotExist()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task DeleteFileAsync_SucceedsIfSourceFileExists()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task DeleteFileAsync_DoesNotThrowIfFileDoesNotExist()
        {
            throw new NotImplementedException();
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
        public Task GetDirectoryContentsAsync_ReturnsFileList()
        {
            throw new NotImplementedException();
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
        public Task GetFileInfoAsync_ReturnsFileInfoIfFileExists()
        {
            throw new NotImplementedException();
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
        public Task GetFileStreamAsync_ReturnsFileStreamIfFileExists()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task GetFileStreamAsync_ThrowsIfFileDoesNotExist()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task GetFileStreamAsync_ThrowsIfFileIsNotSpecified()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task RenameFileAsync_SucceedsIfSourceFileExists()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task RenameFileAsync_ThrowsIfFileDoesNotExist()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task SaveFileAsync_Succeeds()
        {
            throw new NotImplementedException();
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
                _sut.Dispose();

                _disposed = true;
            }
        }
    }
}
