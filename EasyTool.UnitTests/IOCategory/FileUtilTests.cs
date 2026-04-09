using Xunit;
using System;
using System.IO;

namespace EasyTool.IOCategory.Tests
{
    public class FileUtilTests : IDisposable
    {
        private readonly string _testDir;

        public FileUtilTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "EasyToolTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [Fact]
        public void IsEmpty_EmptyDirectory_ReturnsTrue()
        {
            var emptyDir = Path.Combine(_testDir, "EmptyDir");
            Directory.CreateDirectory(emptyDir);
            Assert.True(FileUtil.IsEmpty(emptyDir));
        }

        [Fact]
        public void IsEmpty_DirectoryWithFiles_ReturnsFalse()
        {
            var dirWithFile = Path.Combine(_testDir, "DirWithFile");
            Directory.CreateDirectory(dirWithFile);
            File.WriteAllText(Path.Combine(dirWithFile, "test.txt"), "content");
            Assert.False(FileUtil.IsEmpty(dirWithFile));
        }

        [Fact]
        public void IsEmpty_EmptyFile_ReturnsTrue()
        {
            var emptyFile = Path.Combine(_testDir, "empty.txt");
            File.WriteAllText(emptyFile, "");
            Assert.True(FileUtil.IsEmpty(emptyFile));
        }

        [Fact]
        public void IsEmpty_FileWithContent_ReturnsFalse()
        {
            var fileWithContent = Path.Combine(_testDir, "content.txt");
            File.WriteAllText(fileWithContent, "Hello World");
            Assert.False(FileUtil.IsEmpty(fileWithContent));
        }

        [Fact]
        public void IsEmpty_NonExistentPath_ThrowsFileNotFoundException()
        {
            var nonExistent = Path.Combine(_testDir, "nonexistent");
            Assert.Throws<FileNotFoundException>(() => FileUtil.IsEmpty(nonExistent));
        }

        [Fact]
        public void LoopFiles_ReturnsAllFiles()
        {
            // Create test structure
            Directory.CreateDirectory(Path.Combine(_testDir, "sub1"));
            File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "content");
            File.WriteAllText(Path.Combine(_testDir, "sub1", "file2.txt"), "content");

            var files = FileUtil.LoopFiles(_testDir, "*");
            Assert.Equal(2, files.Count);
        }

        [Fact]
        public void LoopFiles_WithPattern_FiltersCorrectly()
        {
            Directory.CreateDirectory(Path.Combine(_testDir, "sub"));
            File.WriteAllText(Path.Combine(_testDir, "test.txt"), "content");
            File.WriteAllText(Path.Combine(_testDir, "test.log"), "content");
            File.WriteAllText(Path.Combine(_testDir, "sub", "another.txt"), "content");

            var files = FileUtil.LoopFiles(_testDir, "*.txt");
            Assert.Equal(2, files.Count);
            Assert.All(files, f => Assert.EndsWith(".txt", f));
        }

        [Fact]
        public void LoopFiles_WithMaxDepth_RespectsDepth()
        {
            Directory.CreateDirectory(Path.Combine(_testDir, "level1", "level2"));
            File.WriteAllText(Path.Combine(_testDir, "root.txt"), "content");
            File.WriteAllText(Path.Combine(_testDir, "level1", "l1.txt"), "content");
            File.WriteAllText(Path.Combine(_testDir, "level1", "level2", "l2.txt"), "content");

            // maxDepth=1 means only root level (depth 0), maxDepth=2 means root + level1
            var files = FileUtil.LoopFiles(_testDir, 2, "*");
            Assert.Equal(2, files.Count); // root.txt and l1.txt, not l2.txt
        }

        [Fact]
        public void Clean_EmptyDirectory_ReturnsTrue()
        {
            var emptyDir = Path.Combine(_testDir, "CleanEmpty");
            Directory.CreateDirectory(emptyDir);
            Assert.True(FileUtil.Clean(emptyDir));
        }

        [Fact]
        public void Clean_DirectoryWithFiles_RemovesAllFiles()
        {
            var dirToClean = Path.Combine(_testDir, "DirToClean");
            Directory.CreateDirectory(dirToClean);
            File.WriteAllText(Path.Combine(dirToClean, "file1.txt"), "content");
            File.WriteAllText(Path.Combine(dirToClean, "file2.txt"), "content");

            Assert.True(FileUtil.Clean(dirToClean));
            Assert.Empty(Directory.GetFiles(dirToClean));
        }

        [Fact]
        public void Touch_CreatesNewFile()
        {
            var newFile = Path.Combine(_testDir, "newfile.txt");
            var result = FileUtil.Touch(newFile);
            Assert.True(File.Exists(newFile));
            Assert.Equal(newFile, result.FullName);
        }

        [Fact]
        public void Touch_ExistingFile_ReturnsExisting()
        {
            var existingFile = Path.Combine(_testDir, "existing.txt");
            File.WriteAllText(existingFile, "content");
            var result = FileUtil.Touch(existingFile);
            Assert.True(File.Exists(existingFile));
            Assert.Equal("content", File.ReadAllText(existingFile));
        }

        [Fact]
        public void CreateTempFile_ReturnsValidPath()
        {
            var tempFile = FileUtil.CreateTempFile();
            Assert.True(File.Exists(tempFile));
            File.Delete(tempFile); // Cleanup
        }

        [Fact]
        public void Normalize_NormalizesPath()
        {
            var path = "/foo//bar/";
            var result = FileUtil.Normalize(path);
            Assert.Equal("/foo/bar/", result);
        }

        [Fact]
        public void Normalize_HandlesRelativePath()
        {
            var path = "foo/../bar";
            var result = FileUtil.Normalize(path);
            Assert.Equal("bar", result);
        }

        [Fact]
        public void GetFileName_ReturnsFileName()
        {
            var path = "/path/to/file.txt";
            var result = FileUtil.GetFileName(path);
            Assert.Equal("file.txt", result);
        }

        [Fact]
        public void GetFileSuffix_ReturnsExtension()
        {
            var path = "/path/to/file.txt";
            var result = FileUtil.GetFileSuffix(path);
            Assert.Equal("txt", result);
        }

        [Fact]
        public void GetFileSuffix_NoExtension_ReturnsEmpty()
        {
            var path = "/path/to/file";
            var result = FileUtil.GetFileSuffix(path);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void IsAbsolutePath_AbsolutePath_ReturnsTrue()
        {
            var path = "C:\\path\\to\\file.txt";
            var result = FileUtil.IsAbsolutePath(path);
            Assert.True(result);
        }

        [Fact]
        public void IsAbsolutePath_RelativePath_ReturnsFalse()
        {
            var path = "path/to/file.txt";
            var result = FileUtil.IsAbsolutePath(path);
            Assert.False(result);
        }

        [Fact]
        public void CleanInvalid_RemovesInvalidChars()
        {
            var fileName = "file<name>.txt";
            var result = FileUtil.CleanInvalid(fileName);
            Assert.DoesNotContain("<", result);
            Assert.DoesNotContain(">", result);
        }

        [Fact]
        public void ContainsInvalid_InvalidChars_ReturnsTrue()
        {
            var fileName = "file<name>.txt";
            Assert.True(FileUtil.ContainsInvalid(fileName));
        }

        [Fact]
        public void ContainsInvalid_ValidName_ReturnsFalse()
        {
            var fileName = "valid_filename.txt";
            Assert.False(FileUtil.ContainsInvalid(fileName));
        }

        [Fact]
        public void GetMimeType_ReturnsCorrectMimeType()
        {
            Assert.Equal("image/png", FileUtil.GetMimeType("test.png"));
            Assert.Equal("image/jpeg", FileUtil.GetMimeType("test.jpg"));
            Assert.Equal("text/plain", FileUtil.GetMimeType("test.txt"));
        }
    }
}