using System;

namespace Enable.Extensions.FileSystem
{
    /// <summary>
    /// Represents a non-existant file.
    /// </summary>
    public class NotFoundFile : IFile
    {
        public NotFoundFile(string path)
        {
            Name = path;
        }

        /// <inheritdoc />
        public bool Exists => false;

        /// <inheritdoc />
        public bool IsDirectory => false;

        /// <inheritdoc />
        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        /// <inheritdoc />
        public long Length => -1;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Path => null;
    }
}
