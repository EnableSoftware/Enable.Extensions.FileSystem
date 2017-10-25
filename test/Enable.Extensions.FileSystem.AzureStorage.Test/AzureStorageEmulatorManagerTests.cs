using System.Threading.Tasks;
using Xunit;

namespace Enable.Extensions.FileSystem
{
    public class AzureStorageEmulatorManagerTests
    {
        private readonly AzureStorageEmulatorManager _sut;

        public AzureStorageEmulatorManagerTests()
        {
            _sut = AzureStorageEmulatorManager.Instance;
        }

        [Fact]
        public async Task GetIsEmulatorRunning_ReturnsTrue_IfStorageEmulatorStarted()
        {
            // Arrange
            await EnsureEmulatorIsRunning();

            // Act
            var isRunning = await _sut.GetIsEmulatorRunning();

            // Assert
            Assert.True(isRunning);
        }

        [Fact]
        public async Task GetIsEmulatorRunning_ReturnsFalse_IfStorageEmulatorStopped()
        {
            // Arrange
            await EnsureEmulatorIsStopped();

            // Act
            var isRunning = await _sut.GetIsEmulatorRunning();

            // Assert
            Assert.False(isRunning);
        }

        [Fact]
        public async Task Start_StartsStorageEmulator()
        {
            // Arrange
            await EnsureEmulatorIsStopped();

            // Act
            await _sut.Start();

            // Assert
            await VerifyAzureStorageEmulatorIsRunning();
        }

        [Fact]
        public async Task Stop_StopsStorageEmulator()
        {
            // Arrange
            await EnsureEmulatorIsStopped();

            await _sut.Start();

            // Act
            await _sut.Stop();

            // Assert
            await VerifyAzureStorageEmulatorIsStopped();
        }

        private async Task EnsureEmulatorIsRunning()
        {
            // Here we use a separate reference to the Azure Storage Emulator.
            // This is the same instance as the SUT. Review if this is sensible.
            var otherInstance = AzureStorageEmulatorManager.Instance;

            await otherInstance.Start();

            await VerifyAzureStorageEmulatorIsRunning();
        }

        private async Task EnsureEmulatorIsStopped()
        {
            // Here we use a separate reference to the Azure Storage Emulator.
            // This is the same instance as the SUT. Review if this is sensible.
            var otherInstance = AzureStorageEmulatorManager.Instance;

            await otherInstance.Stop();

            await VerifyAzureStorageEmulatorIsStopped();
        }

        private async Task VerifyAzureStorageEmulatorIsRunning()
        {
            // Here we use a separate reference to the Azure Storage Emulator.
            // This is the same instance as the SUT. Review if this is sensible.
            var otherInstance = AzureStorageEmulatorManager.Instance;

            var isRunning = await otherInstance.GetIsEmulatorRunning();

            Assert.True(isRunning, "Azure storage emulator stopped unexpectedly.");

            //// TODO Actually try and interact with the running emulator instance using the dev. storage account.
        }

        private async Task VerifyAzureStorageEmulatorIsStopped()
        {
            // Here we use a separate reference to the Azure Storage Emulator.
            // This is the same instance as the SUT. Review if this is sensible.
            var otherInstance = AzureStorageEmulatorManager.Instance;

            var isRunning = await otherInstance.GetIsEmulatorRunning();

            Assert.False(isRunning, "Azure storage emulator started unexpectedly.");
        }
    }
}
