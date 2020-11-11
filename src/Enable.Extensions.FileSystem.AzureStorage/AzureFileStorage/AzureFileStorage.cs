using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.FileSystem.AzureStorage.Internal;
using Azure.Storage.Files.Shares;
using Azure;

namespace Enable.Extensions.FileSystem
{
    public class AzureFileStorage : BaseFileSystem
    {
        private readonly ShareClient _share;

        public AzureFileStorage(string connectionString, string shareName)
        {
            _share = new ShareClient(connectionString, shareName);
        }

        public AzureFileStorage(ShareClient share)
        {
            if (share == null)
            {
                throw new ArgumentNullException(nameof(share));
            }

            _share = share;
        }

        public override async Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var sourceDirectoryName = Path.GetDirectoryName(sourcePath);
            var targetDirectoryName = Path.GetDirectoryName(targetPath);

            var sourceDirectory = _share.GetDirectoryClient(sourceDirectoryName);
            var targetDirectory = _share.GetDirectoryClient(targetDirectoryName);

            // Ensure that the target directory exists, used if we're copying a
            // file to a new sub-directory.
            var targetDirectoryExists = await targetDirectory.ExistsAsync();
            if (!targetDirectoryExists)
            {
                await targetDirectory.CreateAsync();
            }

            var sourceFile = sourceDirectory.GetFileClient(Path.GetFileName(sourcePath));
            var targetFile = targetDirectory.GetFileClient(Path.GetFileName(targetPath));

            // The following only initiates a copy. There does not appear a way
            // to wait until the copy is complete without monitoring the copy
            // status of the target file.
            var shareFileCopyInfo = await targetFile.StartCopyAsync(sourceFile.Uri);

            // However, for a file copy operation within the same storage
            // account, we can assume that the copy operation has completed
            // when `StartCopyAsync` completes. Here we check this assumption.
            if (shareFileCopyInfo.Value.CopyStatus != Azure.Storage.Files.Shares.Models.CopyStatus.Success)
            {
                // TODO Consider if we can handle this case better.
                throw new NotSupportedException();
            }
        }

        public override async Task DeleteDirectoryAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var contents = await GetDirectoryContentsAsync(path);

            if (contents.Exists)
            {
                foreach (var item in contents)
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

                var directory = _share.GetDirectoryClient(path);
                await directory.DeleteIfExistsAsync();
            }
        }

        public override async Task DeleteFileAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directoryClient = _share.GetDirectoryClient(path);
            var fileClient = directoryClient.GetFileClient(path);

            await fileClient.DeleteIfExistsAsync();
        }

        public override async Task<IDirectoryContents> GetDirectoryContentsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return new NotFoundDirectoryContents(path);

            //var directory = _share.GetDirectoryClient(path);

            //try
            //{
            //    await directory.FetchAttributesAsync();

            //    var directoryContents = new AzureFileStorageDirectoryContents(directory);

            //    return directoryContents;
            //}
            //catch (RequestFailedException ex) when (RequestFailedExceptionHelper.IsNotFoundStorageException(ex))
            //{
            //    return new NotFoundDirectoryContents(path);
            //}
        }

        /// <summary>
        /// Locate a file at the given subpath by directly mapping path segments to Azure File Storage directories.
        /// </summary>
        /// <param name="path">A path under the root file share.</param>
        /// <returns>The file information. Callers must check <see cref="IFile.Exists"/>.
        public override async Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directoryClient = _share.GetDirectoryClient(path);
            var fileClient = directoryClient.GetFileClient(path);

            try
            {
                var properties = await fileClient.GetPropertiesAsync();

                var fileInfo = new AzureFile(properties, path);

                return fileInfo;
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
            var directoryClient = _share.GetDirectoryClient(path);
            var fileClient = directoryClient.GetFileClient(path);

            return await fileClient.OpenReadAsync();
        }

        public override async Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var sourceDirectoryName = Path.GetDirectoryName(sourcePath);
            var sourceDirectory = _share.GetDirectoryClient(sourceDirectoryName);
            var sourceFile = sourceDirectory.GetFileClient(Path.GetFileName(sourcePath));

            // It is not currently possible to rename a file in Azure. We
            // therefore attempt a copy and then delete. Note, however, that
            // `CopyFileAsync` may throw an exception event if the copy is
            // pending. It is therefore possible to end up with two files.
            await CopyFileAsync(sourcePath, targetPath, cancellationToken);
            await sourceFile.DeleteIfExistsAsync();
        }

        public override async Task SaveFileAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directoryClient = _share.GetDirectoryClient(path);
            var fileClient = directoryClient.GetFileClient(path);

            // Ensure that the target directory exists, used if we're creating a
            // file in a new sub-directory.
            //await EnsureParentDirectoriesExistAsync(fileClient, cancellationToken);

            await fileClient.UploadAsync(stream);
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
        //private static async Task EnsureParentDirectoriesExistAsync(CloudFile file, CancellationToken cancellationToken)
        //{
        //    if (file.Parent == null)
        //    {
        //        return;
        //    }

        //    var directories = new Stack<CloudFileDirectory>();

        //    var parentDirectory = file.Parent;

        //    do
        //    {
        //        directories.Push(parentDirectory);
        //    }
        //    while ((parentDirectory = parentDirectory.Parent) != null);

        //    foreach (var directory in directories)
        //    {
        //        await directory.CreateIfNotExistsAsync();
        //    }
        //}
    }
}
