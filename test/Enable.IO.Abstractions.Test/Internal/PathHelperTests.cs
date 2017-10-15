using Xunit;

namespace Enable.IO.Abstractions.Internal.Test
{
    public class PathHelperTests
    {
        [Theory]
        [InlineData("X:", "X:\\")]
        [InlineData("X:/foo", @"X:/foo\")]
        [InlineData(@"X:\foo", @"X:\foo\")]
        [InlineData(@"\\foo", @"\\foo\")]
        [InlineData("/foo", "/foo\\")]
        public void EnsureTrailingPathSeparator_AddsMissingTrailingSeparator(string input, string expected)
        {
            var result = PathHelper.EnsureTrailingPathSeparator(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("X:/")]
        [InlineData("X:/foo/")]
        [InlineData(@"X:/foo\")]
        [InlineData(@"X:\")]
        [InlineData(@"X:\foo\")]
        [InlineData(@"X:\foo/")]
        [InlineData(@"\\")]
        [InlineData(@"\\foo\")]
        [InlineData(@"\\foo/")]
        [InlineData("/")]
        public void EnsureTrailingPathSeparator_DoesNotAppendDuplicateSeparator(string input)
        {
            var result = PathHelper.EnsureTrailingPathSeparator(input);

            Assert.Equal(input, result);
        }

        [Theory]
        [InlineData(@"X:\foo", "bar", @"X:\foo\bar")]
        [InlineData("X:/foo", "bar", @"X:\foo\bar")]
        [InlineData(@"\\foo", "bar", @"\\foo\bar")]
        public void GetFullPath_ReturnsCombinedAbsolutePath(string path1, string path2, string expected)
        {
            var result = PathHelper.GetFullPath(path1, path2);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(@"X:", @"X:\foo")]
        [InlineData(@"X:\", @"X:\foo")]
        [InlineData(@"X:\foo", @"X:\foo")]
        [InlineData(@"X:\foo", @"X:\foo\")]
        [InlineData(@"X:\foo\", @"X:\foo")]
        [InlineData(@"X:\foo\", @"X:\foo\bar")]
        [InlineData(@"X:\foo\", @"X:\foo\bar\")]
        [InlineData(@"X:\foo\", @"X:\foo\bar.txt")]
        [InlineData(@"X:", "X:/foo")]
        [InlineData(@"X:\", "X:/foo")]
        [InlineData(@"X:\foo", "X:/foo")]
        [InlineData(@"X:\foo", "X:/foo/")]
        [InlineData(@"X:\foo\", "X:/foo")]
        [InlineData(@"X:\foo\", "X:/foo/bar")]
        [InlineData(@"X:\foo\", "X:/foo/bar/")]
        [InlineData(@"X:\foo\", "X:/foo/bar.txt")]
        [InlineData("X:/", @"X:\foo")]
        [InlineData("X:/foo", @"X:\foo")]
        [InlineData("X:/foo", @"X:\foo\")]
        [InlineData("X:/foo/", @"X:\foo")]
        [InlineData("X:/foo/", @"X:\foo\bar")]
        [InlineData("X:/foo/", @"X:\foo\bar\")]
        [InlineData("X:/foo/", @"X:\foo\bar.txt")]
        public void IsUnderneathRoot_ReturnsTrueIfPathIsASubPath(string root, string path)
        {
            var result = PathHelper.IsUnderneathRoot(root, path);

            Assert.True(result);
        }

        [Theory]
        [InlineData(@"X:\foo", @"Y:\foo")]
        [InlineData(@"X:\foo", @"X:\foobar")]
        [InlineData(@"X:\foo", @"X:\foo\..\bar")]
        public void IsUnderneathRoot_ReturnsFalseIfPathIsNotASubPath(string root, string path)
        {
            var result = PathHelper.IsUnderneathRoot(root, path);

            Assert.False(result);
        }
    }
}
