using System;
using System.IO;

namespace Enable.Extensions.FileSystem.Internal
{
    /// <summary>
    /// Represents a file on a physical file system.
    /// </summary>
    internal class FileSystemFile : IFile
    {
        private readonly FileInfo _fileInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemFile"/> class.
        /// </summary>
        /// <param name="fileInfo">The <see cref="System.IO.FileInfo"/> to wrap.</param>
        public FileSystemFile(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        /// <inheritdoc />
        public bool Exists => _fileInfo.Exists;

        /// <inheritdoc />
        public bool IsDirectory => false;

        /// <inheritdoc />
        public DateTimeOffset LastModified => _fileInfo.LastWriteTimeUtc;

        /// <inheritdoc />
        public long Length => _fileInfo.Length;

        /// <inheritdoc />
        public string Name => _fileInfo.Name;

        /// <inheritdoc />
        public string Path => _fileInfo.FullName;
    }
}
