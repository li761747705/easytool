using Xunit;
using System;
using System.IO;
using System.Linq;

namespace EasyTool.IOCategory.Tests
{
    public class PathUtilTests : IDisposable
    {
        private readonly string _testDir;

        public PathUtilTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "EasyTool_PathUtilTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        #region Combine

        [Fact]
        public void Combine_TwoPaths_CombinesCorrectly()
        {
            var result = PathUtil.Combine("folder", "file.txt");
            Assert.EndsWith(Path.Combine("folder", "file.txt"), result);
        }

        [Fact]
        public void Combine_MultiplePaths_CombinesCorrectly()
        {
            var result = PathUtil.Combine("a", "b", "c", "file.txt");
            Assert.Contains("file.txt", result);
            Assert.Contains("a", result);
        }

        [Fact]
        public void Combine_SinglePath_ReturnsPath()
        {
            var result = PathUtil.Combine("folder");
            Assert.Equal("folder", result);
        }

        #endregion

        #region GetFullPath

        [Fact]
        public void GetFullPath_AbsolutePath_ReturnsFullPath()
        {
            var path = Path.GetTempPath();
            var result = PathUtil.GetFullPath(path);
            Assert.Equal(Path.GetFullPath(path), result);
        }

        [Fact]
        public void GetFullPath_RelativePath_ReturnsFullPath()
        {
            var result = PathUtil.GetFullPath("subfolder");
            Assert.True(Path.IsPathRooted(result));
        }

        [Fact]
        public void GetFullPath_WithBasePath_ResolvesRelativeToBase()
        {
            var result = PathUtil.GetFullPath("file.txt", _testDir);
            Assert.StartsWith(_testDir, result);
        }

        [Fact]
        public void GetFullPath_NullOrEmpty_ReturnsInput()
        {
            Assert.Null(PathUtil.GetFullPath(null));
            Assert.Equal(string.Empty, PathUtil.GetFullPath(string.Empty));
        }

        #endregion

        #region GetRelativePath

        [Fact]
        public void GetRelativePath_ReturnsRelativePath()
        {
            var baseDir = Path.Combine(_testDir, "base");
            var targetFile = Path.Combine(_testDir, "base", "sub", "file.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);

            var result = PathUtil.GetRelativePath(baseDir, targetFile);
            Assert.Equal(Path.Combine("sub", "file.txt"), result);
        }

        #endregion

        #region GetFileName

        [Fact]
        public void GetFileName_WithExtension_ReturnsFileName()
        {
            var result = PathUtil.GetFileName("/path/to/file.txt");
            Assert.Equal("file.txt", result);
        }

        [Fact]
        public void GetFileName_NoExtension_ReturnsFileName()
        {
            var result = PathUtil.GetFileName("/path/to/file");
            Assert.Equal("file", result);
        }

        [Fact]
        public void GetFileName_EmptyPath_ReturnsEmpty()
        {
            var result = PathUtil.GetFileName("");
            Assert.Equal("", result);
        }

        #endregion

        #region GetFileNameWithoutExtension

        [Fact]
        public void GetFileNameWithoutExtension_ReturnsNameWithoutExtension()
        {
            var result = PathUtil.GetFileNameWithoutExtension("/path/to/file.txt");
            Assert.Equal("file", result);
        }

        [Fact]
        public void GetFileNameWithoutExtension_NoExtension_ReturnsFileName()
        {
            var result = PathUtil.GetFileNameWithoutExtension("/path/to/file");
            Assert.Equal("file", result);
        }

        [Fact]
        public void GetFileNameWithoutExtension_MultipleDots_ReturnsNameBeforeLastDot()
        {
            var result = PathUtil.GetFileNameWithoutExtension("/path/to/file.min.js");
            Assert.Equal("file.min", result);
        }

        #endregion

        #region GetExtension

        [Fact]
        public void GetExtension_WithExtension_ReturnsExtension()
        {
            var result = PathUtil.GetExtension("/path/to/file.txt");
            Assert.Equal(".txt", result);
        }

        [Fact]
        public void GetExtension_NoExtension_ReturnsEmpty()
        {
            var result = PathUtil.GetExtension("/path/to/file");
            Assert.Equal("", result);
        }

        [Fact]
        public void GetExtension_EmptyPath_ReturnsEmpty()
        {
            var result = PathUtil.GetExtension("");
            Assert.Equal("", result);
        }

        #endregion

        #region GetDirectoryName

        [Fact]
        public void GetDirectoryName_ReturnsParentDirectory()
        {
            var result = PathUtil.GetDirectoryName("/path/to/file.txt");
            Assert.NotNull(result);
            Assert.EndsWith(Path.Combine("path", "to"), result);
        }

        [Fact]
        public void GetDirectoryName_RootPath_ReturnsNullOrEmpty()
        {
            var result = PathUtil.GetDirectoryName("file.txt");
            // On Windows, Path.GetDirectoryName returns empty string for relative filenames
            Assert.True(string.IsNullOrEmpty(result));
        }

        #endregion

        #region ChangeExtension

        [Fact]
        public void ChangeExtension_ValidChange_ReturnsNewPath()
        {
            var result = PathUtil.ChangeExtension("/path/to/file.txt", ".md");
            Assert.Equal("/path/to/file.md", result);
        }

        [Fact]
        public void ChangeExtension_RemoveExtension_ReturnsPathWithoutExtension()
        {
            var result = PathUtil.ChangeExtension("/path/to/file.txt", null);
            Assert.Equal("/path/to/file", result);
        }

        [Fact]
        public void ChangeExtension_AddExtension_ReturnsPathWithExtension()
        {
            var result = PathUtil.ChangeExtension("/path/to/file", ".txt");
            Assert.Equal("/path/to/file.txt", result);
        }

        #endregion

        #region RemoveExtension

        [Fact]
        public void RemoveExtension_ReturnsPathWithoutExtension()
        {
            var result = PathUtil.RemoveExtension("/path/to/file.txt");
            Assert.Equal("/path/to/file", result);
        }

        [Fact]
        public void RemoveExtension_NoExtension_ReturnsSamePath()
        {
            var path = "/path/to/file";
            var result = PathUtil.RemoveExtension(path);
            Assert.Equal(path, result);
        }

        #endregion

        #region IsAbsolute / IsRelative

        [Fact]
        public void IsAbsolute_AbsolutePath_ReturnsTrue()
        {
            var path = Path.GetTempPath();
            Assert.True(PathUtil.IsAbsolute(path));
        }

        [Fact]
        public void IsAbsolute_RelativePath_ReturnsFalse()
        {
            Assert.False(PathUtil.IsAbsolute("folder/file.txt"));
        }

        [Fact]
        public void IsRelative_RelativePath_ReturnsTrue()
        {
            Assert.True(PathUtil.IsRelative("folder/file.txt"));
        }

        [Fact]
        public void IsRelative_AbsolutePath_ReturnsFalse()
        {
            var path = Path.GetTempPath();
            Assert.False(PathUtil.IsRelative(path));
        }

        #endregion

        #region Normalize

        [Fact]
        public void Normalize_ForwardSlash_ConvertsToDirectorySeparator()
        {
            var result = PathUtil.Normalize("a/b/c");
            Assert.Equal($"a{Path.DirectorySeparatorChar}b{Path.DirectorySeparatorChar}c", result);
        }

        [Fact]
        public void Normalize_Backslash_ConvertsToDirectorySeparator()
        {
            var result = PathUtil.Normalize("a\\b\\c");
            Assert.Equal($"a{Path.DirectorySeparatorChar}b{Path.DirectorySeparatorChar}c", result);
        }

        [Fact]
        public void Normalize_TrailingSeparator_RemovesTrailingSeparator()
        {
            var result = PathUtil.Normalize("a/b/c/");
            Assert.False(result.EndsWith(Path.DirectorySeparatorChar.ToString()));
        }

        [Fact]
        public void Normalize_EmptyPath_ReturnsEmpty()
        {
            Assert.Equal("", PathUtil.Normalize(""));
            Assert.Null(PathUtil.Normalize(null));
        }

        #endregion

        #region EnsureTrailingSeparator

        [Fact]
        public void EnsureTrailingSeparator_NoTrailing_AddsSeparator()
        {
            var result = PathUtil.EnsureTrailingSeparator("a/b");
            Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), result);
        }

        [Fact]
        public void EnsureTrailingSeparator_AlreadyHasTrailing_ReturnsSame()
        {
            var path = $"a/b{Path.DirectorySeparatorChar}";
            var result = PathUtil.EnsureTrailingSeparator(path);
            Assert.Equal(path, result);
        }

        [Fact]
        public void EnsureTrailingSeparator_EmptyPath_ReturnsEmpty()
        {
            Assert.Equal("", PathUtil.EnsureTrailingSeparator(""));
        }

        #endregion

        #region TrimTrailingSeparator

        [Fact]
        public void TrimTrailingSeparator_HasTrailing_RemovesSeparator()
        {
            var result = PathUtil.TrimTrailingSeparator("a/b/");
            Assert.False(result.EndsWith("/"));
        }

        [Fact]
        public void TrimTrailingSeparator_NoTrailing_ReturnsSame()
        {
            var path = "a/b";
            var result = PathUtil.TrimTrailingSeparator(path);
            Assert.Equal(path, result);
        }

        [Fact]
        public void TrimTrailingSeparator_EmptyPath_ReturnsEmpty()
        {
            Assert.Equal("", PathUtil.TrimTrailingSeparator(""));
        }

        #endregion

        #region GetParent

        [Fact]
        public void GetParent_ReturnsParentDirectory()
        {
            var result = PathUtil.GetParent("/path/to/file.txt");
            Assert.NotNull(result);
            Assert.Contains("to", result!);
        }

        [Fact]
        public void GetParent_EmptyPath_ReturnsNull()
        {
            Assert.Null(PathUtil.GetParent(""));
        }

        [Fact]
        public void GetParent_RootPath_ReturnsNull()
        {
            Assert.Null(PathUtil.GetParent("file.txt"));
        }

        #endregion

        #region GetParents

        [Fact]
        public void GetParents_ReturnsAllParentDirectories()
        {
            var path = Path.Combine(_testDir, "a", "b", "c");
            var parents = PathUtil.GetParents(path).ToList();
            Assert.True(parents.Count >= 2);
        }

        [Fact]
        public void GetParents_EmptyPath_ReturnsEmpty()
        {
            Assert.Empty(PathUtil.GetParents(""));
        }

        #endregion

        #region GetDepth

        [Fact]
        public void GetDepth_ReturnsCorrectDepth()
        {
            var path = Path.Combine("a", "b", "c");
            var depth = PathUtil.GetDepth(path);
            Assert.Equal(2, depth);
        }

        [Fact]
        public void GetDepth_EmptyPath_ReturnsZero()
        {
            Assert.Equal(0, PathUtil.GetDepth(""));
        }

        [Fact]
        public void GetDepth_SingleSegment_ReturnsZero()
        {
            Assert.Equal(0, PathUtil.GetDepth("file.txt"));
        }

        #endregion

        #region IsInDirectory

        [Fact]
        public void IsInDirectory_PathInDirectory_ReturnsTrue()
        {
            var dir = Path.Combine(_testDir, "sub");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, "file.txt");
            File.WriteAllText(file, "content");

            Assert.True(PathUtil.IsInDirectory(file, dir));
        }

        [Fact]
        public void IsInDirectory_PathOutsideDirectory_ReturnsFalse()
        {
            var otherDir = Path.Combine(_testDir, "other");
            Directory.CreateDirectory(otherDir);
            var file = Path.Combine(otherDir, "file.txt");
            File.WriteAllText(file, "content");

            var checkDir = Path.Combine(_testDir, "sub");
            Assert.False(PathUtil.IsInDirectory(file, checkDir));
        }

        [Fact]
        public void IsInDirectory_EmptyInputs_ReturnsFalse()
        {
            Assert.False(PathUtil.IsInDirectory("", "dir"));
            Assert.False(PathUtil.IsInDirectory("file", ""));
        }

        #endregion

        #region GetUniqueFileName

        [Fact]
        public void GetUniqueFileName_NoConflict_ReturnsSameName()
        {
            var result = PathUtil.GetUniqueFileName(_testDir, "newfile.txt");
            Assert.Equal("newfile.txt", result);
        }

        [Fact]
        public void GetUniqueFileName_WithConflict_ReturnsNewName()
        {
            var file = Path.Combine(_testDir, "conflict.txt");
            File.WriteAllText(file, "content");

            var result = PathUtil.GetUniqueFileName(_testDir, "conflict.txt");
            Assert.Equal("conflict (1).txt", result);
        }

        [Fact]
        public void GetUniqueFileName_MultipleConflicts_ReturnsIncrementedName()
        {
            File.WriteAllText(Path.Combine(_testDir, "multi.txt"), "a");
            File.WriteAllText(Path.Combine(_testDir, "multi (1).txt"), "b");

            var result = PathUtil.GetUniqueFileName(_testDir, "multi.txt");
            Assert.Equal("multi (2).txt", result);
        }

        #endregion

        #region GetTempFilePath

        [Fact]
        public void GetTempFilePath_ReturnsValidPath()
        {
            var path = PathUtil.GetTempFilePath();
            Assert.True(File.Exists(path));
            File.Delete(path);
        }

        [Fact]
        public void GetTempFilePath_WithExtension_HasCorrectExtension()
        {
            var path = PathUtil.GetTempFilePath(".txt");
            Assert.True(File.Exists(path));
            Assert.Equal(".txt", Path.GetExtension(path));
            File.Delete(path);
        }

        #endregion

        #region GetTempDirectoryPath

        [Fact]
        public void GetTempDirectoryPath_ReturnsValidDirectory()
        {
            var path = PathUtil.GetTempDirectoryPath();
            Assert.True(Directory.Exists(path));
            Directory.Delete(path, true);
        }

        [Fact]
        public void GetTempDirectoryPath_CalledTwice_ReturnsDifferentPaths()
        {
            var path1 = PathUtil.GetTempDirectoryPath();
            var path2 = PathUtil.GetTempDirectoryPath();
            Assert.NotEqual(path1, path2);
            Directory.Delete(path1, true);
            Directory.Delete(path2, true);
        }

        #endregion

        #region Split

        [Fact]
        public void Split_ReturnsPathParts()
        {
            var path = Path.Combine("a", "b", "c");
            var parts = PathUtil.Split(path);
            Assert.Equal(3, parts.Length);
        }

        [Fact]
        public void Split_EmptyPath_ReturnsEmptyArray()
        {
            Assert.Empty(PathUtil.Split(""));
        }

        [Fact]
        public void Split_AbsolutePath_IncludesRoot()
        {
            var tempRoot = Path.GetPathRoot(Path.GetTempPath());
            if (tempRoot != null)
            {
                var path = Path.Combine(tempRoot, "a", "b");
                var parts = PathUtil.Split(path);
                Assert.True(parts.Length >= 2);
                Assert.Equal(tempRoot.TrimEnd(Path.DirectorySeparatorChar), parts[0]);
            }
        }

        #endregion

        #region Build

        [Fact]
        public void Build_CombinesParts()
        {
            var result = PathUtil.Build("a", "b", "c");
            Assert.Contains("a", result);
            Assert.Contains("c", result);
        }

        [Fact]
        public void Build_SkipsEmptyParts()
        {
            var result = PathUtil.Build("a", "", "b", null, "c");
            Assert.Contains("a", result);
            Assert.Contains("c", result);
        }

        [Fact]
        public void Build_SinglePart_ReturnsPart()
        {
            var result = PathUtil.Build("folder");
            Assert.Equal("folder", result);
        }

        #endregion

        #region IsValid

        [Fact]
        public void IsValid_ValidPath_ReturnsTrue()
        {
            Assert.True(PathUtil.IsValid("folder/file.txt"));
        }

        [Fact]
        public void IsValid_InvalidChars_ReturnsFalse()
        {
            var invalidChars = Path.GetInvalidPathChars();
            Assert.False(PathUtil.IsValid($"folder{invalidChars[0]}file.txt"));
        }

        [Fact]
        public void IsValid_EmptyPath_ReturnsFalse()
        {
            Assert.False(PathUtil.IsValid(""));
        }

        #endregion

        #region IsValidFileName

        [Fact]
        public void IsValidFileName_ValidName_ReturnsTrue()
        {
            Assert.True(PathUtil.IsValidFileName("file.txt"));
        }

        [Fact]
        public void IsValidFileName_InvalidChars_ReturnsFalse()
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            Assert.False(PathUtil.IsValidFileName($"file{invalidChars[0]}.txt"));
        }

        [Fact]
        public void IsValidFileName_EmptyString_ReturnsFalse()
        {
            Assert.False(PathUtil.IsValidFileName(""));
        }

        #endregion

        #region SanitizeFileName

        [Fact]
        public void SanitizeFileName_RemovesInvalidChars()
        {
            var result = PathUtil.SanitizeFileName("file<name>.txt");
            Assert.False(result.Contains("<"));
            Assert.False(result.Contains(">"));
            Assert.Contains("file", result);
            Assert.Contains(".txt", result);
        }

        [Fact]
        public void SanitizeFileName_ValidName_ReturnsSame()
        {
            var result = PathUtil.SanitizeFileName("valid_file.txt");
            Assert.Equal("valid_file.txt", result);
        }

        [Fact]
        public void SanitizeFileName_CustomReplacement()
        {
            var result = PathUtil.SanitizeFileName("file<name>.txt", '-');
            // Both < and > get replaced by -, resulting in "file-name-.txt"
            Assert.Contains("file-name", result);
            Assert.Contains(".txt", result);
        }

        [Fact]
        public void SanitizeFileName_EmptyString_ReturnsEmpty()
        {
            Assert.Equal("", PathUtil.SanitizeFileName(""));
        }

        #endregion

        #region GetSize

        [Fact]
        public void GetSize_ExistingFile_ReturnsFileSize()
        {
            var file = Path.Combine(_testDir, "sizetest.txt");
            File.WriteAllText(file, "Hello World");

            var size = PathUtil.GetSize(file);
            Assert.Equal(11, size);
        }

        [Fact]
        public void GetSize_ExistingDirectory_ReturnsTotalSize()
        {
            var dir = Path.Combine(_testDir, "sizedir");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "a.txt"), "123");
            File.WriteAllText(Path.Combine(dir, "b.txt"), "4567");

            var size = PathUtil.GetSize(dir);
            Assert.Equal(7, size);
        }

        [Fact]
        public void GetSize_NonExistentPath_ReturnsZero()
        {
            Assert.Equal(0, PathUtil.GetSize(Path.Combine(_testDir, "nonexistent")));
        }

        #endregion
    }
}
