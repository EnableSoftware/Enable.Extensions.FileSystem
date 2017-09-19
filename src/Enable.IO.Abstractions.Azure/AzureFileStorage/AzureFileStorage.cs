using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;

namespace Enable.IO.Abstractions
{
    public class AzureFileStorage : IFileSystem
    {
        private readonly CloudFileShare _share;
        private readonly string _directory;

        public AzureFileStorage(string connectionString, string shareName, string directory)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudFileClient();

            _share = client.GetShareReference(shareName);
            _directory = directory;
        }

        public async Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directory = await GetDirectoryAndCreateIfNotExists(_directory, cancellationToken);

            var sourceFile = directory.GetFileReference(sourcePath);
            var targetFile = directory.GetFileReference(targetPath);

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
            var directory = await GetDirectoryAndCreateIfNotExists(_directory, cancellationToken);

            var file = directory.GetFileReference(path);

            await file.DeleteIfExistsAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directory = await GetDirectoryAndCreateIfNotExists(_directory, cancellationToken);

            var file = directory.GetFileReference(path);

            return await file.ExistsAsync(cancellationToken);
        }

        public async Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directory = await GetDirectoryAndCreateIfNotExists(_directory, cancellationToken);

            var file = directory.GetFileReference(path);

            await file.FetchAttributesAsync(cancellationToken);

            var fileInfo = new AzureFile(file);

            return fileInfo;
        }

        public async Task<IEnumerable<IFile>> GetFileListAsync(
            string searchPattern = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directory = await GetDirectoryAndCreateIfNotExists(_directory, cancellationToken);

            // TODO `searchPattern` is not used here.
            return new AzureFileEnumerator(directory, cancellationToken);
        }

        public async Task<Stream> GetFileStreamAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directory = await GetDirectoryAndCreateIfNotExists(_directory, cancellationToken);

            var file = directory.GetFileReference(path);

            return await file.OpenReadAsync(cancellationToken);
        }

        public async Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directory = await GetDirectoryAndCreateIfNotExists(_directory, cancellationToken);

            var sourceFile = directory.GetFileReference(sourcePath);

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
            var directory = await GetDirectoryAndCreateIfNotExists(_directory, cancellationToken);

            var file = directory.GetFileReference(path);

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

        private async Task<CloudFileDirectory> GetDirectoryAndCreateIfNotExists(
            string path,
            CancellationToken cancellationToken)
        {
            await _share.CreateIfNotExistsAsync(cancellationToken);

            var rootDirectory = _share.GetRootDirectoryReference();

            var directory = rootDirectory.GetDirectoryReference(path);

            await directory.CreateIfNotExistsAsync(cancellationToken);

            return directory;
        }
    }
}
