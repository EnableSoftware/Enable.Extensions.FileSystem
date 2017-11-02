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

        internal static Task CreateTestFileAsync(CloudBlobContainer container, string path)
        {
            var contents = CreateRandomString();

            return CreateTestFileAsync(container, path, contents);
        }

        internal static Task CreateTestFileAsync(CloudBlobContainer container, string path, string contents)
        {
            var blob = container.GetBlockBlobReference(path);

            return blob.UploadTextAsync(contents);
        }

        internal static Task<bool> ExistsAsync(CloudBlobContainer container, string path)
        {
            var blob = container.GetBlockBlobReference(path);

            return blob.ExistsAsync();
        }

        internal static Task CreateTestFilesAsync(CloudFileShare fileShare, int count, string directory = null)
        {
            directory = directory ?? string.Empty;

            var tasks = Enumerable.Range(0, count)
                .Select(o => Path.GetRandomFileName())
                .Select(o => Path.Combine(directory, o))
                .Select(o => CreateTestFileAsync(fileShare, o))
                .ToArray();

            return Task.WhenAll(tasks);
        }

        internal static Task CreateTestFileAsync(CloudFileShare fileShare)
        {
            var fileName = Path.GetRandomFileName();

            return CreateTestFileAsync(fileShare, fileName);
        }

        internal static Task CreateTestFileAsync(CloudFileShare fileShare, string path)
        {
            var contents = CreateRandomString();

            return CreateTestFileAsync(fileShare, path, contents);
        }

        internal static async Task CreateTestFileAsync(CloudFileShare fileShare, string path, string contents)
        {
            var fileName = Path.GetFileName(path);
            var directoryPath = Path.GetDirectoryName(path);

            var pathSegments = directoryPath.Split(
                new[]
                {
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                },
                StringSplitOptions.RemoveEmptyEntries);

            var parentDirectory = fileShare.GetRootDirectoryReference();

            foreach (var segment in pathSegments)
            {
                parentDirectory = parentDirectory.GetDirectoryReference(segment);

                // Each parent directory needs to be created before we can create a file.
                // This is different to the Blob Storage implementation.
                await parentDirectory.CreateIfNotExistsAsync();
            }

            var file = parentDirectory.GetFileReference(fileName);

            await file.UploadTextAsync(contents);
        }

        internal static Task<bool> ExistsAsync(CloudFileShare fileShare, string path)
        {
            var rootDirectory = fileShare.GetRootDirectoryReference();

            var file = rootDirectory.GetFileReference(path);

            return file.ExistsAsync();
        }
    }
}
