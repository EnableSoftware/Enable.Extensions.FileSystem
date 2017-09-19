using System;
using Microsoft.WindowsAzure.Storage.File;

namespace Enable.IO.Abstractions
{
    internal class AzureFile : IFile
    {
        public AzureFile(CloudFile file)
        {
            Path = file.Name;

            // There are no properties on an Azure File that can tell us the created time of the file.
            Created = default(DateTimeOffset).UtcDateTime;

            Modified = file.Properties.LastModified.GetValueOrDefault().UtcDateTime;
        }

        public string Path { get; private set; }

        public DateTimeOffset Created { get; private set; }

        public DateTimeOffset Modified { get; private set; }
    }
}
