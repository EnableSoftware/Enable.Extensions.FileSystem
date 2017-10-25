using System;
using Microsoft.WindowsAzure.Storage.File;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    /// <summary>
    /// Represents a file in Azure File Storage.
    /// </summary>
    internal class AzureFile : IFile
    {
        private readonly CloudFile _file;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFile"/> class.
        /// </summary>
        /// <param name="fileInfo">The <see cref="CloudFile"/> to wrap.</param>
        public AzureFile(CloudFile file)
        {
            _file = file;
        }

        /// <inheritdoc />
        // TODO Review this property.
        public bool Exists => true;

        /// <inheritdoc />
        public bool IsDirectory => false;

        /// <inheritdoc />
        // TODO Review this property.
        public DateTimeOffset LastModified => _file.Properties.LastModified.GetValueOrDefault();

        /// <inheritdoc />
        public long Length => _file.Properties.Length;

        /// <inheritdoc />
        public string Name => _file.Name;

        /// <inheritdoc />
        // TODO Review this property.
        public string Path => _file.Uri.PathAndQuery;
    }
}
