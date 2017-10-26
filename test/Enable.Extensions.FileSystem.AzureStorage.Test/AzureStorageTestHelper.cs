using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;

namespace Enable.Extensions.FileSystem.Test
{
    internal static class AzureStorageTestHelper
    {
        internal static string CreateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        internal static int CreateRandomNumber()
        {
            var rng = new Random();
            return rng.Next(byte.MaxValue);
        }

        internal static Task CreateTestFilesAsync(CloudBlobContainer container, int count, string prefix = null)
        {
            prefix = prefix ?? string.Empty;

            var tasks = Enumerable.Range(0, count)
                .Select(o => Path.GetRandomFileName())
                .Select(o => Path.Combine(prefix, o))
                .Select(o => CreateTestFileAsync(container, o))
                .ToArray();

            return Task.WhenAll(tasks);
        }

        internal static Task CreateTestFileAsync(CloudBlobContainer container, string blobName)
        {
            var contents = CreateRandomString();

            return CreateTestFileAsync(container, blobName, contents);
        }

        internal static Task CreateTestFileAsync(CloudBlobContainer container, string blobName, string contents)
        {
            var blob = container.GetBlockBlobReference(blobName);

            return blob.UploadTextAsync(contents);
        }

        internal static Task<bool> ExistsAsync(CloudBlobContainer container, string blobName)
        {
            var blob = container.GetBlockBlobReference(blobName);

            return blob.ExistsAsync();
        }

        internal static Task CreateTestFilesAsync(CloudFileShare fileShare, int count)
        {
            var tasks = Enumerable.Range(0, count)
                .Select(o => CreateTestFileAsync(fileShare))
                .ToArray();

            return Task.WhenAll(tasks);
        }

        internal static Task CreateTestFileAsync(CloudFileShare fileShare)
        {
            var fileName = Path.GetRandomFileName();

            return CreateTestFileAsync(fileShare, fileName);
        }

        internal static Task CreateTestFileAsync(CloudFileShare fileShare, string fileName)
        {
            var contents = CreateRandomString();

            return CreateTestFileAsync(fileShare, fileName, contents);
        }

        internal static Task CreateTestFileAsync(CloudFileShare fileShare, string fileName, string contents)
        {
            var rootDirectory = fileShare.GetRootDirectoryReference();

            var file = rootDirectory.GetFileReference(fileName);

            return file.UploadTextAsync(contents);
        }

        internal static Task<bool> ExistsAsync(CloudFileShare fileShare, string fileName)
        {
            var rootDirectory = fileShare.GetRootDirectoryReference();

            var file = rootDirectory.GetFileReference(fileName);

            return file.ExistsAsync();
        }
    }
}
