using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure.Storage.File;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    internal class AzureFileEnumerator : IEnumerable<IFile>, IEnumerator<IFile>
    {
        private readonly CloudFileDirectory _directory;
        private readonly CancellationToken _cancellationToken;

        private FileContinuationToken _continuationToken = null;
        private IEnumerator<IFile> _currentSegment = null;

        internal AzureFileEnumerator(
            CloudFileDirectory directory,
            CancellationToken cancellationToken)
        {
            _directory = directory;
            _cancellationToken = cancellationToken;
        }

        public IFile Current
        {
            get
            {
                return _currentSegment.Current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return _currentSegment.Current;
            }
        }

        public void Dispose()
        {
        }

        public IEnumerator<IFile> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool MoveNext()
        {
            _cancellationToken.ThrowIfCancellationRequested();

            // Here we enumerate over segmented result sets. Each segment of
            // blob results is retrieved asynchronously as the previous
            // segment is exhausted.
            //
            // If we do not currently have a segment of results, or if there
            // are no further results in the current segment, then advance to
            // the next segment of results.
            if (_currentSegment == null || !_currentSegment.MoveNext())
            {
                if (_currentSegment != null &&
                    _continuationToken == null)
                {
                    // If we've previously seen a result segment but we don't
                    // have a continuation token, then there are no further
                    // result segments to retrieve.
                    return false;
                }

                var response = _directory.ListFilesAndDirectoriesSegmentedAsync(_continuationToken)
                    .GetAwaiter()
                    .GetResult();

                _continuationToken = response.ContinuationToken;

                _currentSegment = response.Results
                    .Select<IListFileItem, IFile>((item) =>
                    {
                        if (item is CloudFile)
                        {
                            return new AzureFile(item as CloudFile);
                        }
                        else if (item is CloudFileDirectory)
                        {
                            return new AzureFileDirectory(item as CloudFileDirectory);
                        }

                        // This shouldn't happen unless the Azure Storage introduces a new implementation of the `IListFileItem` base type.
                        throw new InvalidOperationException("Unexpected file type enumerated.");
                    })
                    .GetEnumerator();

                return _currentSegment.MoveNext();
            }

            return true;
        }

        public void Reset()
        {
            _continuationToken = null;
            _currentSegment = null;
        }
    }
}
