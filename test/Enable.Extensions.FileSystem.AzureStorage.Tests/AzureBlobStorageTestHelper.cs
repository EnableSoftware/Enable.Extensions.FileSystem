using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Enable.Extensions.FileSystem.Test
{
    internal static class AzureBlobStorageTestHelper
    {
        internal static string CreateRandomString(BlobType blobType = BlobType.Block)
        {
            return blobType == BlobType.Page
                ? new string('a', 512)
                : Guid.NewGuid().ToString();
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

        internal static Task CreateTestFilesAsync(BlobContainerClient container, BlobType blobType, int count, string prefix = null)
        {
            prefix = prefix ?? string.Empty;

            var tasks = Enumerable.Range(0, count)
                .Select(o => Path.GetRandomFileName())
                .Select(o => Path.Combine(prefix, o))
                .Select(o => CreateTestFileAsync(container, blobType, o))
                .ToArray();

            return Task.WhenAll(tasks);
        }

        internal static Task CreateTestFileAsync(BlobContainerClient container, BlobType blobType, string path)
        {
            var contents = CreateRandomString(blobType);

            return CreateTestFileAsync(container, blobType, path, contents);
        }

        internal static async Task CreateTestFileAsync(BlobContainerClient container, BlobType blobType, string path, string contents)
        {
            switch (blobType)
            {
                default:
                case BlobType.Block:
                    var blockBlob = container.GetBlockBlobClient(path);

                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
                    {
                        await blockBlob.UploadAsync(stream);
                    }

                    return;

                case BlobType.Append:
                    var appendBlob = container.GetAppendBlobClient(path);

                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
                    {
                        await appendBlob.AppendBlockAsync(stream);
                    }

                    return;

                case BlobType.Page:
                    var pageBlob = container.GetPageBlobClient(path);
                    var byteArray = Encoding.UTF8.GetBytes(contents);

                    using (var stream = new MemoryStream(byteArray))
                    {
                        await pageBlob.UploadPagesAsync(stream, 0);
                    }

                    return;
            }
        }

        internal static async Task<bool> ExistsAsync(BlobContainerClient container, BlobType blobType, string path)
        {
            var blob = GetBlobReference(container, blobType, path);

            var exists = await blob.ExistsAsync();

            return exists.Value;
        }

        internal static async Task<string> ReadFileContents(BlobContainerClient container, BlobType blobType, string path)
        {
            var blob = GetBlobReference(container, blobType, path);

            string content;
            using (var stream = new MemoryStream())
            {
                await blob.DownloadToAsync(stream);

                content = Encoding.UTF8.GetString(stream.ToArray());
            }

            return content;
        }

        internal static BlobBaseClient GetBlobReference(BlobContainerClient container, BlobType blobType, string path)
        {
            switch (blobType)
            {
                default:
                case BlobType.Block:
                    return container.GetBlockBlobClient(path);
                case BlobType.Append:
                    return container.GetAppendBlobClient(path);
                case BlobType.Page:
                    return container.GetPageBlobClient(path);
            }
        }
    }
}
