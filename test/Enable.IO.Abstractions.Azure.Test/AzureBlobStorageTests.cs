using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Enable.IO.Abstractions.Test
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
        public Task CanCopyFileAsync()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task CopyFileAsync_ThrowsIfFileDoesNotExist()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task CanDeleteFileAsync()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task DeleteFileAsync_DoesNotThrowIfFileDoesNotExist()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task CanCheckExistsAsync()
        {
            throw new NotImplementedException();
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
        public Task GetFileInfoAsync()
        {
            throw new NotImplementedException();
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
        public Task CanGetFileListAsync()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task CanSearchFileListAsync()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task CanGetFileStreamAsync()
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
        public Task CanRenameFileAsync()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task RenameFileAsync_ThrowsIfFileDoesNotExist()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task CanSaveFileAsync()
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
