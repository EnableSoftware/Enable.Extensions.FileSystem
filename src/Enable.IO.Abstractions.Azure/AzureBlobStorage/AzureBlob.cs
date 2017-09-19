using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Enable.IO.Abstractions
{
    internal class AzureBlob : IFile
    {
        public AzureBlob(CloudBlockBlob blob)
        {
            Path = blob.Name;

            // There are no properties on an Azure Blob that can tell us the created time of the file.
            Created = default(DateTimeOffset).UtcDateTime;

            Modified = blob.Properties.LastModified.GetValueOrDefault().UtcDateTime;
        }

        public string Path { get; private set; }

        public DateTimeOffset Created { get; private set; }

        public DateTimeOffset Modified { get; private set; }
    }
}
