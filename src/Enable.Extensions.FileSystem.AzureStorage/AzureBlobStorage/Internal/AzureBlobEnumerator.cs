using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Azure.Storage.Blob;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    internal class AzureBlobEnumerator : IEnumerable<IFile>, IEnumerator<IFile>
    {
        private readonly CloudBlobDirectory _directory;
        private readonly CancellationToken _cancellationToken;

        private BlobContinuationToken _continuationToken = null;
        private IEnumerator<IFile> _currentSegment = null;

        internal AzureBlobEnumerator(
            CloudBlobDirectory directory,
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

                var response = _directory.ListBlobsSegmentedAsync(_continuationToken)
                    .GetAwaiter()
                    .GetResult();

                _continuationToken = response.ContinuationToken;

                _currentSegment = response.Results
                    .Select<IListBlobItem, IFile>((item) =>
                    {
                        if (item is CloudBlob)
                        {
                            return new AzureBlob(item as CloudBlob);
                        }
                        else if (item is CloudBlobDirectory)
                        {
                            return new AzureBlobDirectory(item as CloudBlobDirectory);
                        }

                        // This shouldn't happen unless Azure Storage introduces a new implementation of the `IListFileItem` base type.
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
