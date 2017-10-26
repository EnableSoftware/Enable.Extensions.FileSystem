using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.FileSystem.AzureStorage.Internal;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Enable.Extensions.FileSystem
{
    public class AzureBlobStorage : IFileSystem
    {
        private readonly CloudBlobContainer _container;

        public AzureBlobStorage(string accountName, string accountKey, string containerName)
        {
            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, useHttps: true);

            var client = storageAccount.CreateCloudBlobClient();

            _container = client.GetContainerReference(containerName);
        }

        public AzureBlobStorage(CloudBlobClient client, string containerName)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _container = client.GetContainerReference(containerName);
        }

        public async Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            var sourceBlob = _container.GetBlockBlobReference(sourcePath);
            var targetBlob = _container.GetBlockBlobReference(targetPath);

            // The following only initiates a copy. There does not appear a way
            // to wait until the copy is complete without monitoring the copy
            // status of the target file.
            await targetBlob.StartCopyAsync(sourceBlob);

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
            await _container.CreateIfNotExistsAsync();

            var blob = _container.GetBlockBlobReference(path);

            await blob.DeleteIfExistsAsync();
        }

        public async Task<IDirectoryContents> GetDirectoryContentsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            // The implementation of this method differs from the File Storage implementation.
            // With Blob Storage there is no concept of a "directory". Path segements in file names
            // are considered part of the file name, or a filename "prefix". There is therefore no
            // equivalent of `CloudFileDirectory.ExistsAsync()` on a `CloudBlobDirectory`. A
            // "directory" therefore only exists if there is a file whose name contains this
            // "directory" or "prefix". Here were therefore try a list files with the given prefix
            // and consider the directory "path" to exist if there is at least one file with this
            // prefix.
            var response = await _container.ListBlobsSegmentedAsync(
                prefix: path,
                useFlatBlobListing: true,
                blobListingDetails: BlobListingDetails.None,
                maxResults: 1,
                currentToken: null,
                options: null,
                operationContext: null,
                cancellationToken: cancellationToken);

            var directoryExists = response.Results.Any();

            if (directoryExists)
            {
                var directory = _container.GetDirectoryReference(path);

                var directoryContents = new AzureBlobStorageDirectoryContents(directory);

                return directoryContents;
            }

            return new NotFoundDirectoryContents(path);
        }

        public async Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            var blob = _container.GetBlockBlobReference(path);

            try
            {
                await blob.FetchAttributesAsync();

                var blobInfo = new AzureBlob(blob);

                return blobInfo;
            }
            catch (StorageException ex) when (StorageExceptionHelper.IsNotFoundStorageException(ex))
            {
                return new NotFoundFile(path);
            }
        }

        public async Task<Stream> GetFileStreamAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            var blob = _container.GetBlockBlobReference(path);

            return await blob.OpenReadAsync();
        }

        public async Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            var sourceBlob = _container.GetBlockBlobReference(sourcePath);

            // It is not currently possible to rename a blob in Azure. We
            // therefore attempt a copy and then delete. Note, however, that
            // `CopyFileAsync` may throw an exception if the copy is still
            // pending. It is therefore possible to end up with two blobs.
            await CopyFileAsync(sourcePath, targetPath, cancellationToken);
            await sourceBlob.DeleteIfExistsAsync();
        }

        public async Task SaveFileAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            var blob = _container.GetBlockBlobReference(path);

            await blob.UploadFromStreamAsync(stream);
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
