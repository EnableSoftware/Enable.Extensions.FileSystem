//using System;

//namespace Enable.Extensions.FileSystem.AzureStorage.Internal
//{
//    internal class AzureBlobDirectory : IFile
//    {
//        private readonly CloudBlobDirectory _directory;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="AzureBlobDirectory"/> class.
//        /// </summary>
//        /// <param name="directory">The <see cref="CloudBlobDirectory"/> to wrap.</param>
//        public AzureBlobDirectory(CloudBlobDirectory directory)
//        {
//            _directory = directory;
//        }

//        /// <inheritdoc />
//        public bool Exists => true;

//        /// <inheritdoc />
//        public bool IsDirectory => true;

//        /// <inheritdoc />
//        public DateTimeOffset LastModified => default(DateTimeOffset);

//        /// <inheritdoc />
//        public long Length => -1;

//        /// <inheritdoc />
//        public string Name => _directory.GetName();

//        /// <inheritdoc />
//        public string Path => _directory.GetRelativeSubpath();
//    }
//}
