using Xunit;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace EasyTool.IOCategory.Tests
{
    public class CompressionUtilTests : IDisposable
    {
        private readonly string _testDir;

        public CompressionUtilTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "EasyTool_CompressionTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        #region GZip

        [Fact]
        public void GZipCompress_CompressesData()
        {
            var data = Encoding.UTF8.GetBytes("Hello World, this is a test string for compression.");
            var compressed = CompressionUtil.GZipCompress(data);

            Assert.True(compressed.Length > 0);
            Assert.NotEqual(data, compressed);
        }

        [Fact]
        public void GZipDecompress_DecompressesData()
        {
            var original = Encoding.UTF8.GetBytes("Compression round-trip test data.");
            var compressed = CompressionUtil.GZipCompress(original);
            var decompressed = CompressionUtil.GZipDecompress(compressed);

            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void GZipCompress_EmptyData_ReturnsCompressedBytes()
        {
            var data = Array.Empty<byte>();
            var compressed = CompressionUtil.GZipCompress(data);

            Assert.True(compressed.Length > 0);
        }

        [Fact]
        public void GZipDecompress_EmptyCompressedData_ReturnsEmptyArray()
        {
            var data = Array.Empty<byte>();
            var compressed = CompressionUtil.GZipCompress(data);
            var decompressed = CompressionUtil.GZipDecompress(compressed);

            Assert.Empty(decompressed);
        }

        [Fact]
        public void GZipCompressString_RoundTrip()
        {
            var original = "Hello, GZip string compression test!";
            var compressed = CompressionUtil.GZipCompressString(original);
            var decompressed = CompressionUtil.GZipDecompressString(compressed);

            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void GZipCompressString_WithCustomEncoding_RoundTrip()
        {
            var original = "Unicode test: \u4e2d\u6587\u6d4b\u8bd5";
            var encoding = Encoding.Unicode;
            var compressed = CompressionUtil.GZipCompressString(original, encoding);
            var decompressed = CompressionUtil.GZipDecompressString(compressed, encoding);

            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void GZipCompressString_ReturnsBase64()
        {
            var compressed = CompressionUtil.GZipCompressString("test");

            // Should not throw - valid base64
            var bytes = Convert.FromBase64String(compressed);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void GZip_LargeData_RoundTrip()
        {
            var data = Encoding.UTF8.GetBytes(new string('A', 100000));
            var compressed = CompressionUtil.GZipCompress(data);
            var decompressed = CompressionUtil.GZipDecompress(compressed);

            Assert.Equal(data, decompressed);
            Assert.True(compressed.Length < data.Length);
        }

        #endregion

        #region Deflate

        [Fact]
        public void DeflateCompress_CompressesData()
        {
            var data = Encoding.UTF8.GetBytes("Deflate compression test string.");
            var compressed = CompressionUtil.DeflateCompress(data);

            Assert.True(compressed.Length > 0);
            Assert.NotEqual(data, compressed);
        }

        [Fact]
        public void DeflateDecompress_DecompressesData()
        {
            var original = Encoding.UTF8.GetBytes("Deflate round-trip test data.");
            var compressed = CompressionUtil.DeflateCompress(original);
            var decompressed = CompressionUtil.DeflateDecompress(compressed);

            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void Deflate_EmptyData_RoundTrip()
        {
            var data = Array.Empty<byte>();
            var compressed = CompressionUtil.DeflateCompress(data);
            var decompressed = CompressionUtil.DeflateDecompress(compressed);

            Assert.Empty(decompressed);
        }

        [Fact]
        public void Deflate_LargeData_RoundTrip()
        {
            var data = Encoding.UTF8.GetBytes(new string('B', 100000));
            var compressed = CompressionUtil.DeflateCompress(data);
            var decompressed = CompressionUtil.DeflateDecompress(compressed);

            Assert.Equal(data, decompressed);
        }

        #endregion

        #region Brotli

        [Fact]
        public void BrotliCompress_CompressesData()
        {
            var data = Encoding.UTF8.GetBytes("Brotli compression test string.");
            var compressed = CompressionUtil.BrotliCompress(data);

            Assert.True(compressed.Length > 0);
            Assert.NotEqual(data, compressed);
        }

        [Fact]
        public void BrotliDecompress_DecompressesData()
        {
            var original = Encoding.UTF8.GetBytes("Brotli round-trip test data.");
            var compressed = CompressionUtil.BrotliCompress(original);
            var decompressed = CompressionUtil.BrotliDecompress(compressed);

            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void Brotli_EmptyData_RoundTrip()
        {
            var data = Array.Empty<byte>();
            var compressed = CompressionUtil.BrotliCompress(data);
            var decompressed = CompressionUtil.BrotliDecompress(compressed);

            Assert.Empty(decompressed);
        }

        [Fact]
        public void Brotli_LargeData_RoundTrip()
        {
            var data = Encoding.UTF8.GetBytes(new string('C', 100000));
            var compressed = CompressionUtil.BrotliCompress(data);
            var decompressed = CompressionUtil.BrotliDecompress(compressed);

            Assert.Equal(data, decompressed);
        }

        #endregion

        #region Zip Directory

        [Fact]
        public void ZipDirectory_CreatesZipFile()
        {
            var sourceDir = Path.Combine(_testDir, "zipSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "a.txt"), "contentA");
            File.WriteAllText(Path.Combine(sourceDir, "b.txt"), "contentB");

            var zipPath = Path.Combine(_testDir, "output.zip");
            CompressionUtil.ZipDirectory(sourceDir, zipPath);

            Assert.True(File.Exists(zipPath));
            Assert.True(new FileInfo(zipPath).Length > 0);
        }

        [Fact]
        public void Unzip_ExtractsFiles()
        {
            var sourceDir = Path.Combine(_testDir, "unzipSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "unzip test content");

            var zipPath = Path.Combine(_testDir, "archive.zip");
            CompressionUtil.ZipDirectory(sourceDir, zipPath, includeBaseDirectory: false);

            var destDir = Path.Combine(_testDir, "extracted");
            CompressionUtil.Unzip(zipPath, destDir);

            Assert.True(Directory.Exists(destDir));
            Assert.True(File.Exists(Path.Combine(destDir, "test.txt")));
            Assert.Equal("unzip test content", File.ReadAllText(Path.Combine(destDir, "test.txt")));
        }

        [Fact]
        public void Unzip_WithOverwrite_OverwritesExistingFiles()
        {
            var sourceDir = Path.Combine(_testDir, "overwriteSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "ow.txt"), "new content");

            var zipPath = Path.Combine(_testDir, "ow.zip");
            // Use includeBaseDirectory: false so ow.txt is at root level in zip
            CompressionUtil.ZipDirectory(sourceDir, zipPath, includeBaseDirectory: false);

            var destDir = Path.Combine(_testDir, "overwriteDest");
            Directory.CreateDirectory(destDir);
            File.WriteAllText(Path.Combine(destDir, "ow.txt"), "old content");

            CompressionUtil.Unzip(zipPath, destDir, true);

            Assert.Equal("new content", File.ReadAllText(Path.Combine(destDir, "ow.txt")));
        }

        [Fact]
        public void ZipDirectory_WithoutBaseDirectory_ExcludesBaseDir()
        {
            var sourceDir = Path.Combine(_testDir, "noBaseSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "inner.txt"), "data");

            var zipPath = Path.Combine(_testDir, "noBase.zip");
            CompressionUtil.ZipDirectory(sourceDir, zipPath, includeBaseDirectory: false);

            var entries = CompressionUtil.GetZipEntries(zipPath);
            Assert.Contains("inner.txt", entries);
            // When includeBaseDirectory is false, the base directory name should not be in entries
            Assert.DoesNotContain("noBaseSource", entries.FirstOrDefault() ?? "");
        }

        #endregion

        #region Zip Files

        [Fact]
        public void ZipFiles_CreatesZipWithSpecifiedFiles()
        {
            var file1 = Path.Combine(_testDir, "zf1.txt");
            var file2 = Path.Combine(_testDir, "zf2.txt");
            File.WriteAllText(file1, "content1");
            File.WriteAllText(file2, "content2");

            var zipPath = Path.Combine(_testDir, "files.zip");
            CompressionUtil.ZipFiles(new[] { file1, file2 }, zipPath);

            Assert.True(File.Exists(zipPath));
            var entries = CompressionUtil.GetZipEntries(zipPath);
            Assert.Equal(2, entries.Count);
        }

        [Fact]
        public void ZipFiles_WithBasePath_PreservesRelativeStructure()
        {
            var subDir = Path.Combine(_testDir, "zipSub");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "a.txt"), "data");

            var zipPath = Path.Combine(_testDir, "withBase.zip");
            CompressionUtil.ZipFiles(new[] { Path.Combine(subDir, "a.txt") }, zipPath, subDir);

            var entries = CompressionUtil.GetZipEntries(zipPath);
            Assert.Contains("a.txt", entries);
        }

        #endregion

        #region Zip Entries / Extract Single File

        [Fact]
        public void GetZipEntries_ReturnsAllEntries()
        {
            var sourceDir = Path.Combine(_testDir, "entriesSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "e1.txt"), "a");
            File.WriteAllText(Path.Combine(sourceDir, "e2.txt"), "b");

            var zipPath = Path.Combine(_testDir, "entries.zip");
            CompressionUtil.ZipDirectory(sourceDir, zipPath, includeBaseDirectory: false);

            var entries = CompressionUtil.GetZipEntries(zipPath);
            Assert.Contains("e1.txt", entries);
            Assert.Contains("e2.txt", entries);
        }

        [Fact]
        public void ExtractFile_ExtractsSingleFile()
        {
            var sourceDir = Path.Combine(_testDir, "extractSingleSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "target.txt"), "extract me");
            File.WriteAllText(Path.Combine(sourceDir, "other.txt"), "not me");

            var zipPath = Path.Combine(_testDir, "extractSingle.zip");
            CompressionUtil.ZipDirectory(sourceDir, zipPath, includeBaseDirectory: false);

            var destFile = Path.Combine(_testDir, "extracted_single.txt");
            CompressionUtil.ExtractFile(zipPath, "target.txt", destFile);

            Assert.True(File.Exists(destFile));
            Assert.Equal("extract me", File.ReadAllText(destFile));
        }

        [Fact]
        public void ExtractFile_EntryNotFound_ThrowsException()
        {
            var sourceDir = Path.Combine(_testDir, "notFoundSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "a.txt"), "a");

            var zipPath = Path.Combine(_testDir, "notFound.zip");
            CompressionUtil.ZipDirectory(sourceDir, zipPath, includeBaseDirectory: false);

            Assert.Throws<FileNotFoundException>(() =>
                CompressionUtil.ExtractFile(zipPath, "nonexistent.txt", Path.Combine(_testDir, "out.txt")));
        }

        #endregion

        #region Add / Remove from Zip

        [Fact]
        public void AddFileToZip_AddsFile()
        {
            var sourceDir = Path.Combine(_testDir, "addSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "initial.txt"), "initial");

            var zipPath = Path.Combine(_testDir, "add.zip");
            CompressionUtil.ZipDirectory(sourceDir, zipPath, includeBaseDirectory: false);

            var newFile = Path.Combine(_testDir, "added.txt");
            File.WriteAllText(newFile, "added content");
            CompressionUtil.AddFileToZip(zipPath, newFile);

            var entries = CompressionUtil.GetZipEntries(zipPath);
            Assert.Contains("added.txt", entries);
        }

        [Fact]
        public void AddFileToZip_WithCustomEntryName_UsesCustomName()
        {
            var zipPath = Path.Combine(_testDir, "customEntry.zip");
            // Create a minimal valid zip first
            var sourceDir = Path.Combine(_testDir, "customSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "orig.txt"), "data");
            CompressionUtil.ZipDirectory(sourceDir, zipPath, includeBaseDirectory: false);

            var addFile = Path.Combine(_testDir, "custom.txt");
            File.WriteAllText(addFile, "custom");
            CompressionUtil.AddFileToZip(zipPath, addFile, "renamed.txt");

            var entries = CompressionUtil.GetZipEntries(zipPath);
            Assert.Contains("renamed.txt", entries);
        }

        [Fact]
        public void RemoveFileFromZip_RemovesFile()
        {
            var sourceDir = Path.Combine(_testDir, "removeSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "keep.txt"), "keep");
            File.WriteAllText(Path.Combine(sourceDir, "remove.txt"), "remove");

            var zipPath = Path.Combine(_testDir, "remove.zip");
            CompressionUtil.ZipDirectory(sourceDir, zipPath, includeBaseDirectory: false);

            CompressionUtil.RemoveFileFromZip(zipPath, "remove.txt");

            var entries = CompressionUtil.GetZipEntries(zipPath);
            Assert.DoesNotContain("remove.txt", entries);
            Assert.Contains("keep.txt", entries);
        }

        [Fact]
        public void RemoveFileFromZip_EntryNotFound_ThrowsException()
        {
            var zipPath = Path.Combine(_testDir, "removeNotFound.zip");
            var sourceDir = Path.Combine(_testDir, "removeNotFoundSource");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "a.txt"), "a");
            CompressionUtil.ZipDirectory(sourceDir, zipPath, includeBaseDirectory: false);

            Assert.Throws<FileNotFoundException>(() =>
                CompressionUtil.RemoveFileFromZip(zipPath, "missing.txt"));
        }

        #endregion

        #region Compression Ratio

        [Fact]
        public void CalculateCompressionRatio_StandardCase_ReturnsPositiveRatio()
        {
            var ratio = CompressionUtil.CalculateCompressionRatio(1000, 500);
            Assert.Equal(50.0, ratio);
        }

        [Fact]
        public void CalculateCompressionRatio_NoCompression_ReturnsZero()
        {
            var ratio = CompressionUtil.CalculateCompressionRatio(1000, 1000);
            Assert.Equal(0.0, ratio);
        }

        [Fact]
        public void CalculateCompressionRatio_ZeroOriginal_ReturnsZero()
        {
            var ratio = CompressionUtil.CalculateCompressionRatio(0, 0);
            Assert.Equal(0.0, ratio);
        }

        [Fact]
        public void CalculateCompressionRatio_Expansion_ReturnsNegative()
        {
            var ratio = CompressionUtil.CalculateCompressionRatio(100, 150);
            Assert.True(ratio < 0);
        }

        #endregion

        #region Optimal Compression Level

        [Fact]
        public void GetOptimalCompressionLevel_HighTarget_ReturnsOptimal()
        {
            Assert.Equal(CompressionLevel.Optimal, CompressionUtil.GetOptimalCompressionLevel(90));
            Assert.Equal(CompressionLevel.Optimal, CompressionUtil.GetOptimalCompressionLevel(60));
        }

        [Fact]
        public void GetOptimalCompressionLevel_MediumTarget_ReturnsFastest()
        {
            Assert.Equal(CompressionLevel.Fastest, CompressionUtil.GetOptimalCompressionLevel(30));
        }

        [Fact]
        public void GetOptimalCompressionLevel_LowTarget_ReturnsNoCompression()
        {
            Assert.Equal(CompressionLevel.NoCompression, CompressionUtil.GetOptimalCompressionLevel(10));
            Assert.Equal(CompressionLevel.NoCompression, CompressionUtil.GetOptimalCompressionLevel(0));
        }

        #endregion

        #region Cross-algorithm comparison

        [Fact]
        public void AllAlgorithms_ProduceCorrectDecompression()
        {
            var data = Encoding.UTF8.GetBytes("Cross-algorithm test string for verification.");

            var gzipResult = CompressionUtil.GZipDecompress(CompressionUtil.GZipCompress(data));
            var deflateResult = CompressionUtil.DeflateDecompress(CompressionUtil.DeflateCompress(data));
            var brotliResult = CompressionUtil.BrotliDecompress(CompressionUtil.BrotliCompress(data));

            Assert.Equal(data, gzipResult);
            Assert.Equal(data, deflateResult);
            Assert.Equal(data, brotliResult);
        }

        #endregion
    }
}
