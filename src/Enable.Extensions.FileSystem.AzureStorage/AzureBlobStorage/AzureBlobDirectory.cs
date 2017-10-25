using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Enable.Extensions.FileSystem
{
    internal class AzureBlobDirectory : IFile
    {
        private readonly CloudBlobDirectory _directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobDirectory"/> class.
        /// </summary>
        /// <param name="directory">The <see cref="CloudBlobDirectory"/> to wrap.</param>
        public AzureBlobDirectory(CloudBlobDirectory directory)
        {
            _directory = directory;
        }

        /// <inheritdoc />
        // TODO Review this property.
        public bool Exists => true;

        /// <inheritdoc />
        public bool IsDirectory => false;

        /// <inheritdoc />
        // TODO Review this property.
        public DateTimeOffset LastModified => default(DateTimeOffset);

        /// <inheritdoc />
        public long Length => -1;

        /// <inheritdoc />
        /// // TODO Review this property.
        public string Name => _directory.Prefix;

        /// <inheritdoc />
        // TODO Review this property.
        public string Path => _directory.Uri.PathAndQuery;
    }
}
