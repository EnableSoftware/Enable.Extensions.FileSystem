using System;
using System.IO;
using Microsoft.Azure.Storage.Blob;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    public static class IListBlobItemExtensions
    {
        public static string GetRelativeSubpath(this IListBlobItem listBlobItem)
        {
            if (listBlobItem is CloudBlobDirectory)
            {
                var directory = listBlobItem as CloudBlobDirectory;

                return directory.Prefix;
            }
            else if (listBlobItem is CloudBlob)
            {
                var blob = listBlobItem as CloudBlob;

                return blob.Name;
            }

            // This shouldn't happen unless Azure Storage introduces a new implementation of the `IListBlobItem` base type.
            throw new InvalidOperationException("Unexpected file type enumerated.");
        }

        public static string GetName(this IListBlobItem listBlobItem)
        {
            var path = listBlobItem.GetRelativeSubpath();

            if (listBlobItem is CloudBlobDirectory)
            {
                path = Path.GetFileName(Path.GetDirectoryName(path));
            }

            return Path.GetFileName(path);
        }
    }
}
