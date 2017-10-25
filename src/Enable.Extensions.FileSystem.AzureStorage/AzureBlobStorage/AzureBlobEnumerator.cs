using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Enable.Extensions.FileSystem
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
                    // result segements to retrieve.
                    return false;
                }

                // TODO How should this handle virtual directories?
                var response = _directory.ListBlobsSegmentedAsync(_continuationToken, _cancellationToken)
                    .GetAwaiter()
                    .GetResult();

                _continuationToken = response.ContinuationToken;

                _currentSegment = response.Results
                    .OfType<CloudBlockBlob>()
                    .Select(o => new AzureBlob(o))
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
