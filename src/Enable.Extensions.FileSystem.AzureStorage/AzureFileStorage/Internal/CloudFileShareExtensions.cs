//using System;
//using System.IO;
//using Microsoft.Azure.Storage.File;

//namespace Enable.Extensions.FileSystem.AzureStorage.Internal
//{
//    internal static class CloudFileShareExtensions
//    {
//        // `CloudFileDirectory.GetFileReference` expects a file name, not a
//        // path to a file. Calling it with a relative path will create a
//        // reference to a file in the root directory of the file share, with
//        // the directory segments included in the file name, rather than
//        // creating a reference to a file in a subdirectory of the root file
//        // share directory. We therefore we need to "walk" the path segments
//        // when building a file reference from a file path.
//        public static CloudFile GetFileReference(this CloudFileShare fileShare, string path)
//        {
//            var fileName = Path.GetFileName(path);
//            var directoryPath = Path.GetDirectoryName(path);

//            var parentDirectory = fileShare.GetDirectoryReference(directoryPath);

//            return parentDirectory.GetFileReference(fileName);
//        }

//        public static CloudFileDirectory GetDirectoryReference(this CloudFileShare fileShare, string path)
//        {
//            // Absolute paths not permitted. This will throw if path contains invalid characters.
//            if (Path.IsPathRooted(path))
//            {
//                throw new InvalidOperationException("Absolute paths are not permitted.");
//            }

//            var pathSegments = GetDirectoryPathSegments(path);

//            var parentDirectory = fileShare.GetRootDirectoryReference();

//            foreach (var segment in pathSegments)
//            {
//                parentDirectory = parentDirectory.GetDirectoryReference(segment);
//            }

//            return parentDirectory;
//        }

//        private static string EnsureTrailingPathSeparator(string path)
//        {
//            if (!string.IsNullOrWhiteSpace(path))
//            {
//                if (path[path.Length - 1] != Path.DirectorySeparatorChar &&
//                    path[path.Length - 1] != Path.AltDirectorySeparatorChar)
//                {
//                    return path + Path.DirectorySeparatorChar;
//                }
//            }

//            return path;
//        }

//        private static string[] GetDirectoryPathSegments(string path)
//        {
//            if (string.IsNullOrWhiteSpace(path))
//            {
//                return new string[0];
//            }

//            path = EnsureTrailingPathSeparator(path);

//            var pathSegments = Path.GetDirectoryName(path)
//                .Split(
//                    new[]
//                    {
//                        Path.DirectorySeparatorChar,
//                        Path.AltDirectorySeparatorChar
//                    },
//                    StringSplitOptions.RemoveEmptyEntries);

//            return pathSegments;
//        }
//    }
//}
