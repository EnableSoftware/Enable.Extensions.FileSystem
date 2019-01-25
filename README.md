# Enable.Extensions.FileSystem

[![Build status](https://ci.appveyor.com/api/projects/status/prwu1wi9g23p7p5a/branch/master?svg=true)](https://ci.appveyor.com/project/EnableSoftware/enable-extensions-filesystem/branch/master)

File system abstractions for building testable applications.

Writing code that interacts with files can be a pain. Testing this code can be
even more of a pain.

`Enable.Extensions.FileSystem` provides a flexible set of file system
abstractions that allow you to write the same code for working with files
regardless of whether you are interacting with files on your local machine
or files stored in a cloud offering such as [Azure Storage].

Best of all, these abstractions allow you to easily mock file system access
during tests, making your unit tests run faster and more predictably.

`Enable.Extensions.FileSystem` currently provides two file system implementations:

- [`Enable.Extensions.FileSystem.Physical`]: A physical, i.e. on-disk, implementation.

- [`Enable.Extensions.FileSystem.AzureStorage`]: An [Azure Storage] implementation.

In addition to these packages, an additional [`Enable.Extensions.FileSystem.Abstractions`]
package is available. This contains the basic abstractions that the implementations
listed above build upon. Use [`Enable.Extensions.FileSystem.Abstractions`] to implement
your own file system provider.

Package name                                | NuGet version
--------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
`Enable.Extensions.FileSystem.Abstractions` | [![NuGet](https://img.shields.io/nuget/v/Enable.Extensions.FileSystem.Abstractions.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Enable.Extensions.FileSystem.Abstractions/)
`Enable.Extensions.FileSystem.AzureStorage` | [![NuGet](https://img.shields.io/nuget/v/Enable.Extensions.FileSystem.AzureStorage.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Enable.Extensions.FileSystem.AzureStorage/)
`Enable.Extensions.FileSystem.Physical`     | [![NuGet](https://img.shields.io/nuget/v/Enable.Extensions.FileSystem.Physical.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Enable.Extensions.FileSystem.Physical/)


## Examples

The following example demonstrates creating, reading and deleting a file.

```csharp
using System;
using System.Threading.Tasks;
using Enable.Extensions.FileSystem.Physical;

namespace FileSystemSamples
{
    public class Program
    {
        public static void Main() => MainAsync().GetAwaiter().GetResult();

        public static async Task MainAsync()
        {
            // We start by declaring the absolute path to the directory we
            // want to work with.
            var directory = @"C:\some\absolute\path";
            
            // We then construct a represention of the physical file system
            // under this directory. Note that the files we want to work must
            // be rooted under `directory`. If we try walking out of this
            // directory then we'll be hit with an exception.
            using (var fileSystem = new FileSystem(directory))
            {
                // Let's start with some content that we want to write to disk.
                var content = "Hello, World!";

                // Files are identified by relative paths, with paths relative
                // to `directory`, which we specified above. Here we declare
                // the file we want to work with.
                var filePath = @"relative\path\to\file.txt";

                // We then save this text to this file.
                await fileSystem.SaveFileAsync(filePath, content);

                // Let's now check that this file now exists.
                var fileInfo = await fileSystem.GetFileInfoAsync(filePath);

                // The following will print `True`.
                Console.WriteLine(fileInfo.Exists) 

                // Now let's try and read the contents of this file.
                using (var stream = await _sut.GetFileStreamAsync(filePath))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    // Here `text` will be the string `"Hello, World!"`.
                    var text = reader.ReadToEnd();
                }

                // Finally, let's be good citizens and clean up after ourselves
                // by deleting the file we just created. If we don't do this,
                // the file will long on forever, long after we dispose of our
                // `fileSystem`.
                await fileSystem.DeleteFileAsync(filePath);
            }
        }
    }
}
```

Here we're using the physical file system implementation to interact with our
files. However, how we work with files is the same across any of the file
system implementations. The only differences are in how you initially construct
the file system provider.

For example, to work with files stored in an Azure Storage account, all we
need to do is:

1. Bring in the namespace ` Enable.Extensions.FileSystem.AzureStorage`;

2. Change the line:

   ```csharp
   using (var fileSystem = new FileSystem(directory))
   ```

   to:

   ```csharp
   using (var fileSystem = new AzureBlobStorage("account-name", "account-key", "container-name", "BlockBlob"))
   ```

The rest of the sample remains unchanged!

Here `account-name` is the name of your own Azure Storage account and
`account-key` the key to use to access the account. The steps you need to
take to obtain this from the Azure Portal can be found in the article
[Configure Azure Storage connection strings]. Here we're using
[Azure Blob Storage], so we need to specify a container name, which we
do as the third parameter to the `AzureBlobStorage` constructor.
The final parameter `BlockBlob` is to specify which type of Blob storage to
use, the options are `BlockBlob`, `AppendBlob` and `PageBlob`, if no value is passed it will default to `BlockBlob`.

[Azure Storage]: https://azure.microsoft.com/services/storage/
[Azure Blob Storage]: https://azure.microsoft.com/services/storage/blobs/
[Configure Azure Storage connection strings]: https://docs.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string

[`Enable.Extensions.FileSystem.Abstractions`]: https://www.nuget.org/packages/Enable.Extensions.FileSystem.Abstractions/
[`Enable.Extensions.FileSystem.AzureStorage`]: https://www.nuget.org/packages/Enable.Extensions.FileSystem.AzureStorage/
[`Enable.Extensions.FileSystem.Physical`]: https://www.nuget.org/packages/Enable.Extensions.FileSystem.Physical/
