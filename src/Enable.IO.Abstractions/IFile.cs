using System;

namespace Enable.IO.Abstractions
{
    public interface IFile
    {
        bool Exists { get; }

        string Path { get; }

        DateTimeOffset Created { get; }

        DateTimeOffset Modified { get; }
    }
}
