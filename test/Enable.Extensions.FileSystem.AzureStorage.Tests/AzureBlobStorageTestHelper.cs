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
        internal static string CreateRandomString(BlobType blobType = BlobType.BlockBlob)
        {
            return blobType == BlobType.PageBlob
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

        internal static Task CreateTestFilesAsync(CloudBlobContainer container, BlobType blobType, int count, string prefix = null)
        {
            prefix = prefix ?? string.Empty;

            var tasks = Enumerable.Range(0, count)
                .Select(o => Path.GetRandomFileName())
                .Select(o => Path.Combine(prefix, o))
                .Select(o => CreateTestFileAsync(container, blobType, o))
                .ToArray();

            return Task.WhenAll(tasks);
        }

        internal static Task CreateTestFileAsync(CloudBlobContainer container, BlobType blobType, string path)
        {
            var contents = CreateRandomString(blobType);

            return CreateTestFileAsync(container, blobType, path, contents);
        }

        internal static Task CreateTestFileAsync(CloudBlobContainer container, BlobType blobType, string path, string contents)
        {
            switch (blobType)
            {
                default:
                case BlobType.BlockBlob:
                    var blockBlob = container.GetBlockBlobReference(path);

                    return blockBlob.UploadTextAsync(contents);
                case BlobType.AppendBlob:
                    var appendBlob = container.GetAppendBlobReference(path);

                    return appendBlob.UploadTextAsync(contents);
                case BlobType.PageBlob:
                    var pageBlob = container.GetPageBlobReference(path);
                    var byteArray = Encoding.UTF8.GetBytes(contents);

                    return pageBlob.UploadFromByteArrayAsync(byteArray, 0, byteArray.Length);
            }
        }

        internal static Task<bool> ExistsAsync(CloudBlobContainer container, BlobType blobType, string path)
        {
            var blob = GetBlobReference(container, blobType, path);

            return blob.ExistsAsync();
        }

        internal static async Task<string> ReadFileContents(CloudBlobContainer container, BlobType blobType, string path)
        {
            var blob = GetBlobReference(container, blobType, path);

            string content;
            using (var stream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(stream);

                content = Encoding.UTF8.GetString(stream.ToArray());
            }

            return content;
        }

        internal static CloudBlob GetBlobReference(CloudBlobContainer container, BlobType blobType, string path)
        {
            switch (blobType)
            {
                default:
                case BlobType.BlockBlob:
                    return container.GetBlockBlobReference(path);
                case BlobType.AppendBlob:
                    return container.GetAppendBlobReference(path);
                case BlobType.PageBlob:
                    return container.GetPageBlobReference(path);
            }
        }
    }
}
