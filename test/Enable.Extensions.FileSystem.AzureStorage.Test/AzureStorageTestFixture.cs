using System;
using Microsoft.WindowsAzure.Storage;

namespace Enable.Extensions.FileSystem.Test
{
    public class AzureStorageTestFixture
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureStorageTestFixture()
        {
            var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_CONNECTION_STRING");

#if DEBUG
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback to using the Azure Storage emulator if we did't find a connection
                // string in the relevant environment variable. This should only be the case
                // at development time. CI builds will set this environment variable.
                _storageAccount = CloudStorageAccount.DevelopmentStorageAccount;

                // TODO We should make use of the `AzureStorageEmulatorFixture` here
                // and auto-start the storage emulator if we've hit this block.
                return;
            }
#endif

            _storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        public CloudStorageAccount StorageAccount
        {
            get
            {
                return _storageAccount;
            }
        }
    }
}
