using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Enable.Extensions.FileSystem.AzureStorage.Internal;

namespace Enable.Extensions.FileSystem
{
    public class AzureBlobStorage : BaseFileSystem
    {
        private readonly BlobContainerClient _container;
        private readonly BlobType _blobType;

        public AzureBlobStorage(
            string connectionString,
            string containerName,
            BlobType blobType)
        {
            _container = new BlobContainerClient(connectionString, containerName);
            _blobType = blobType;
        }

        public AzureBlobStorage(
            string connectionString,
            string containerName)
            : this(connectionString, containerName, BlobType.Block)
        {
        }

        public AzureBlobStorage(
            BlobContainerClient client,
            BlobType blobType)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _container = client;
            _blobType = blobType;
        }

        public override async Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _container.CreateIfNotExistsAsync();

            var sourceBlob = GetBlobReference(sourcePath);
            var targetBlob = GetBlobReference(targetPath);

            CopyFromUriOperation copyFromUriOperation;

            // The following only initiates a copy. There does not appear a way
            // to wait until the copy is complete without monitoring the copy
            // status of the target file.
            switch (_blobType)
            {
                case BlobType.Block:
                    copyFromUriOperation = await ((BlockBlobClient)targetBlob).StartCopyFromUriAsync(sourceBlob.Uri);
                    break;

                case BlobType.Append:
                    copyFromUriOperation = await ((AppendBlobClient)targetBlob).StartCopyFromUriAsync(sourceBlob.Uri);
                    break;

                case BlobType.Page:
                    copyFromUriOperation = await ((PageBlobClient)targetBlob).StartCopyFromUriAsync(sourceBlob.Uri);
                    break;

                default:
                    // This shouldn't happen unless Azure Storage introduces a new blob type.
                    throw new NotSupportedException("Unexpected blob type.");
            }

            await copyFromUriOperation.WaitForCompletionAsync();

            // Check that the copy has succeeded.
            var targetBlobProperties = await targetBlob.GetPropertiesAsync();
            if (targetBlobProperties.Value.CopyStatus != CopyStatus.Success)
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
            // https://stackoverflow.com/questions/59714051/is-there-a-way-to-get-file-structure-from-azure-blob
            // https://docs.microsoft.com/en-us/azure/storage/blobs/storage-blobs-list?tabs=dotnet
            //await _container.CreateIfNotExistsAsync();

            //string continuationToken = null;

            //// The implementation of this method differs from the File Storage implementation.
            //// With Blob Storage there is no concept of a "directory". Path segments in file names
            //// are considered part of the file name, or a filename "prefix". There is therefore no
            //// equivalent of `CloudFileDirectory.ExistsAsync()` on a `CloudBlobDirectory`. A
            //// "directory" therefore only exists if there is a file whose name contains this
            //// "directory" or "prefix". Here were therefore try a list files with the given prefix
            //// and consider the directory "path" to exist if there is at least one file with this
            //// prefix.

            //do
            //{
            //    var resultSegment = _container.GetBlobsByHierarchyAsync(prefix: path, delimiter: "/")
            //        .AsPages(continuationToken);

            //    await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
            //    {
            //        foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
            //        {
            //            if (blobhierarchyItem.IsPrefix)
            //            {
            //                Console.WriteLine("Virtual directory prefix: {0}", blobhierarchyItem.Prefix);
            //                var result = await ListBlobsHierarchicalListing(container, blobhierarchyItem.Prefix, null).ConfigureAwait(false);
            //                blobs.AddRange(result);
            //            }
            //            else
            //            {
            //                Console.WriteLine("Blob name: {0}", blobhierarchyItem.Blob.Name);

            //                blobs.Add(blobhierarchyItem.Blob);
            //            }
            //        }

            //        Console.WriteLine();

            //        // Get the continuation token and loop until it is empty.
            //        continuationToken = blobPage.ContinuationToken;
            //    }
            //} while (continuationToken != string.Empty);

            //var response = await _container.ListBlobsSegmentedAsync(
            //    prefix: path,
            //    useFlatBlobListing: true,
            //    blobListingDetails: BlobListingDetails.None,
            //    maxResults: 1,
            //    currentToken: null,
            //    options: null,
            //    operationContext: null,
            //    cancellationToken: cancellationToken);

            //var directoryExists = response.Results.Any();

            //if (directoryExists)
            //{
            //    var directory = _client.GetDirectoryReference(path);

            //    var directoryContents = new AzureBlobStorageDirectoryContents(directory);

            //    return directoryContents;
            //}

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
                var blobProperties = await blob.GetPropertiesAsync();

                var blobInfo = new AzureBlob(blobProperties.Value, blob.Name, blob.Uri.AbsoluteUri);

                return blobInfo;
            }
            catch (RequestFailedException ex) when (RequestFailedExceptionHelper.IsNotFoundStorageException(ex))
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

            var blob = GetBlobReference(path);

            switch (_blobType)
            {
                case BlobType.Block:
                    await ((BlockBlobClient)blob).UploadAsync(stream);
                    return;

                case BlobType.Append:
                    await ((AppendBlobClient)blob).AppendBlockAsync(stream);
                    return;

                case BlobType.Page:
                    throw new NotSupportedException("Page blob type not supported.");

                default:
                    // This shouldn't happen unless Azure Storage introduces a new blob type.
                    throw new NotSupportedException("Unexpected blob type.");
            }
        }

        private BlobBaseClient GetBlobReference(string path)
        {
            switch (_blobType)
            {
                case BlobType.Block:
                    return _container.GetBlockBlobClient(path);

                case BlobType.Append:
                    return _container.GetAppendBlobClient(path);

                case BlobType.Page:
                    return _container.GetPageBlobClient(path);

                default:
                    // This shouldn't happen unless Azure Storage introduces a new blob type.
                    throw new NotSupportedException("Unexpected blob type.");
            }
        }
    }
}
