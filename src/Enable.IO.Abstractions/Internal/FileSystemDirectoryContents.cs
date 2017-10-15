using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Enable.IO.Abstractions.Internal
{
    /// <summary>
    /// Represents the contents of a physical file system directory
    /// </summary>
    internal class FileSystemDirectoryContents : IDirectoryContents
    {
        private readonly DirectoryInfo _directoryInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemDirectoryContents"/> class.
        /// </summary>
        /// <param name="directoryInfo">The <see cref="System.IO.DirectoryInfo"/> to wrap.</param>
        public FileSystemDirectoryContents(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
        }

        /// <inheritdoc />
        public bool Exists => _directoryInfo.Exists;

        /// <inheritdoc />
        public string Name => _directoryInfo.Name;

        /// <inheritdoc />
        public string Path => _directoryInfo.FullName;

        public IEnumerator<IFile> GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        private IEnumerator<IFile> GetEnumeratorInternal()
        {
            try
            {
                var fileList = _directoryInfo
                    .EnumerateFileSystemInfos()
                    .Select<FileSystemInfo, IFile>((info) =>
                    {
                        if (info is FileInfo)
                        {
                            return new FileSystemFile(info as FileInfo);
                        }
                        else if (info is DirectoryInfo)
                        {
                            return new FileSystemDirectory(info as DirectoryInfo);
                        }

                        // This shouldn't happen unless the BCL introduces new implementation of the `FileSystemInfo` base type.
                        throw new InvalidOperationException("Unexpected file type enumerated.");
                    });

                return fileList.GetEnumerator();
            }
            catch (DirectoryNotFoundException)
            {
                return Enumerable.Empty<IFile>().GetEnumerator();
            }
        }
    }
}
