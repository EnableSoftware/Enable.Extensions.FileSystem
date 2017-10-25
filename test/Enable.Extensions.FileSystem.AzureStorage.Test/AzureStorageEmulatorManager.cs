using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Enable.Extensions.FileSystem
{
    public sealed class AzureStorageEmulatorManager
    {
        private static volatile AzureStorageEmulatorManager _instance;
        private static object _syncRoot = new object();

        private AzureStorageEmulatorManager()
        {
        }

        public static AzureStorageEmulatorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new AzureStorageEmulatorManager();
                        }
                    }
                }

                return _instance;
            }
        }

        public async Task<bool> GetIsEmulatorRunning()
        {
            var result = await InvokeStorageEmulator(command: "status");

            if (result.StandardOutput.IndexOf("IsRunning: True", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        public Task Start()
        {
            return InvokeStorageEmulator(command: "start");
        }

        public Task Stop()
        {
            return InvokeStorageEmulator("stop");
        }

        public Task ClearAll()
        {
            return InvokeStorageEmulator("clear all");
        }

        public Task ClearBlobs()
        {
            return InvokeStorageEmulator("clear blob");
        }

        public Task ClearQueues()
        {
            return InvokeStorageEmulator("clear queue");
        }

        public Task ClearTables()
        {
            return InvokeStorageEmulator("clear table");
        }

        private static Task<ProcessResult> InvokeStorageEmulator(string command)
        {
            var path = GetStorageEmulatorExecutablePath();

            var processStartInfo = new ProcessStartInfo(path, command);

            return ProcessExecutor.Execute(processStartInfo);
        }

        private static string GetStorageEmulatorExecutablePath()
        {
            var storageEmulatorDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft SDKs",
                "Azure",
                "Storage Emulator");

            var storageEmulatorExecutablePath = Path.Combine(
                storageEmulatorDirectory,
                "AzureStorageEmulator.exe");

            if (!File.Exists(storageEmulatorExecutablePath))
            {
                throw new FileNotFoundException(
                   "Unable to locate the Azure Storage Emulator executable.",
                   storageEmulatorExecutablePath);
            }

            return storageEmulatorExecutablePath;
        }
    }
}
