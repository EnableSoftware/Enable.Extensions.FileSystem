using System;
using Azure.Storage.Blobs.Models;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    internal class AzureBlob : IFile
    {
        private readonly BlobProperties _blob;
        private readonly string _blobName;
        private readonly string _blobUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlob"/> class.
        /// </summary>
        /// <param name="fileInfo">The <see cref="CloudBlob"/> to wrap.</param>
        public AzureBlob(BlobProperties blob, string blobName, string blobUri)
        {
            _blob = blob;
            _blobName = blobName;
            _blobUri = blobUri;
        }

        /// <inheritdoc />
        public bool Exists => true;

        /// <inheritdoc />
        public bool IsDirectory => false;

        /// <inheritdoc />
        public DateTimeOffset LastModified => _blob.LastModified;

        /// <inheritdoc />
        public long Length => _blob.ContentLength;

        /// <inheritdoc />
        public string Name => _blobName;

        /// <inheritdoc />
        public string Path => _blobUri;
    }
}
