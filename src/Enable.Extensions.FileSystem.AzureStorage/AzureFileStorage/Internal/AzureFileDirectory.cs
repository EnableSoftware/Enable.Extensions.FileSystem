//using System;
//using Microsoft.Azure.Storage.File;

//namespace Enable.Extensions.FileSystem.AzureStorage.Internal
//{
//    /// <summary>
//    /// Represents a directory in Azure File Storage.
//    /// </summary>
//    internal class AzureFileDirectory : IFile
//    {
//        private readonly CloudFileDirectory _directory;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="AzureFileDirectory"/> class.
//        /// </summary>
//        /// <param name="directory">The <see cref="CloudFileDirectory"/> to wrap.</param>
//        public AzureFileDirectory(CloudFileDirectory directory)
//        {
//            _directory = directory;
//        }

//        /// <inheritdoc />
//        public bool Exists => true;

//        /// <inheritdoc />
//        public bool IsDirectory => true;

//        /// <inheritdoc />
//        public DateTimeOffset LastModified => _directory.Properties.LastModified.GetValueOrDefault();

//        /// <inheritdoc />
//        public long Length => -1;

//        /// <inheritdoc />
//        public string Name => _directory.Name;

//        /// <inheritdoc />
//        public string Path => _directory.GetRelativeSubpath();
//    }
//}
