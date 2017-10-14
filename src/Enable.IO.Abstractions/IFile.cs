using System;

namespace Enable.IO.Abstractions
{
    /// <summary>
    /// Represents a file in the given file system.
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// Gets a value indicating whether the current file exists.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Gets the time, in coordinated universal time (UTC), that the
        /// current file was last written to.
        /// </summary>
        DateTimeOffset LastModified { get; }

        /// <summary>
        /// Gets the length, in bytes, of the current file, or -1  if the file is not directly accessible.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Gets the name of the file, not including any path.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the path to the file, including the file name. Returns `null` if the file is not directly accessible.
        /// </summary>
        string Path { get; }
    }
}
