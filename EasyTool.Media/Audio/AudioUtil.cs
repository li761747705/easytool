using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EasyTool.Media.Audio
{
    /// <summary>
    /// 音频工具类
    /// 提供音频转换、提取、处理等功能
    /// 需要安装 FFmpeg
    /// </summary>
    public static class AudioUtil
    {
        /// <summary>
        /// FFmpeg 可执行文件路径
        /// </summary>
        public static string? FFmpegPath { get; set; }

        /// <summary>
        /// 转换音频格式
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="format">输出格式（mp3, wav, aac, flac 等）</param>
        /// <param name="bitrate">比特率（如 "128k", "256k"）</param>
        /// <param name="sampleRate">采样率（如 44100, 48000）</param>
        /// <returns>是否成功</returns>
        public static bool Convert(string inputPath, string outputPath, string format, string? bitrate = null, int? sampleRate = null)
        {
            var args = $"-i \"{inputPath}\"";

            if (!string.IsNullOrEmpty(bitrate))
                args += $" -b:a {bitrate}";

            if (sampleRate.HasValue)
                args += $" -ar {sampleRate.Value}";

            args += $" -f {format} \"{outputPath}\" -y";

            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 异步转换音频格式
        /// </summary>
        public static async Task<bool> ConvertAsync(string inputPath, string outputPath, string format, string? bitrate = null, int? sampleRate = null)
        {
            return await Task.Run(() => Convert(inputPath, outputPath, format, bitrate, sampleRate));
        }

        /// <summary>
        /// 从视频中提取音频
        /// </summary>
        /// <param name="videoPath">视频文件路径</param>
        /// <param name="outputPath">输出音频路径</param>
        /// <param name="format">输出格式</param>
        /// <param name="bitrate">比特率</param>
        /// <returns>是否成功</returns>
        public static bool ExtractFromVideo(string videoPath, string outputPath, string format = "mp3", string? bitrate = "192k")
        {
            var args = $"-i \"{videoPath}\" -vn";

            if (!string.IsNullOrEmpty(bitrate))
                args += $" -b:a {bitrate}";

            args += $" -f {format} \"{outputPath}\" -y";

            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 裁剪音频
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
        /// 合并音频文件
        /// </summary>
        /// <param name="inputPaths">输入文件路径列表</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <returns>是否成功</returns>
        public static bool Merge(string[] inputPaths, string outputPath)
        {
            // 创建临时文件列表
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
        /// 调整音量
        /// </summary>
        /// <param name="inputPath">输入文件路径</param>
        /// <param name="outputPath">输出文件路径</param>
        /// <param name="volumeFactor">音量因子（1.0 = 原音量，2.0 = 两倍，0.5 = 一半）</param>
        /// <returns>是否成功</returns>
        public static bool AdjustVolume(string inputPath, string outputPath, double volumeFactor)
        {
            var args = $"-i \"{inputPath}\" -af \"volume={volumeFactor}\" \"{outputPath}\" -y";
            return ExecuteFFmpeg(args);
        }

        /// <summary>
        /// 获取音频信息
        /// </summary>
        /// <param name="filePath">音频文件路径</param>
        /// <returns>音频信息</returns>
        public static AudioInfo? GetInfo(string filePath)
        {
            var args = $"-i \"{filePath}\" -hide_banner -show_format -show_streams -of json";
            var result = ExecuteFFmpegProbe(args);

            if (string.IsNullOrEmpty(result))
                return null;

            try
            {
                var json = System.Text.Json.JsonDocument.Parse(result);
                var format = json.RootElement.GetProperty("format");

                return new AudioInfo
                {
                    Duration = TimeSpan.FromSeconds(double.Parse(format.GetProperty("duration").GetString() ?? "0")),
                    BitRate = long.Parse(format.GetProperty("bit_rate").GetString() ?? "0"),
                    Format = format.GetProperty("format_name").GetString() ?? "",
                    Size = long.Parse(format.GetProperty("size").GetString() ?? "0")
                };
            }
            catch
            {
                return null;
            }
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
    /// 音频信息
    /// </summary>
    public class AudioInfo
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
    }
}
