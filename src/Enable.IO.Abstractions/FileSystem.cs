using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var fileInfo = new FileInfo(GetFullPath(path));

            if (!fileInfo.Exists)
            {
                return Task.FromResult<IFile>(null);
            }

            var file = new FileSystemFile(fileInfo);

            return Task.FromResult<IFile>(file);
        }

        public Task<IEnumerable<IFile>> GetFileListAsync(
            string searchPattern = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*";
            }

            var files = Directory.GetFiles(_directory, searchPattern, SearchOption.TopDirectoryOnly)
                .Select(o => new FileInfo(o))
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

        private class FileSystemFile : IFile
        {
            public FileSystemFile(FileInfo fileInfo)
            {
                Path = fileInfo.FullName;
                Created = fileInfo.CreationTimeUtc;
                Modified = fileInfo.LastWriteTimeUtc;
            }

            public string Path { get; private set; }

            public DateTimeOffset Created { get; private set; }

            public DateTimeOffset Modified { get; private set; }
        }
    }
}
