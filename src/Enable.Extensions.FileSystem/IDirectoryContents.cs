using System.Collections.Generic;

namespace Enable.Extensions.FileSystem
{
    /// <summary>
    /// Represents the contents of a file system directory.
    /// </summary>
    public interface IDirectoryContents : IEnumerable<IFile>
    {
        /// <summary>
        /// Gets a value indicating whether the directory was located at the given path.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Gets the path to the directory, including the directory name. Returns `null` if the file is not directly accessible.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the name of the directory, not including any path.
        /// </summary>
        string Name { get; }
    }
}
