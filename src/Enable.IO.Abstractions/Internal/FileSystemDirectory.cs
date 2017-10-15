using System;
using System.IO;

namespace Enable.IO.Abstractions.Internal
{
    /// <summary>
    /// Represents a directory on a physical file system.
    /// </summary>
    internal class FileSystemDirectory : IFile
    {
        private readonly DirectoryInfo _directoryInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemDirectory"/> class.
        /// </summary>
        /// <param name="directoryInfo">The <see cref="System.IO.DirectoryInfo"/> to wrap.</param>
        public FileSystemDirectory(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
        }

        /// <inheritdoc />
        public bool Exists => _directoryInfo.Exists;

        /// <inheritdoc />
        public bool IsDirectory => true;

        /// <inheritdoc />
        public DateTimeOffset LastModified => _directoryInfo.LastWriteTimeUtc;

        /// <inheritdoc />
        public long Length => -1;

        /// <inheritdoc />
        public string Name => _directoryInfo.Name;

        /// <inheritdoc />
        public string Path => _directoryInfo.FullName;
    }
}
