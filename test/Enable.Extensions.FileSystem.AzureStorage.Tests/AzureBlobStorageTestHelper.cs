using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;

namespace Enable.Extensions.FileSystem.Test
{
    internal static class AzureBlobStorageTestHelper
    {
        internal static string CreateRandomString()
        {
            return Guid.NewGuid().ToString();
        }

        internal static int CreateRandomNumber()
        {
            return CreateRandomNumber(byte.MaxValue);
        }

        internal static int CreateRandomNumber(int maxValue)
        {
            return CreateRandomNumber(0, maxValue);
        }

        internal static int CreateRandomNumber(int minValue, int maxValue)
        {
            var rng = new Random();
            return rng.Next(minValue, maxValue);
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

        internal static async Task<string> ReadFileContents(CloudBlobContainer container, string path)
        {
            var blob = container.GetBlockBlobReference(path);

            string content;
            using (var stream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(stream);

                content = Encoding.UTF8.GetString(stream.ToArray());
            }

            return content;
        }
    }
}
