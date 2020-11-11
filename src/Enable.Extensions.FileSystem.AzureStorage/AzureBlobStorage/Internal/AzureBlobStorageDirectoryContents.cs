//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using Microsoft.Azure.Storage.Blob;

//namespace Enable.Extensions.FileSystem.AzureStorage.Internal
//{
//    /// <summary>
//    /// Represents the contents of a Azure File Storage directory.
//    /// </summary>
//    internal class AzureBlobStorageDirectoryContents : IDirectoryContents
//    {
//        private readonly CloudBlobDirectory _directory;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="AzureBlobStorageDirectoryContents"/> class.
//        /// </summary>
//        /// <param name="directoryInfo">The <see cref="CloudBlobDirectory"/> to wrap.</param>
//        public AzureBlobStorageDirectoryContents(CloudBlobDirectory directory)
//        {
//            _directory = directory;
//        }

//        public bool Exists => true;

//        public string Path => _directory.Uri.LocalPath;

//        public string Name => _directory.Prefix;

//        public IEnumerator<IFile> GetEnumerator()
//        {
//            return new AzureBlobEnumerator(_directory, CancellationToken.None);
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return new AzureBlobEnumerator(_directory, CancellationToken.None);
//        }
//    }
//}
