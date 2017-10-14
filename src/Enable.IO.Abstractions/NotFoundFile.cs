using System;

namespace Enable.IO.Abstractions
{
    /// <summary>
    /// Represents a non-existant file.
    /// </summary>
    public class NotFoundFile : IFile
    {
        public NotFoundFile(string path)
        {
        }

        public bool Exists => false;

        public DateTimeOffset Created => DateTimeOffset.MinValue;

        public DateTimeOffset Modified => DateTimeOffset.MinValue;

        public string Path => null;
    }
}
