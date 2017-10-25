using System;

namespace Enable.Extensions.FileSystem
{
    public class AzureStorageEmulatorFixture : IDisposable
    {
        private readonly AzureStorageEmulatorManager _azureStorageEmulatorManager;
        private readonly bool _emulatorAlreadyRunning;

        private bool _disposed;

        public AzureStorageEmulatorFixture()
        {
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
        }

        internal AzureStorageEmulatorManager AzureStorageEmulator
        {
            get
            {
                return AzureStorageEmulatorManager.Instance;
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
                if (!_emulatorAlreadyRunning)
                {
                    _azureStorageEmulatorManager.Stop()
                        .GetAwaiter()
                        .GetResult();
                }

                _disposed = true;
            }
        }
    }
}
