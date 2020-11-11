//using System;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Azure.Storage;
//using Microsoft.Azure.Storage.File;
//using Xunit;

//namespace Enable.Extensions.FileSystem.Test
//{
//    /// <summary>
//    /// Tests for the Azure File Storage file storage implementation.
//    /// </summary>
//    /// <remarks>
//    /// These tests require a connection to an Azure Storage account, since the
//    /// Azure Storage Emulator does not yet support Azure File Storage.
//    /// </remarks>
//    public class AzureFileStorageTests : IClassFixture<AzureStorageTestFixture>, IDisposable
//    {
//        private readonly CloudFileShare _fileShare;
//        private readonly AzureFileStorage _sut;

//        private bool _disposed;

//        public AzureFileStorageTests(AzureStorageTestFixture fixture)
//        {
//            var storageAccount = fixture.StorageAccount;

//            var storageClient = storageAccount.CreateCloudFileClient();

//            var fileShareName = Guid.NewGuid().ToString();

//            _fileShare = storageClient.GetShareReference(fileShareName);

//            _fileShare.CreateIfNotExistsAsync()
//                .GetAwaiter()
//                .GetResult();

//            _sut = new AzureFileStorage(storageClient, fileShareName);
//        }

//        [Fact]
//        public async Task CopyFileAsync_SucceedsIfSourceFileExists()
//        {
//            // Arrange
//            var source = Path.GetRandomFileName();
//            var target = Path.GetRandomFileName();

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, source);

//            // Act
//            await _sut.CopyFileAsync(source, target);

//            // Assert
//            Assert.True(await AzureFileStorageTestHelper.ExistsAsync(_fileShare, source));
//            Assert.True(await AzureFileStorageTestHelper.ExistsAsync(_fileShare, target));
//        }

//        [Fact]
//        public async Task CopyFileAsync_CanMoveAcrossSubDirectories()
//        {
//            // Arrange
//            var source = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());
//            var target = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, source);

//            // Act
//            await _sut.CopyFileAsync(source, target);

//            // Assert
//            Assert.True(await AzureFileStorageTestHelper.ExistsAsync(_fileShare, source));
//            Assert.True(await AzureFileStorageTestHelper.ExistsAsync(_fileShare, target));
//        }

//        [Fact]
//        public async Task CopyFileAsync_ThrowsIfFileDoesNotExist()
//        {
//            // Arrange
//            var source = Path.GetRandomFileName();
//            var target = Path.GetRandomFileName();

//            // Act
//            var exception = await Record.ExceptionAsync(
//                () => _sut.CopyFileAsync(source, target));

//            // Assert
//            Assert.IsAssignableFrom<StorageException>(exception);
//        }

//        [Fact]
//        public async Task DeleteDirectoryAsync_CanDeleteFromSubDirectory()
//        {
//            // Arrange
//            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());
//            var directoryName = Path.GetRandomFileName();

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, Path.Combine(directoryName, fileName));

//            // Act
//            await _sut.DeleteDirectoryAsync(directoryName);

//            // Assert
//            Assert.False(await AzureFileStorageTestHelper.DirectoryExistsAsync(_fileShare, directoryName));
//        }

//        [Fact]
//        public async Task DeleteDirectoryAsync_DoesNotThrowIfDirectoryDoesNotExist()
//        {
//            // Arrange
//            var directoryName = AzureFileStorageTestHelper.CreateRandomString();

//            // Act
//            await _sut.DeleteDirectoryAsync(directoryName);
//        }

//        [Fact]
//        public async Task DeleteDirectoryAsync_SucceedsIfDirectoryExists()
//        {
//            // Arrange
//            var directoryName = AzureFileStorageTestHelper.CreateRandomString();

//            var numberOfFilesToCreate = AzureFileStorageTestHelper.CreateRandomNumber(
//                minValue: 1, // Ensure that we always create at least one test file.
//                maxValue: byte.MaxValue);

//            await AzureFileStorageTestHelper.CreateTestDirectoryAsync(_fileShare, directoryName);
//            await AzureFileStorageTestHelper.CreateTestFilesAsync(_fileShare, numberOfFilesToCreate, directoryName);

//            // Act
//            await _sut.DeleteDirectoryAsync(directoryName);

//            // Assert
//            Assert.False(await AzureFileStorageTestHelper.DirectoryExistsAsync(_fileShare, directoryName));
//        }

//        [Fact]
//        public async Task DeleteDirectoryAsync_SucceedsIfDirectoryExistsAndEmpty()
//        {
//            // Arrange
//            var directoryName = AzureFileStorageTestHelper.CreateRandomString();
//            await AzureFileStorageTestHelper.CreateTestDirectoryAsync(_fileShare, directoryName);

//            // Act
//            await _sut.DeleteDirectoryAsync(directoryName);

//            // Assert
//            Assert.False(await AzureFileStorageTestHelper.DirectoryExistsAsync(_fileShare, directoryName));
//        }

//        [Fact]
//        public async Task DeleteFileAsync_SucceedsIfFileExists()
//        {
//            // Arrange
//            var fileName = Path.GetRandomFileName();

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, fileName);

//            // Act
//            await _sut.DeleteFileAsync(fileName);

//            // Assert
//            Assert.False(await AzureFileStorageTestHelper.ExistsAsync(_fileShare, fileName));
//        }

//        [Fact]
//        public async Task DeleteFileAsync_CanDeleteFromSubDirectory()
//        {
//            // Arrange
//            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, fileName);

//            // Act
//            await _sut.DeleteFileAsync(fileName);

//            // Assert
//            Assert.False(await AzureFileStorageTestHelper.ExistsAsync(_fileShare, fileName));
//        }

//        [Fact]
//        public async Task DeleteFileAsync_DoesNotThrowIfFileDoesNotExist()
//        {
//            // Arrange
//            var fileName = Path.GetRandomFileName();

//            // Act
//            await _sut.DeleteFileAsync(fileName);
//        }

//        [Fact]
//        public async Task GetDirectoryContentsAsync_ReturnsEmptyListForEmptyDirectory()
//        {
//            // Act
//            var result = await _sut.GetDirectoryContentsAsync(string.Empty);

//            // Assert
//            Assert.Empty(result);
//        }

//        [Fact]
//        public async Task GetDirectoryContentsAsync_ReturnsFileList()
//        {
//            // Arrange
//            var filesCount = AzureFileStorageTestHelper.CreateRandomNumber();

//            await AzureFileStorageTestHelper.CreateTestFilesAsync(_fileShare, filesCount);

//            // Act
//            var result = await _sut.GetDirectoryContentsAsync(string.Empty);

//            // Assert
//            Assert.Equal(filesCount, result.Count());
//        }

//        [Fact]
//        public async Task GetDirectoryContentsAsync_ReturnsFileListForSubDirectory()
//        {
//            // Arrange
//            var subpath = Path.GetRandomFileName();

//            var filesCount = AzureFileStorageTestHelper.CreateRandomNumber();

//            await AzureFileStorageTestHelper.CreateTestFilesAsync(
//                _fileShare,
//                filesCount,
//                subpath);

//            // Act
//            var result = await _sut.GetDirectoryContentsAsync(subpath);

//            // Assert
//            Assert.Equal(filesCount, result.Count());
//        }

//        [Fact]
//        public async Task GetDirectoryContentsAsync_ReturnsNotFoundDirectoryIfDirectoryDoesNotExist()
//        {
//            // Arrange
//            var path = Path.GetRandomFileName();

//            // Act
//            var result = await _sut.GetDirectoryContentsAsync(path);

//            // Assert
//            Assert.False(result.Exists);
//        }

//        [Fact]
//        public async Task GetFileInfoAsync_ReturnsFileInfoIfFileExists()
//        {
//            // Arrange
//            var fileName = Path.GetRandomFileName();

//            var expectedFileName = Path.GetFileName(fileName);
//            var expectedFilePath = fileName;

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, fileName);

//            // Act
//            var result = await _sut.GetFileInfoAsync(fileName);

//            // Assert
//            Assert.True(result.Exists);
//            Assert.False(result.IsDirectory);
//            Assert.Equal(expectedFileName, result.Name);
//            Assert.Equal(expectedFilePath, result.Path);
//        }

//        [Fact]
//        public async Task GetFileInfoAsync_ReturnsFileInfoForFileInSubDirectory()
//        {
//            // Arrange
//            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

//            var expectedFileName = Path.GetFileName(fileName);
//            var expectedFilePath = fileName;

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, fileName);

//            // Act
//            var result = await _sut.GetFileInfoAsync(fileName);

//            // Assert
//            Assert.True(result.Exists);
//            Assert.False(result.IsDirectory);
//            Assert.Equal(expectedFileName, result.Name);
//            Assert.Equal(expectedFilePath, result.Path);
//        }

//        [Fact]
//        public async Task GetFileInfoAsync_ReturnsNotFoundFileIfFileDoesNotExist()
//        {
//            // Arrange
//            var fileName = Path.GetRandomFileName();

//            // Act
//            var result = await _sut.GetFileInfoAsync(fileName);

//            // Assert
//            Assert.False(result.Exists);
//            Assert.Equal(fileName, result.Name);
//        }

//        [Fact]
//        public async Task GetFileStreamAsync_ReturnsFileStreamIfFileExists()
//        {
//            // Arrange
//            var fileName = Path.GetRandomFileName();

//            var expectedContents = AzureFileStorageTestHelper.CreateRandomString();

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, fileName, expectedContents);

//            // Act
//            using (var stream = await _sut.GetFileStreamAsync(fileName))
//            using (var reader = new StreamReader(stream, Encoding.UTF8))
//            {
//                var contents = reader.ReadToEnd();

//                // Assert
//                Assert.Equal(expectedContents, contents);
//            }
//        }

//        [Fact]
//        public async Task GetFileStreamAsync_ReturnsFileStreamForFileInSubDirectory()
//        {
//            // Arrange
//            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

//            var expectedContents = AzureFileStorageTestHelper.CreateRandomString();

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, fileName, expectedContents);

//            // Act
//            using (var stream = await _sut.GetFileStreamAsync(fileName))
//            using (var reader = new StreamReader(stream, Encoding.UTF8))
//            {
//                var contents = reader.ReadToEnd();

//                // Assert
//                Assert.Equal(expectedContents, contents);
//            }
//        }

//        [Fact]
//        public async Task GetFileStreamAsync_ThrowsIfFileDoesNotExist()
//        {
//            // Arrange
//            var fileName = Path.GetRandomFileName();

//            // Act
//            var exception = await Record.ExceptionAsync(() => _sut.GetFileStreamAsync(fileName));

//            // Assert
//            Assert.IsAssignableFrom<StorageException>(exception);
//        }

//        [Fact]
//        public async Task GetFileStreamAsync_ThrowsIfFileIsNotSpecified()
//        {
//            // Arrange
//            var fileName = Path.GetRandomFileName();

//            // Act
//            var exception = await Record.ExceptionAsync(
//                () => _sut.GetFileStreamAsync(fileName));

//            // Assert
//            Assert.IsAssignableFrom<StorageException>(exception);
//        }

//        [Fact]
//        public async Task RenameFileAsync_SucceedsIfSourceFileExists()
//        {
//            // Arrange
//            var source = Path.GetRandomFileName();
//            var target = Path.GetRandomFileName();

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, source);

//            // Act
//            await _sut.RenameFileAsync(source, target);

//            // Assert
//            Assert.False(await AzureFileStorageTestHelper.ExistsAsync(_fileShare, source));
//            Assert.True(await AzureFileStorageTestHelper.ExistsAsync(_fileShare, target));
//        }

//        [Fact]
//        public async Task RenameFileAsync_CanRenameAcrossSubDirectories()
//        {
//            // Arrange
//            var source = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());
//            var target = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

//            await AzureFileStorageTestHelper.CreateTestFileAsync(_fileShare, source);

//            // Act
//            await _sut.RenameFileAsync(source, target);

//            // Assert
//            Assert.False(await AzureFileStorageTestHelper.ExistsAsync(_fileShare, source));
//            Assert.True(await AzureFileStorageTestHelper.ExistsAsync(_fileShare, target));
//        }

//        [Fact]
//        public async Task RenameFileAsync_ThrowsIfFileDoesNotExist()
//        {
//            // Arrange
//            var source = Path.GetRandomFileName();
//            var target = Path.GetRandomFileName();

//            // Act
//            var exception = await Record.ExceptionAsync(
//                () => _sut.RenameFileAsync(source, target));

//            // Assert
//            Assert.IsAssignableFrom<StorageException>(exception);
//        }

//        [Fact]
//        public async Task SaveFileAsync_Succeeds()
//        {
//            // Arrange
//            var fileName = Path.GetRandomFileName();

//            var contents = AzureFileStorageTestHelper.CreateRandomString();

//            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
//            {
//                // Act
//                await _sut.SaveFileAsync(fileName, stream);
//            }
//        }

//        [Fact]
//        public async Task SaveFileAsync_SucceedsForFileInSubDirectory()
//        {
//            // Arrange
//            var fileName = Path.Combine(Path.GetRandomFileName(), Path.GetRandomFileName());

//            var contents = AzureFileStorageTestHelper.CreateRandomString();

//            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
//            {
//                // Act
//                await _sut.SaveFileAsync(fileName, stream);
//            }
//        }

//        [Fact]
//        public async Task SaveFileAsync_SavesCorrectContent()
//        {
//            // Arrange
//            var fileName = Path.GetRandomFileName();

//            var expectedContents = AzureFileStorageTestHelper.CreateRandomString();

//            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContents)))
//            {
//                // Act
//                await _sut.SaveFileAsync(fileName, stream);
//            }

//            // Assert
//            var actualContents = await AzureFileStorageTestHelper.ReadFileContents(_fileShare, fileName);

//            Assert.Equal(expectedContents, actualContents);
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (_disposed)
//            {
//                return;
//            }

//            if (disposing)
//            {
//                try
//                {
//                    // Make a best effort to remove our temporary test share.
//                    _fileShare.DeleteIfExistsAsync()
//                        .GetAwaiter()
//                        .GetResult();
//                }
//                catch
//                {
//                }

//                _sut.Dispose();

//                _disposed = true;
//            }
//        }
//    }
//}
