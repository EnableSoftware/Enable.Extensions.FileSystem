using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Enable.IO.Abstractions
{
    public interface IFileSystem : IDisposable
    {
        Task CopyFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteFileAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> ExistsAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IFile> GetFileInfoAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<IEnumerable<IFile>> GetFileListAsync(
            string searchPattern = null,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<Stream> GetFileStreamAsync(
            string path,
            CancellationToken cancellationToken = default(CancellationToken));

        Task RenameFileAsync(
            string sourcePath,
            string targetPath,
            CancellationToken cancellationToken = default(CancellationToken));

        Task SaveFileAsync(
            string path,
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken));
    }

    public static class FileSystemExtensions
    {
        public static Task SaveFileAsync(
            this IFileSystem storage,
            string path,
            string contents)
        {
            // TODO Review this handling of the memory stream.
            return storage.SaveFileAsync(
                path,
                new MemoryStream(Encoding.UTF8.GetBytes(contents ?? string.Empty)));
        }
    }
}
