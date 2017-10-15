using System;
using System.IO;

namespace Enable.Extensions.FileSystem.Internal
{
    internal static class PathHelper
    {
        internal static string EnsureTrailingPathSeparator(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (path[path.Length - 1] != Path.DirectorySeparatorChar &&
                    path[path.Length - 1] != Path.AltDirectorySeparatorChar)
                {
                    return path + Path.DirectorySeparatorChar;
                }
            }

            return path;
        }

        internal static string GetFullPath(string root, string path)
        {
            return Path.GetFullPath(Path.Combine(root, path));
        }

        internal static bool IsUnderneathRoot(string root, string path)
        {
            var normalisedRootPath = NormalisePath(root);
            var normalisedSubPath = NormalisePath(path);

            return normalisedSubPath.StartsWith(
                normalisedRootPath,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalisePath(string path)
        {
            var normalisedPath = Path.GetFullPath(EnsureTrailingPathSeparator(path))
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return normalisedPath;
        }
    }
}
