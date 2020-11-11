using System;
using Azure.Storage.Files.Shares.Models;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    /// <summary>
    /// Represents a file in Azure File Storage.
    /// </summary>
    internal class AzureFile : IFile
    {
        private readonly ShareFileProperties _properties;
        private readonly string _path;
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFile"/> class.
        /// </summary>
        /// <param name="fileInfo">The <see cref="CloudFile"/> to wrap.</param>
        public AzureFile(ShareFileProperties properties, string path)
        {
            _properties = properties;
            _path = path;
            _name = System.IO.Path.GetFileName(path);
        }

        /// <inheritdoc />
        public bool Exists => true;

        /// <inheritdoc />
        public bool IsDirectory => false;

        /// <inheritdoc />
        public DateTimeOffset LastModified => _properties.LastModified;

        /// <inheritdoc />
        public long Length => _properties.ContentLength;

        /// <inheritdoc />
        public string Name => _name;

        /// <inheritdoc />
        public string Path => _path;
    }
}
