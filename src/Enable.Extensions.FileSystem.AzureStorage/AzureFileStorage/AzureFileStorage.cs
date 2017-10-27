using System;
using System.Collections.Generic;
using System.IO;
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

        public AzureFileStorage(string accountName, string accountKey, string shareName)
        {
            var credentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(credentials, useHttps: true);

            var client = storageAccount.CreateCloudFileClient();

            _share = client.GetShareReference(shareName);
        }

        public AzureFileStorage(CloudFileClient client, string shareName)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _share = client.GetShareReference(shareName);
        }

        public async Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var sourceFile = _share.GetFileReference(sourcePath);
            var targetFile = _share.GetFileReference(targetPath);

            // Ensure that the target directory exists, used if we're copying a
            // file to a new sub-directory.
            await EnsureParentDirectoriesExistAsync(targetFile, cancellationToken);

            // The following only initiates a copy. There does not appear a way
            // to wait until the copy is complete without monitoring the copy
            // status of the target file.
            await targetFile.StartCopyAsync(sourceFile);

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
            var file = _share.GetFileReference(path);

            await file.DeleteIfExistsAsync();
        }

        public async Task<IDirectoryContents> GetDirectoryContentsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directory = _share.GetDirectoryReference(path);

            try
            {
                await directory.FetchAttributesAsync();

                var directoryContents = new AzureFileStorageDirectoryContents(directory);

                return directoryContents;
            }
            catch (StorageException ex) when (StorageExceptionHelper.IsNotFoundStorageException(ex))
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
            var file = _share.GetFileReference(path);

            try
            {
                await file.FetchAttributesAsync();

                var fileInfo = new AzureFile(file);

                return fileInfo;
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
            var file = _share.GetFileReference(path);

            return await file.OpenReadAsync();
        }

        public async Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var sourceFile = _share.GetFileReference(sourcePath);
            var targetFile = _share.GetFileReference(targetPath);

            // Ensure that the target directory exists, used if we're moving a
            // file to a new sub-directory.
            await EnsureParentDirectoriesExistAsync(targetFile, cancellationToken);

            // It is not currently possible to rename a file in Azure. We
            // therefore attempt a copy and then delete. Note, however, that
            // `CopyFileAsync` may throw an exception event if the copy is
            // pending. It is therefore possible to end up with two files.
            await CopyFileAsync(sourcePath, targetPath, cancellationToken);
            await sourceFile.DeleteIfExistsAsync();
        }

        public async Task SaveFileAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var file = _share.GetFileReference(path);

            // Ensure that the target directory exists, used if we're creating a
            // file in a new sub-directory.
            await EnsureParentDirectoriesExistAsync(file, cancellationToken);

            await file.UploadFromStreamAsync(stream);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Creates the directory representing the parent directory for the file,
        /// and all of it's parent directories, if any do not already exist.
        /// </summary>
        /// <param name="file">A <see cref="CloudFile"/> whose parent directory is to be created.</param>
        /// <returns>A <see cref="Task"/> object that represents the current operation.</returns>
        /// <remarks>
        /// Azure File Storage requires that parent directories are created before files
        /// or subdirectories are created as children of that directory. This helper
        /// method can be used to ensure that the parent directory, and all of it's parent
        /// directories, are present before attempting to create a file.
        /// </remarks>
        private static async Task EnsureParentDirectoriesExistAsync(CloudFile file, CancellationToken cancellationToken)
        {
            if (file.Parent == null)
            {
                return;
            }

            var directories = new Stack<CloudFileDirectory>();

            var parentDirectory = file.Parent;

            do
            {
                directories.Push(parentDirectory);
            }
            while ((parentDirectory = parentDirectory.Parent) != null);

            foreach (var directory in directories)
            {
                await directory.CreateIfNotExistsAsync();
            }
        }
    }
}
