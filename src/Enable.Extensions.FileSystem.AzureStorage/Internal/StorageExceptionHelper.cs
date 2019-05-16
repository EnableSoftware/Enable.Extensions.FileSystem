using System.Net;
using Microsoft.Azure.Storage;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    internal static class StorageExceptionHelper
    {
        private const int HttpNotFoundStatusCode = (int)HttpStatusCode.NotFound;

        internal static bool IsNotFoundStorageException(StorageException ex)
        {
            return ex.RequestInformation.HttpStatusCode == HttpNotFoundStatusCode;
        }
    }
}
