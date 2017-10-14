using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enable.IO.Abstractions.Internal;

namespace Enable.IO.Abstractions
{
    public class FileSystem : IFileSystem
    {
        private readonly string _directory;

        public FileSystem(string directory)
        {
            if (!Path.IsPathRooted(directory))
            {
                directory = Path.GetFullPath(directory);
            }

            if (!directory.EndsWith("\\", StringComparison.Ordinal))
            {
                directory += "\\";
            }

            Directory.CreateDirectory(directory);

            _directory = directory;
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

        public Task<bool> ExistsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var exists = File.Exists(GetFullPath(path));

            return Task.FromResult(exists);
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

        public Task<IEnumerable<IFile>> GetFileListAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var directoryInfo = new DirectoryInfo(GetFullPath(path));

            var files = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly)
                .Select(o => new FileSystemFile(o))
                .ToList();

            return Task.FromResult<IEnumerable<IFile>>(files);
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

        private string GetFullPath(string path)
        {
            // TODO Ensure that we don't walk out of the root directory.
            return Path.Combine(_directory, path);
        }
    }
}
