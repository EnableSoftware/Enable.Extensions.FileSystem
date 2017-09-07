using System;

namespace Enable.IO.Abstractions
{
    public interface IFile
    {
        string Path { get; }

        DateTimeOffset Created { get; }

        DateTimeOffset Modified { get; }
    }
}
