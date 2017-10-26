using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.FileSystem.AzureStorage.Internal;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;

namespace Enable.Extensions.FileSystem
{
    public class AzureFileStorage : IFileSystem
    {
        private readonly CloudFileShare _share;
        private readonly CloudFileDirectory _directory;

        public AzureFileStorage(string accountName, string accountKey, string shareName)
        {
            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, useHttps: true);

            var client = storageAccount.CreateCloudFileClient();

            _share = client.GetShareReference(shareName);

            _directory = _share.GetRootDirectoryReference();
        }

        public AzureFileStorage(CloudFileClient client, string shareName)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _share = client.GetShareReference(shareName);
            _directory = _share.GetRootDirectoryReference();
        }

        public async Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var sourceFile = _directory.GetFileReference(sourcePath);
            var targetFile = _directory.GetFileReference(targetPath);

            // The following only initiates a copy. There does not appear a way
            // to wait until the copy is complete without monitoring the copy
            // status of the target file.
            await targetFile.StartCopyAsync(sourceFile, cancellationToken);

            // However, for a file copy operation within the same storage
            // account, we can assume that the copy operation has completed
            // when `StartCopyAsync` completes. Here we check this assumption.
            if (targetFile.CopyState.Status != CopyStatus.Success)
            {
                // TODO Consider if we can handle this case better.
                throw new NotSupportedException();
            }
        }

        public async Task DeleteFileAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var file = _directory.GetFileReference(path);

            await file.DeleteIfExistsAsync(cancellationToken);
        }

        public async Task<IDirectoryContents> GetDirectoryContentsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directory = string.IsNullOrEmpty(path)
                ? _directory
                : _directory.GetDirectoryReference(path);

            try
            {
                await directory.FetchAttributesAsync(cancellationToken);

                var directoryContents = new AzureFileStorageDirectoryContents(directory);

                return directoryContents;
            }
            catch (StorageException ex) when (IsNotFoundStorageException(ex))
            {
                return new NotFoundDirectoryContents(path);
            }
        }

        /// <summary>
        /// Locate a file at the given subpath by directly mapping path segments to Azure File Storage directories.
        /// </summary>
        /// <param name="path">A path under the root file share.</param>
        /// <returns>The file information. Callers must check <see cref="IFile.Exists"/>.
        public async Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var file = _directory.GetFileReference(path);

            try
            {
                await file.FetchAttributesAsync(cancellationToken);

                var fileInfo = new AzureFile(file);

                return fileInfo;
            }
            catch (StorageException ex) when (IsNotFoundStorageException(ex))
            {
                return new NotFoundFile(path);
            }
        }

        public async Task<Stream> GetFileStreamAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var file = _directory.GetFileReference(path);

            return await file.OpenReadAsync(cancellationToken);
        }

        public async Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var sourceFile = _directory.GetFileReference(sourcePath);

            // It is not currently possible to rename a file in Azure. We
            // therefore attempt a copy and then delete. Note, however, that
            // `CopyFileAsync` may throw an exception event if the copy is
            // pending. It is therefore possible to end up with two files.
            await CopyFileAsync(sourcePath, targetPath, cancellationToken);
            await sourceFile.DeleteIfExistsAsync(cancellationToken);
        }

        public async Task SaveFileAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var file = _directory.GetFileReference(path);

            await file.UploadFromStreamAsync(stream, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        private static bool IsNotFoundStorageException(StorageException ex)
        {
            return ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound;
        }
    }
}
