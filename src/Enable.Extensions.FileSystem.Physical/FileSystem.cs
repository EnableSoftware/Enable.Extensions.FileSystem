using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Enable.Extensions.FileSystem.Physical.Internal;

namespace Enable.Extensions.FileSystem
{
    /// <summary>
    /// Represents a physical, on-disk file system.
    /// </summary>
    public class FileSystem : BaseFileSystem
    {
        private readonly string _directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystem"/> class.
        /// </summary>
        /// <param name="directory">The root directory to represent. This must be an existing, absolute path.</param>
        /// <remarks>
        /// This class represents a directory, and the files and directories located under this "root" directory.
        /// </remarks>
        public FileSystem(string directory)
        {
            if (!Path.IsPathRooted(directory))
            {
                throw new ArgumentException("The path supplied must be an absolute path.", nameof(directory));
            }

            var root = Path.GetFullPath(directory);

            root = PathHelper.EnsureTrailingPathSeparator(root);

            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException(root);
            }

            _directory = root;
        }

        public override Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var source = GetFullPath(sourcePath);
            var target = GetFullPath(targetPath);

            // Ensure that the target directory exists, used if we're moving a
            // file to a new sub-directory.
            EnsureParentDirectoryExists(target);

            File.Copy(source, target);

            return Task.CompletedTask;
        }

        public override Task DeleteDirectoryAsync(
           string path,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            var directoryInfo = new DirectoryInfo(GetFullPath(path));

            if (directoryInfo.Exists)
            {
                Directory.Delete(GetFullPath(path), recursive: true);
            }

            return Task.CompletedTask;
        }

        public override Task DeleteFileAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            File.Delete(GetFullPath(path));

            return Task.CompletedTask;
        }

        public override Task<IDirectoryContents> GetDirectoryContentsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var fullPath = GetFullPath(path);

            var directoryInfo = new DirectoryInfo(fullPath);

            if (directoryInfo.Exists)
            {
                var directoryContents = new FileSystemDirectoryContents(directoryInfo);

                return Task.FromResult<IDirectoryContents>(directoryContents);
            }

            return Task.FromResult<IDirectoryContents>(
                    new NotFoundDirectoryContents(path));
        }

        /// <summary>
        /// Locate a file at the given subpath by directly mapping path segments to physical directories.
        /// </summary>
        /// <param name="path">A path under the root directory.</param>
        /// <returns>The file information. Callers must check <see cref="IFile.Exists"/>.
        public override Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var fullPath = GetFullPath(path);

            var fileInfo = new FileInfo(fullPath);

            if (fileInfo.Exists)
            {
                return Task.FromResult<IFile>(new FileSystemFile(fileInfo));
            }

            return Task.FromResult<IFile>(new NotFoundFile(path));
        }

        public override Task<Stream> GetFileStreamAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var source = GetFullPath(path);

            var stream = File.OpenRead(source);

            return Task.FromResult<Stream>(stream);
        }

        public override Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var source = GetFullPath(sourcePath);
            var target = GetFullPath(targetPath);

            // Ensure that the target directory exists, used if we're moving a
            // file to a new sub-directory.
            EnsureParentDirectoryExists(target);

            File.Move(
                GetFullPath(sourcePath),
                GetFullPath(targetPath));

            return Task.CompletedTask;
        }

        public override async Task SaveFileAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var target = GetFullPath(path);

            // Ensure that the target directory exists, used if we're creating a
            // file in a new sub-directory.
            EnsureParentDirectoryExists(target);

            using (var fileStream = File.Create(target))
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        /// <summary>
        /// Expand a path relative to the root directory, ensuring that the path does not walk out of the root directory.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <see cref="path"/> contains one or more of the invalid characters defined in <see cref="System.IO.Path.GetInvalidPathChars"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <see cref="path"/> is an absolute path.
        /// </exception>
        private string GetFullPath(string path)
        {
            // Absolute paths not permitted. This will throw if
            // path contains invalid characters.
            if (Path.IsPathRooted(path))
            {
                throw new InvalidOperationException("Absolute paths are not permitted.");
            }

            var fullPath = PathHelper.GetFullPath(_directory, path);

            // Sub-paths must be within sub-directories of the root directory.
            if (!PathHelper.IsUnderneathRoot(_directory, fullPath))
            {
                throw new InvalidOperationException("Paths must be a relative sub-path.");
            }

            return fullPath;
        }

        private void EnsureParentDirectoryExists(string path)
        {
            var parentDirectory = Directory.GetParent(path);

            Directory.CreateDirectory(parentDirectory.FullName);
        }
    }
}
