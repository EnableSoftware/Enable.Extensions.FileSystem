using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.WindowsAzure.Storage.File;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    /// <summary>
    /// Represents the contents of a Azure File Storage directory.
    /// </summary>
    internal class AzureFileStorageDirectoryContents : IDirectoryContents
    {
        private readonly CloudFileDirectory _directory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileStorageDirectoryContents"/> class.
        /// </summary>
        /// <param name="directoryInfo">The <see cref="CloudFileDirectory"/> to wrap.</param>
        public AzureFileStorageDirectoryContents(CloudFileDirectory directory)
        {
            _directory = directory;
        }

        public bool Exists => true;

        public string Path => _directory.Uri.LocalPath;

        public string Name => _directory.Name;

        public IEnumerator<IFile> GetEnumerator()
        {
            return new AzureFileEnumerator(_directory, CancellationToken.None);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AzureFileEnumerator(_directory, CancellationToken.None);
        }
    }
}
