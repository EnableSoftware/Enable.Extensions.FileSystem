using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    internal class AzureBlob : IFile
    {
        private readonly CloudBlockBlob _blob;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlob"/> class.
        /// </summary>
        /// <param name="fileInfo">The <see cref="CloudBlockBlob"/> to wrap.</param>
        public AzureBlob(CloudBlockBlob blob)
        {
            _blob = blob;
        }

        /// <inheritdoc />
        public bool Exists => true;

        /// <inheritdoc />
        public bool IsDirectory => false;

        /// <inheritdoc />
        public DateTimeOffset LastModified => _blob.Properties.LastModified.GetValueOrDefault();

        /// <inheritdoc />
        public long Length => _blob.Properties.Length;

        /// <inheritdoc />
        public string Name => _blob.GetName();

        /// <inheritdoc />
        public string Path => _blob.GetRelativeSubpath();
    }
}
