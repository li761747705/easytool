using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EasyTool.MediaCategory
{
    /// <summary>
    /// 视频工具类
    /// 提供视频转换、剪辑、处理等功能
    /// 需要安装 FFmpeg
    /// </summary>
    public static class VideoUtil
    {
        /// <summary>
        /// FFmpeg 可执行文件路径
        /// </summary>
        public static string? FFmpegPath { get; set; }

        /// <summary>
        /// 转换视频格式
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="videoCodec">视频编码器（libx264, libx265, vp9 等）</param>
        /// <param name="audioCodec">音频编码器（aac, mp3, opus 等）</param>
        /// <param name="crf">视频质量（0-51，越小质量越高，默认23）</param>
        /// <returns>是否成功</returns>
        public static bool Convert(string inputPath, string outputPath, string? videoCodec = null, string? audioCodec = null, int? crf = null)
        {
            var args = $"-i \"{inputPath}\"";

            if (!string.IsNullOrEmpty(videoCodec))
                args += $" -c:v {videoCodec}";
            else
                args += " -c:v libx264";

            if (!string.IsNullOrEmpty(audioCodec))
                args += $" -c:a {audioCodec}";
            else
                args += " -c:a aac";

            if (crf.HasValue)
                args += $" -crf {crf.Value}";
            else
                args += " -crf 23";

            args += $" \"{outputPath}\" -y";

            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 异步转换视频格式
        /// </summary>
        public static async Task<bool> ConvertAsync(string inputPath, string outputPath, string? videoCodec = null, string? audioCodec = null, int? crf = null)
        {
            return await Task.Run(() => Convert(inputPath, outputPath, videoCodec, audioCodec, crf));
        }

        /// <summary>
        /// 压缩视频
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="quality">质量（1-100，越小压缩率越高）</param>
        /// <returns>是否成功</returns>
        public static bool Compress(string inputPath, string outputPath, int quality = 50)
        {
            var crf = 51 - (quality * 51 / 100);
            var args = $"-i \"{inputPath}\" -c:v libx264 -crf {crf} -c:a aac -b:a 128k \"{outputPath}\" -y";
            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 裁剪视频
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="duration">持续时间</param>
        /// <returns>是否成功</returns>
        public static bool Trim(string inputPath, string outputPath, TimeSpan startTime, TimeSpan duration)
        {
            var args = $"-i \"{inputPath}\" -ss {startTime:hh\\:mm\\:ss\\.fff} -t {duration:hh\\:mm\\:ss\\.fff} -c copy \"{outputPath}\" -y";
            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 合并视频文件
        /// </summary>
        /// <param name="inputPaths">输入文件路径列表</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <returns>是否成功</returns>
        public static bool Merge(string[] inputPaths, string outputPath)
        {
            var tempListPath = Path.Combine(Path.GetTempPath(), $"ffmpeg_list_{Guid.NewGuid():N}.txt");
            using (var writer = new StreamWriter(tempListPath))
            {
                foreach (var path in inputPaths)
                {
                    writer.WriteLine($"file '{path}'");
                }
            }

            try
            {
                var args = $"-f concat -safe 0 -i \"{tempListPath}\" -c copy \"{outputPath}\" -y";
                return ExecuteFFmpeg(args);
            }
            finally
            {
                File.Delete(tempListPath);
            }
        }

        /// <summary>
        /// 提取视频帧为图片
        /// </summary>
        /// <param name="videoPath">视频文件路径</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <param name="fps">每秒帧数（默认1，即每秒1帧）</param>
        /// <param name="imageFormat">图片格式（jpg, png）</param>
        /// <returns>是否成功</returns>
        public static bool ExtractFrames(string videoPath, string outputDirectory, int fps = 1, string imageFormat = "jpg")
        {
            Directory.CreateDirectory(outputDirectory);
            var args = $"-i \"{videoPath}\" -vf fps={fps} \"{outputDirectory}/frame_%04d.{imageFormat}\" -y";
            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 从图片创建视频
        /// </summary>
        /// <param name="imageDirectory">图片目录</param>
        /// <param name="outputPath">输出视频路径</param>
        /// <param name="fps">帧率</param>
        /// <param name="imagePattern">图片文件模式（如 "frame_%04d.jpg"）</param>
        /// <returns>是否成功</returns>
        public static bool CreateFromImages(string imageDirectory, string outputPath, int fps = 30, string imagePattern = "frame_%04d.jpg")
        {
            var args = $"-framerate {fps} -i \"{imageDirectory}/{imagePattern}\" -c:v libx264 -pix_fmt yuv420p \"{outputPath}\" -y";
            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 添加水印
        /// </summary>
        /// <param name="videoPath">视频文件路径</param>
        /// <param name="watermarkPath">水印图片路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="position">水印位置</param>
        /// <param name="opacity">透明度（0-1）</param>
        /// <returns>是否成功</returns>
        public static bool AddWatermark(string videoPath, string watermarkPath, string outputPath, WatermarkPosition position = WatermarkPosition.BottomRight, double opacity = 1.0)
        {
            var overlay = position switch
            {
                WatermarkPosition.TopLeft => "0:0",
                WatermarkPosition.TopRight => "main_w-overlay_w-10:10",
                WatermarkPosition.BottomLeft => "10:main_h-overlay_h-10",
                WatermarkPosition.BottomRight => "main_w-overlay_w-10:main_h-overlay_h-10",
                WatermarkPosition.Center => "(main_w-overlay_w)/2:(main_h-overlay_h)/2",
                _ => "main_w-overlay_w-10:main_h-overlay_h-10"
            };

            var args = $"-i \"{videoPath}\" -i \"{watermarkPath}\" -filter_complex \"[1:v]format=rgba,colorchannelmixer=aa={opacity}[logo];[0:v][logo]overlay={overlay}\" -c:a copy \"{outputPath}\" -y";
            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 调整视频分辨率
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="width">目标宽度</param>
        /// <param name="height">目标高度</param>
        /// <returns>是否成功</returns>
        public static bool Resize(string inputPath, string outputPath, int width, int height)
        {
            var args = $"-i \"{inputPath}\" -vf scale={width}:{height} -c:a copy \"{outputPath}\" -y";
            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 获取视频信息
        /// </summary>
        /// <param name="filePath">视频文件路径</param>
        /// <returns>视频信息</returns>
        public static VideoInfo? GetInfo(string filePath)
        {
            var args = $"-i \"{filePath}\" -hide_banner -show_format -show_streams -of json";
            var result = ExecuteFFmpegProbe(args);

            if (string.IsNullOrEmpty(result))
                return null;

            try
            {
                var json = System.Text.Json.JsonDocument.Parse(result);
                var format = json.RootElement.GetProperty("format");

                var info = new VideoInfo
                {
                    Duration = TimeSpan.FromSeconds(double.Parse(format.GetProperty("duration").GetString() ?? "0")),
                    BitRate = long.Parse(format.GetProperty("bit_rate").GetString() ?? "0"),
                    Format = format.GetProperty("format_name").GetString() ?? "",
                    Size = long.Parse(format.GetProperty("size").GetString() ?? "0")
                };

                // 获取视频流信息
                var streams = json.RootElement.GetProperty("streams");
                foreach (var stream in streams.EnumerateArray())
                {
                    if (stream.GetProperty("codec_type").GetString() == "video")
                    {
                        info.Width = stream.GetProperty("width").GetInt32();
                        info.Height = stream.GetProperty("height").GetInt32();
                        info.VideoCodec = stream.GetProperty("codec_name").GetString() ?? "";
                        if (stream.TryGetProperty("r_frame_rate", out var frameRate))
                        {
                            var fpsStr = frameRate.GetString() ?? "0/1";
                            var parts = fpsStr.Split('/');
                            if (parts.Length == 2 && int.TryParse(parts[1], out var denom) && denom > 0)
                            {
                                info.FrameRate = double.Parse(parts[0]) / denom;
                            }
                        }
                        break;
                    }
                }

                return info;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 生成 GIF
        /// </summary>
        /// <param name="videoPath">视频文件路径</param>
        /// <param name="outputPath">输出 GIF 路径</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="duration">持续时间</param>
        /// <param name="width">宽度（默认320）</param>
        /// <param name="fps">帧率（默认10）</param>
        /// <returns>是否成功</returns>
        public static bool CreateGif(string videoPath, string outputPath, TimeSpan startTime, TimeSpan duration, int width = 320, int fps = 10)
        {
            var args = $"-i \"{videoPath}\" -ss {startTime:hh\\:mm\\:ss} -t {duration:hh\\:mm\\:ss} -vf \"fps={fps},scale={width}:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse\" \"{outputPath}\" -y";
            return ExecuteFFmpeg(args);
        }

        private static bool ExecuteFFmpeg(string arguments)
        {
            var ffmpeg = FFmpegPath ?? "ffmpeg";
            var psi = new ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            process.WaitForExit();
            return process.ExitCode == 0;
        }

        private static string? ExecuteFFmpegProbe(string arguments)
        {
            var ffprobe = FFmpegPath ?? "ffprobe";
            var probePath = ffprobe.Replace("ffmpeg", "ffprobe");

            var psi = new ProcessStartInfo
            {
                FileName = probePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return null;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
    }

    /// <summary>
    /// 视频信息
    /// </summary>
    public class VideoInfo
    {
        /// <summary>
        /// 时长
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 比特率
        /// </summary>
        public long BitRate { get; set; }

        /// <summary>
        /// 格式
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 视频宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 视频高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 视频编码
        /// </summary>
        public string VideoCodec { get; set; } = string.Empty;

        /// <summary>
        /// 帧率
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// 分辨率字符串
        /// </summary>
        public string Resolution => $"{Width}x{Height}";
    }

    /// <summary>
    /// 水印位置
    /// </summary>
    public enum WatermarkPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }
}
