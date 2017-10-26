using System;
using Microsoft.WindowsAzure.Storage;

namespace Enable.Extensions.FileSystem.Test
{
    public class AzureStorageTestFixture : IDisposable
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly AzureStorageEmulatorManager _azureStorageEmulatorManager;
        private readonly bool _useDevelopmentStorageAccount;
        private readonly bool _emulatorAlreadyRunning;

        private bool _disposed;

        public AzureStorageTestFixture()
        {
            var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_CONNECTION_STRING");

#if DEBUG
            if (string.IsNullOrEmpty(connectionString))
            {
                _useDevelopmentStorageAccount = true;

                // Fallback to using the Azure Storage emulator if we did't find a connection
                // string in the relevant environment variable. This should only be the case
                // at development time. CI builds will set this environment variable.
                // Note that the Azure Storage Emulator does not yet support Azure File Storage,
                // so Azure File Storage tests will fail if running with the emulator.
                _storageAccount = CloudStorageAccount.DevelopmentStorageAccount;

                // Here we attempt to auto-start the storage emulator if we're using
                // the development storage account.
                _azureStorageEmulatorManager = AzureStorageEmulatorManager.Instance;

                // If the storage emulator is already running, record this so
                // that we don't attempt stop the emulator when being disposed.
                _emulatorAlreadyRunning = _azureStorageEmulatorManager.GetIsEmulatorRunning()
                    .GetAwaiter()
                    .GetResult();

                // TODO If this method is called twice, then the emulator won't be stopped as part of `Dispose`.
                // Also, consider what to do if multiple instances of this class are used simultaneously.
                if (!_emulatorAlreadyRunning)
                {
                    // TODO There is a potential for a race-condition here.
                    // Can we do anything about this? Perhaps we should remove
                    // the above check, and just try to start the emulator. If it
                    // is already started, then we can handle the exception that
                    // is thrown and check at that point whether the emulator is
                    // running. This yields the opposite race condition, but this
                    // might be the case to optimise for.
                    _azureStorageEmulatorManager.Start()
                        .GetAwaiter()
                        .GetResult();
                }

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
#if DEBUG
                if (_useDevelopmentStorageAccount &&
                    !_emulatorAlreadyRunning)
                {
                    _azureStorageEmulatorManager.Stop()
                        .GetAwaiter()
                        .GetResult();
                }
#endif

                _disposed = true;
            }
        }
    }
}
