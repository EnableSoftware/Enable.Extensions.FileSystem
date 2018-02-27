using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Enable.Extensions.FileSystem
{
    public abstract class BaseFileSystem : IFileSystem
    {
        public abstract Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task DeleteDirectoryAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task DeleteFileAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task<IDirectoryContents> GetDirectoryContentsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task<Stream> GetFileStreamAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract Task SaveFileAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken));

        public async Task SaveFileAsync(
            string path,
            string contents,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents ?? string.Empty)))
            {
                await SaveFileAsync(path, stream, cancellationToken);
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
    }
}
