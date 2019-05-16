using System.IO;
using System.Linq;
using Microsoft.Azure.Storage.File;

namespace Enable.Extensions.FileSystem.AzureStorage.Internal
{
    public static class IListFileItemExtensions
    {
        public static string GetRelativeSubpath(this IListFileItem listFileItem)
        {
            // The first two segments of a file's URI are the root path, "/",
            // and a file share segment, e.g. "share-name/". When building up
            // the subpath to a file we exclude these two segments so that
            // subpaths are relative to the root share.
            var pathSegments = listFileItem.Uri.Segments.Skip(2);

            // Here we replace URI separators with directory path separators.
            pathSegments = pathSegments.Select(o => o.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));

            // Then join the remaining path segments together and return.
            return string.Join(string.Empty, pathSegments);
        }
    }
}
