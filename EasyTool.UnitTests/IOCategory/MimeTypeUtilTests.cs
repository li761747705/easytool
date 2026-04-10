using Xunit;
using System;
using System.IO;

namespace EasyTool.IOCategory.Tests
{
    public class MimeTypeUtilTests : IDisposable
    {
        private readonly string _testDir;

        public MimeTypeUtilTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "EasyTool_MimeTypeTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        #region GetByExtension

        [Fact]
        public void GetByExtension_KnownTextTypes_ReturnsCorrectMime()
        {
            Assert.Equal("text/plain", MimeTypeUtil.GetByExtension(".txt"));
            Assert.Equal("text/html", MimeTypeUtil.GetByExtension(".html"));
            Assert.Equal("text/html", MimeTypeUtil.GetByExtension(".htm"));
            Assert.Equal("text/css", MimeTypeUtil.GetByExtension(".css"));
            Assert.Equal("application/javascript", MimeTypeUtil.GetByExtension(".js"));
            Assert.Equal("application/json", MimeTypeUtil.GetByExtension(".json"));
            Assert.Equal("application/xml", MimeTypeUtil.GetByExtension(".xml"));
            Assert.Equal("text/csv", MimeTypeUtil.GetByExtension(".csv"));
            Assert.Equal("text/markdown", MimeTypeUtil.GetByExtension(".md"));
            Assert.Equal("text/yaml", MimeTypeUtil.GetByExtension(".yaml"));
            Assert.Equal("text/yaml", MimeTypeUtil.GetByExtension(".yml"));
        }

        [Fact]
        public void GetByExtension_KnownImageTypes_ReturnsCorrectMime()
        {
            Assert.Equal("image/jpeg", MimeTypeUtil.GetByExtension(".jpg"));
            Assert.Equal("image/jpeg", MimeTypeUtil.GetByExtension(".jpeg"));
            Assert.Equal("image/png", MimeTypeUtil.GetByExtension(".png"));
            Assert.Equal("image/gif", MimeTypeUtil.GetByExtension(".gif"));
            Assert.Equal("image/bmp", MimeTypeUtil.GetByExtension(".bmp"));
            Assert.Equal("image/x-icon", MimeTypeUtil.GetByExtension(".ico"));
            Assert.Equal("image/svg+xml", MimeTypeUtil.GetByExtension(".svg"));
            Assert.Equal("image/webp", MimeTypeUtil.GetByExtension(".webp"));
        }

        [Fact]
        public void GetByExtension_KnownAudioTypes_ReturnsCorrectMime()
        {
            Assert.Equal("audio/mpeg", MimeTypeUtil.GetByExtension(".mp3"));
            Assert.Equal("audio/wav", MimeTypeUtil.GetByExtension(".wav"));
            Assert.Equal("audio/ogg", MimeTypeUtil.GetByExtension(".ogg"));
            Assert.Equal("audio/flac", MimeTypeUtil.GetByExtension(".flac"));
            Assert.Equal("audio/aac", MimeTypeUtil.GetByExtension(".aac"));
        }

        [Fact]
        public void GetByExtension_KnownVideoTypes_ReturnsCorrectMime()
        {
            Assert.Equal("video/mp4", MimeTypeUtil.GetByExtension(".mp4"));
            Assert.Equal("video/x-msvideo", MimeTypeUtil.GetByExtension(".avi"));
            Assert.Equal("video/x-matroska", MimeTypeUtil.GetByExtension(".mkv"));
            Assert.Equal("video/quicktime", MimeTypeUtil.GetByExtension(".mov"));
            Assert.Equal("video/webm", MimeTypeUtil.GetByExtension(".webm"));
        }

        [Fact]
        public void GetByExtension_KnownDocumentTypes_ReturnsCorrectMime()
        {
            Assert.Equal("application/pdf", MimeTypeUtil.GetByExtension(".pdf"));
            Assert.Equal("application/msword", MimeTypeUtil.GetByExtension(".doc"));
            Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", MimeTypeUtil.GetByExtension(".docx"));
            Assert.Equal("application/vnd.ms-excel", MimeTypeUtil.GetByExtension(".xls"));
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", MimeTypeUtil.GetByExtension(".xlsx"));
        }

        [Fact]
        public void GetByExtension_KnownCompressionTypes_ReturnsCorrectMime()
        {
            Assert.Equal("application/zip", MimeTypeUtil.GetByExtension(".zip"));
            Assert.Equal("application/x-rar-compressed", MimeTypeUtil.GetByExtension(".rar"));
            Assert.Equal("application/x-7z-compressed", MimeTypeUtil.GetByExtension(".7z"));
            Assert.Equal("application/gzip", MimeTypeUtil.GetByExtension(".gz"));
        }

        [Fact]
        public void GetByExtension_UnknownExtension_ReturnsOctetStream()
        {
            Assert.Equal("application/octet-stream", MimeTypeUtil.GetByExtension(".unknownext"));
            Assert.Equal("application/octet-stream", MimeTypeUtil.GetByExtension(".xyz123"));
        }

        [Fact]
        public void GetByExtension_NullOrEmpty_ReturnsOctetStream()
        {
            Assert.Equal("application/octet-stream", MimeTypeUtil.GetByExtension(null!));
            Assert.Equal("application/octet-stream", MimeTypeUtil.GetByExtension(""));
        }

        [Fact]
        public void GetByExtension_WithoutDot_AddsDot()
        {
            Assert.Equal("text/plain", MimeTypeUtil.GetByExtension("txt"));
            Assert.Equal("application/json", MimeTypeUtil.GetByExtension("json"));
            Assert.Equal("image/png", MimeTypeUtil.GetByExtension("png"));
        }

        [Fact]
        public void GetByExtension_CaseInsensitive_ReturnsCorrectMime()
        {
            Assert.Equal("text/plain", MimeTypeUtil.GetByExtension(".TXT"));
            Assert.Equal("image/png", MimeTypeUtil.GetByExtension(".Png"));
            Assert.Equal("application/json", MimeTypeUtil.GetByExtension(".JSON"));
        }

        #endregion

        #region GetByPath

        [Fact]
        public void GetByPath_WithExtension_ReturnsMime()
        {
            Assert.Equal("text/plain", MimeTypeUtil.GetByPath("/path/to/file.txt"));
            Assert.Equal("image/png", MimeTypeUtil.GetByPath("document.png"));
            Assert.Equal("application/json", MimeTypeUtil.GetByPath("data.json"));
        }

        [Fact]
        public void GetByPath_UnknownExtension_ReturnsOctetStream()
        {
            Assert.Equal("application/octet-stream", MimeTypeUtil.GetByPath("file.unknownext"));
        }

        #endregion

        #region GetExtension (by MIME type)

        [Fact]
        public void GetExtension_KnownMimeTypes_ReturnsExtension()
        {
            Assert.Equal(".txt", MimeTypeUtil.GetExtension("text/plain"));
            Assert.Equal(".html", MimeTypeUtil.GetExtension("text/html"));
            Assert.Equal(".json", MimeTypeUtil.GetExtension("application/json"));
            Assert.Equal(".png", MimeTypeUtil.GetExtension("image/png"));
            Assert.Equal(".pdf", MimeTypeUtil.GetExtension("application/pdf"));
        }

        [Fact]
        public void GetExtension_UnknownMimeType_ReturnsBin()
        {
            Assert.Equal(".bin", MimeTypeUtil.GetExtension("application/unknown-type"));
        }

        [Fact]
        public void GetExtension_NullOrEmpty_ReturnsBin()
        {
            Assert.Equal(".bin", MimeTypeUtil.GetExtension(null!));
            Assert.Equal(".bin", MimeTypeUtil.GetExtension(""));
        }

        [Fact]
        public void GetExtension_CaseInsensitive_ReturnsExtension()
        {
            Assert.Equal(".txt", MimeTypeUtil.GetExtension("TEXT/PLAIN"));
            Assert.Equal(".png", MimeTypeUtil.GetExtension("Image/PNG"));
        }

        #endregion

        #region IsImage / IsAudio / IsVideo / IsText

        [Fact]
        public void IsImage_ImageMime_ReturnsTrue()
        {
            Assert.True(MimeTypeUtil.IsImage("image/png"));
            Assert.True(MimeTypeUtil.IsImage("image/jpeg"));
            Assert.True(MimeTypeUtil.IsImage("image/gif"));
        }

        [Fact]
        public void IsImage_NonImageMime_ReturnsFalse()
        {
            Assert.False(MimeTypeUtil.IsImage("text/plain"));
            Assert.False(MimeTypeUtil.IsImage("application/json"));
        }

        [Fact]
        public void IsImage_NullMime_ReturnsFalse()
        {
            Assert.False(MimeTypeUtil.IsImage(null!));
        }

        [Fact]
        public void IsAudio_AudioMime_ReturnsTrue()
        {
            Assert.True(MimeTypeUtil.IsAudio("audio/mpeg"));
            Assert.True(MimeTypeUtil.IsAudio("audio/wav"));
        }

        [Fact]
        public void IsAudio_NonAudioMime_ReturnsFalse()
        {
            Assert.False(MimeTypeUtil.IsAudio("image/png"));
        }

        [Fact]
        public void IsVideo_VideoMime_ReturnsTrue()
        {
            Assert.True(MimeTypeUtil.IsVideo("video/mp4"));
            Assert.True(MimeTypeUtil.IsVideo("video/webm"));
        }

        [Fact]
        public void IsVideo_NonVideoMime_ReturnsFalse()
        {
            Assert.False(MimeTypeUtil.IsVideo("text/plain"));
        }

        [Fact]
        public void IsText_TextMime_ReturnsTrue()
        {
            Assert.True(MimeTypeUtil.IsText("text/plain"));
            Assert.True(MimeTypeUtil.IsText("text/html"));
            Assert.True(MimeTypeUtil.IsText("text/css"));
        }

        [Fact]
        public void IsText_SpecialTextMimes_ReturnsTrue()
        {
            Assert.True(MimeTypeUtil.IsText("application/json"));
            Assert.True(MimeTypeUtil.IsText("application/xml"));
            Assert.True(MimeTypeUtil.IsText("application/javascript"));
        }

        [Fact]
        public void IsText_NonTextMime_ReturnsFalse()
        {
            Assert.False(MimeTypeUtil.IsText("image/png"));
            Assert.False(MimeTypeUtil.IsText("video/mp4"));
        }

        [Fact]
        public void IsText_NullMime_ReturnsFalse()
        {
            Assert.False(MimeTypeUtil.IsText(null!));
        }

        #endregion

        #region DetectByContent

        [Fact]
        public void DetectByContent_TextFile_ReturnsTextPlain()
        {
            var file = Path.Combine(_testDir, "text.txt");
            File.WriteAllText(file, "Hello World, this is plain text content.");

            var mime = MimeTypeUtil.DetectByContent(file);
            Assert.Equal("text/plain", mime);
        }

        [Fact]
        public void DetectByContent_NonExistentFile_ReturnsOctetStream()
        {
            var mime = MimeTypeUtil.DetectByContent(Path.Combine(_testDir, "nonexistent.txt"));
            Assert.Equal("application/octet-stream", mime);
        }

        [Fact]
        public void DetectByContent_EmptyFile_ReturnsOctetStream()
        {
            var file = Path.Combine(_testDir, "empty.bin");
            File.WriteAllText(file, "");

            var mime = MimeTypeUtil.DetectByContent(file);
            Assert.Equal("application/octet-stream", mime);
        }

        [Fact]
        public void DetectByContent_PngFile_ReturnsPngMime()
        {
            var file = Path.Combine(_testDir, "test.png");
            File.WriteAllBytes(file, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D });

            var mime = MimeTypeUtil.DetectByContent(file);
            Assert.Equal("image/png", mime);
        }

        [Fact]
        public void DetectByContent_JpegFile_ReturnsJpegMime()
        {
            var file = Path.Combine(_testDir, "test.jpg");
            File.WriteAllBytes(file, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 });

            var mime = MimeTypeUtil.DetectByContent(file);
            Assert.Equal("image/jpeg", mime);
        }

        [Fact]
        public void DetectByContent_PdfFile_ReturnsPdfMime()
        {
            var file = Path.Combine(_testDir, "test.pdf");
            File.WriteAllBytes(file, new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D });

            var mime = MimeTypeUtil.DetectByContent(file);
            Assert.Equal("application/pdf", mime);
        }

        [Fact]
        public void DetectByContent_ZipFile_ReturnsZipMime()
        {
            var file = Path.Combine(_testDir, "test.zip");
            File.WriteAllBytes(file, new byte[] { 0x50, 0x4B, 0x03, 0x04 });

            var mime = MimeTypeUtil.DetectByContent(file);
            Assert.Equal("application/zip", mime);
        }

        [Fact]
        public void DetectByContent_GifFile_ReturnsGifMime()
        {
            var file = Path.Combine(_testDir, "test.gif");
            File.WriteAllBytes(file, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 });

            var mime = MimeTypeUtil.DetectByContent(file);
            Assert.Equal("image/gif", mime);
        }

        [Fact]
        public void DetectByContent_BmpFile_ReturnsBmpMime()
        {
            var file = Path.Combine(_testDir, "test.bmp");
            File.WriteAllBytes(file, new byte[] { 0x42, 0x4D, 0x00, 0x00 });

            var mime = MimeTypeUtil.DetectByContent(file);
            Assert.Equal("image/bmp", mime);
        }

        [Fact]
        public void DetectByContent_RarFile_ReturnsRarMime()
        {
            var file = Path.Combine(_testDir, "test.rar");
            File.WriteAllBytes(file, new byte[] { 0x52, 0x61, 0x72, 0x21 });

            var mime = MimeTypeUtil.DetectByContent(file);
            Assert.Equal("application/x-rar-compressed", mime);
        }

        [Fact]
        public void DetectByContent_StreamOverload_DetectsText()
        {
            using var stream = new MemoryStream(global::System.Text.Encoding.UTF8.GetBytes("Plain text content"));
            var mime = MimeTypeUtil.DetectByContent(stream);
            Assert.Equal("text/plain", mime);
        }

        #endregion

        #region Detect (combined)

        [Fact]
        public void Detect_TextFile_ReturnsTextPlain()
        {
            var file = Path.Combine(_testDir, "detect.txt");
            File.WriteAllText(file, "Detection test content.");

            var mime = MimeTypeUtil.Detect(file);
            Assert.Equal("text/plain", mime);
        }

        [Fact]
        public void Detect_NonExistentFile_FallsBackToExtension()
        {
            var mime = MimeTypeUtil.Detect(Path.Combine(_testDir, "fallback.json"));
            Assert.Equal("application/json", mime);
        }

        [Fact]
        public void Detect_PngContent_ReturnsPngMime()
        {
            var file = Path.Combine(_testDir, "detect.png");
            File.WriteAllBytes(file, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

            var mime = MimeTypeUtil.Detect(file);
            Assert.Equal("image/png", mime);
        }

        #endregion

        #region Register

        [Fact]
        public void Register_CustomExtension_CanBeRetrieved()
        {
            MimeTypeUtil.Register(".custom", "application/x-custom");

            Assert.Equal("application/x-custom", MimeTypeUtil.GetByExtension(".custom"));
        }

        [Fact]
        public void Register_WithoutDot_AddsDot()
        {
            MimeTypeUtil.Register("mytype", "application/x-mytype");

            Assert.Equal("application/x-mytype", MimeTypeUtil.GetByExtension(".mytype"));
        }

        [Fact]
        public void Register_OverwriteExisting_Overwrites()
        {
            MimeTypeUtil.Register(".txt", "application/x-overwritten");

            Assert.Equal("application/x-overwritten", MimeTypeUtil.GetByExtension(".txt"));

            // Restore original value for other tests
            MimeTypeUtil.Register(".txt", "text/plain");
        }

        #endregion
    }
}
