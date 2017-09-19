using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Enable.IO.Abstractions
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

        public async Task<bool> ExistsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync(cancellationToken);

            var blob = _container.GetBlockBlobReference(path);

            return await blob.ExistsAsync(cancellationToken);
        }

        public async Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync(cancellationToken);

            var blob = _container.GetBlockBlobReference(path);

            await blob.FetchAttributesAsync(cancellationToken);

            var blobInfo = new AzureBlob(blob);

            return blobInfo;
        }

        public async Task<IEnumerable<IFile>> GetFileListAsync(
            string searchPattern = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync(cancellationToken);

            // TODO `searchPattern` is not used here.
            return new AzureBlobEnumerator(_container, cancellationToken);
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
