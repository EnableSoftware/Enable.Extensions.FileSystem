using System.Net;
using Azure;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    internal static class RequestFailedExceptionHelper
    {
        private const int HttpNotFoundStatusCode = (int)HttpStatusCode.NotFound;

        internal static bool IsNotFoundStorageException(RequestFailedException ex)
        {
            return ex.Status == HttpNotFoundStatusCode;
        }
    }
}
