using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    internal class AzureBlob : IFile
    {
        private readonly CloudBlob _blob;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlob"/> class.
        /// </summary>
        /// <param name="fileInfo">The <see cref="CloudBlob"/> to wrap.</param>
        public AzureBlob(CloudBlob blob)
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
