using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.FileSystem.AzureStorage.Internal;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Enable.Extensions.FileSystem
{
    public class AzureBlobStorage : IFileSystem
    {
        private readonly CloudBlobContainer _container;

        public AzureBlobStorage(string connectionString, string containerName)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();

            _container = client.GetContainerReference(containerName);
        }

        public async Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync(cancellationToken);

            var sourceBlob = _container.GetBlockBlobReference(sourcePath);
            var targetBlob = _container.GetBlockBlobReference(targetPath);

            // The following only initiates a copy. There does not appear a way
            // to wait until the copy is complete without monitoring the copy
            // status of the target file.
            await targetBlob.StartCopyAsync(sourceBlob, cancellationToken);

            // However, for a file copy operation within the same storage
            // account, we can assume that the copy operation has completed
            // when `StartCopyAsync` completes. Here we check this assumption.
            if (targetBlob.CopyState.Status != CopyStatus.Success)
            {
                // TODO Consider if we can handle this case better.
                throw new NotSupportedException();
            }
        }

        public async Task DeleteFileAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync(cancellationToken);

            var blob = _container.GetBlockBlobReference(path);

            await blob.DeleteIfExistsAsync(cancellationToken);
        }

        public async Task<IDirectoryContents> GetDirectoryContentsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync(cancellationToken);

            var directory = _container.GetDirectoryReference(path);

            // TODO Do we need to fetch directory attributes?
            // See `directory.FetchAttributesAsync(cancellationToken)`.
            var directoryContents = new AzureBlobStorageDirectoryContents(directory);

            return directoryContents;
        }

        public async Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync(cancellationToken);

            var blob = _container.GetBlockBlobReference(path);

            try
            {
                await blob.FetchAttributesAsync(cancellationToken);

                var blobInfo = new AzureBlob(blob);

                return blobInfo;
            }
            catch (StorageException)
            {
                // TODO Here we should only be catching errors when the file is not found.
                return new NotFoundFile(path);
            }
        }

        public async Task<Stream> GetFileStreamAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync(cancellationToken);

            var blob = _container.GetBlockBlobReference(path);

            return await blob.OpenReadAsync(cancellationToken);
        }

        public async Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync(cancellationToken);

            var sourceBlob = _container.GetBlockBlobReference(sourcePath);

            // It is not currently possible to rename a blob in Azure. We
            // therefore attempt a copy and then delete. Note, however, that
            // `CopyFileAsync` may throw an exception if the copy is still
            // pending. It is therefore possible to end up with two blobs.
            await CopyFileAsync(sourcePath, targetPath, cancellationToken);
            await sourceBlob.DeleteIfExistsAsync(cancellationToken);
        }

        public async Task SaveFileAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync(cancellationToken);

            var blob = _container.GetBlockBlobReference(path);

            await blob.UploadFromStreamAsync(stream, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
