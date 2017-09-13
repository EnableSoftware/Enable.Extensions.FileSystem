using System;
using System.Threading.Tasks;
using Xunit;

namespace Enable.IO.Abstractions.Test
{
    public class AzureBlobStorageTests : IDisposable
    {
        private readonly AzureBlobStorage _sut;

        private bool _disposed;

        public AzureBlobStorageTests()
        {
            _sut = new AzureBlobStorage(string.Empty, string.Empty);
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
        public Task ExistsAsync_ReturnsFalseIfFileDoesNotExist()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task GetFileInfoAsync()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public Task GetFileListAsync_ReturnsEmptyList()
        {
            throw new NotImplementedException();
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
