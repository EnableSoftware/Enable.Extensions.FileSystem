using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Enable.IO.Abstractions.Internal;

namespace Enable.IO.Abstractions
{
    /// <summary>
    /// Represents a physical, on-disk file system.
    /// </summary>
    public class FileSystem : IFileSystem
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

        public Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            File.Copy(
                GetFullPath(sourcePath),
                GetFullPath(targetPath));

            return Task.CompletedTask;
        }

        public Task DeleteFileAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            File.Delete(GetFullPath(path));

            return Task.CompletedTask;
        }

        public Task<IDirectoryContents> GetDirectoryContentsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var fullPath = GetFullPath(path);

            if (fullPath == null)
            {
                return Task.FromResult<IDirectoryContents>(
                    new NotFoundDirectoryContents(path));
            }

            var directoryInfo = new DirectoryInfo(fullPath);

            var directoryContents = new FileSystemDirectoryContents(directoryInfo);

            return Task.FromResult<IDirectoryContents>(directoryContents);
        }

        /// <summary>
        /// Locate a file at the given subpath by directly mapping path segments to physical directories.
        /// </summary>
        /// <param name="path">A path under the root directory.</param>
        /// <returns>The file information. Callers must check <see cref="IFile.Exists"/>.
        public Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var fullPath = GetFullPath(path);

            if (fullPath == null)
            {
                return Task.FromResult<IFile>(new NotFoundFile(path));
            }

            var fileInfo = new FileInfo(fullPath);

            return Task.FromResult<IFile>(new FileSystemFile(fileInfo));
        }

        public Task<Stream> GetFileStreamAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var stream = File.OpenRead(GetFullPath(path));

            return Task.FromResult<Stream>(stream);
        }

        public Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            File.Move(
                GetFullPath(sourcePath),
                GetFullPath(targetPath));

            return Task.CompletedTask;
        }

        public async Task SaveFileAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var fileStream = File.Create(GetFullPath(path)))
            {
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                }

                await stream.CopyToAsync(fileStream);
            }
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
        /// Expand a path relative to the root directory, ensuring that the path does not walk out of the root directory.
        /// </summary>
        /// <remarks>
        /// Absolute or otherwise invalid paths return a null value.
        /// </remarks>
        private string GetFullPath(string path)
        {
            try
            {
                // Absolute paths not permitted. This will throw if
                // path contains invalid characters.
                if (Path.IsPathRooted(path))
                {
                    return null;
                }

                var fullPath = PathHelper.GetFullPath(_directory, path);

                // Subpaths must be within sub-directoryies of the root directry.
                if (!PathHelper.IsUnderneathRoot(_directory, fullPath))
                {
                    return null;
                }

                return fullPath;
            }
            catch
            {
                return null;
            }
        }
    }
}
