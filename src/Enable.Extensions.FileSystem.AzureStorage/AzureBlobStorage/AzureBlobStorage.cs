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
    public class AzureBlobStorage : BaseFileSystem
    {
        private readonly CloudBlobContainer _container;
        private readonly BlobType _blobType;

        public AzureBlobStorage(string accountName, string accountKey, string containerName, string blobType = "BlockBlob")
        {
            var blobTypeEnum = SetBlobType(blobType);

            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, useHttps: true);

            var client = storageAccount.CreateCloudBlobClient();

            _container = client.GetContainerReference(containerName);
            _blobType = blobTypeEnum;
        }

        public AzureBlobStorage(CloudBlobClient client, string containerName, string blobType = "BlockBlob")
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var blobTypeEnum = SetBlobType(blobType);

            _container = client.GetContainerReference(containerName);
            _blobType = blobTypeEnum;
        }

        public override async Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            CloudBlob sourceBlob;
            CloudBlob targetBlob;

            // The following only initiates a copy. There does not appear a way
            // to wait until the copy is complete without monitoring the copy
            // status of the target file.
            switch (_blobType)
            {
                default:
                case BlobType.BlockBlob:
                    sourceBlob = _container.GetBlockBlobReference(sourcePath);
                    targetBlob = _container.GetBlockBlobReference(targetPath);

                    await ((CloudBlockBlob)targetBlob).StartCopyAsync((CloudBlockBlob)sourceBlob);
                    break;
                case BlobType.AppendBlob:
                    sourceBlob = _container.GetAppendBlobReference(sourcePath);
                    targetBlob = _container.GetAppendBlobReference(targetPath);

                    await ((CloudAppendBlob)targetBlob).StartCopyAsync((CloudAppendBlob)sourceBlob);
                    break;
                case BlobType.PageBlob:
                    sourceBlob = _container.GetPageBlobReference(sourcePath);
                    targetBlob = _container.GetPageBlobReference(targetPath);

                    await ((CloudPageBlob)targetBlob).StartCopyAsync((CloudPageBlob)sourceBlob);
                    break;
            }

            if (targetBlob.CopyState.Status != CopyStatus.Success)
            {
                throw new NotSupportedException();
            }

            // However, for a file copy operation within the same storage
            // account, we can assume that the copy operation has completed
            // when `StartCopyAsync` completes. Here we check this assumption.
            if (targetBlob.CopyState.Status != CopyStatus.Success)
            {
                // TODO Consider if we can handle this case better.
                throw new NotSupportedException();
            }
        }

        public override async Task DeleteDirectoryAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directory = await GetDirectoryContentsAsync(path);

            if (directory.Exists)
            {
                foreach (var item in directory)
                {
                    if (item.IsDirectory)
                    {
                        await DeleteDirectoryAsync(item.Path);
                    }
                    else
                    {
                        await DeleteFileAsync(item.Path);
                    }
                }
            }
        }

        public override async Task DeleteFileAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            var blob = GetBlobReference(path);

            await blob.DeleteIfExistsAsync();
        }

        public override async Task<IDirectoryContents> GetDirectoryContentsAsync(
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

        public override async Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            var blob = GetBlobReference(path);

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

        public override async Task<Stream> GetFileStreamAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            var blob = GetBlobReference(path);

            return await blob.OpenReadAsync();
        }

        public override async Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            var sourceBlob = GetBlobReference(sourcePath);

            // It is not currently possible to rename a blob in Azure. We
            // therefore attempt a copy and then delete. Note, however, that
            // `CopyFileAsync` may throw an exception if the copy is still
            // pending. It is therefore possible to end up with two blobs.
            await CopyFileAsync(sourcePath, targetPath, cancellationToken);
            await sourceBlob.DeleteIfExistsAsync();
        }

        public override async Task SaveFileAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            switch (_blobType)
            {
                default:
                case BlobType.BlockBlob:
                    var blockBlob = _container.GetBlockBlobReference(path);
                    await blockBlob.UploadFromStreamAsync(stream);
                    return;
                case BlobType.AppendBlob:
                    var appendBlob = _container.GetAppendBlobReference(path);
                    await appendBlob.UploadFromStreamAsync(stream);
                    return;
                case BlobType.PageBlob:
                    var pageBlob = _container.GetPageBlobReference(path);
                    await pageBlob.UploadFromStreamAsync(stream);
                    return;
            }
        }

        private CloudBlob GetBlobReference(string path)
        {
            switch (_blobType)
            {
                default:
                case BlobType.BlockBlob:
                    return _container.GetBlockBlobReference(path);
                case BlobType.AppendBlob:
                    return _container.GetAppendBlobReference(path);
                case BlobType.PageBlob:
                    return _container.GetPageBlobReference(path);
            }
        }

        private BlobType SetBlobType(string blobType)
        {
            BlobType blobTypeEnum;
            if (!Enum.TryParse<BlobType>(blobType, out blobTypeEnum)
                || blobTypeEnum == BlobType.Unspecified)
            {
                throw new ArgumentException("Invalid blobType specified. Permitted values: PageBlob, BlockBlob, AppendBlob");
            }

            return blobTypeEnum;
        }
    }
}
