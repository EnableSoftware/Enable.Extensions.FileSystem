using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Enable.Extensions.FileSystem.Test
{
    /// <summary>
    /// Tests for the Azure File Storage file storage implementation.
    /// </summary>
    /// <remarks>
    /// These tests are not currently implemented, since the Azure Storage Emulator does not yet
    /// support Azure File Storage.
    /// </remarks>
    public class AzureFileStorageTests : IDisposable
    {
        private readonly AzureFileStorage _sut;

        private bool _disposed;

        public AzureFileStorageTests()
        {
            var connectionString = ConfigurationManager.AppSettings.Get("StorageConnectionString");

            _sut = new AzureFileStorage(connectionString, "share", "directory");
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
        public Task DeleteFileAsync_SucceedsIfFileExists()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task DeleteFileAsync_DoesNotThrowIfFileDoesNotExist()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task GetDirectoryContentsAsync_ReturnsEmptyListForEmptyDirectory()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task GetDirectoryContentsAsync_ReturnsFileList()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task GetDirectoryContentsAsync_ReturnsNotFoundDirectoryIfDirectoryDoesNotExist()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task GetFileInfoAsync_ReturnsFileInfoIfFileExists()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task GetFileInfoAsync_ReturnsNotFoundFileIfFileDoesNotExist()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task GetFGetFileStreamAsync_ReturnsFileStreamIfFileExistsileStreamAsync()
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
