using System;
using System.IO;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// LZ4 压缩工具类
    /// LZ4 是一种极快的无损压缩算法
    /// 压缩速度可达 500MB/s，解压速度可达 1GB/s
    /// </summary>
    public static class LZ4Util
    {
        private const int MinMatch = 4;
        private const int MaxOffset = 65535;
        private const int MinLookahead = MinMatch + 1;

        #region 压缩

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="data">要压缩的数据</param>
        /// <returns>压缩后的数据</returns>
        public static byte[] Compress(byte[] data)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            return Compress(data, 0, data.Length);
        }

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="data">要压缩的数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>压缩后的数据</returns>
        public static byte[] Compress(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length == 0)
                return Array.Empty<byte>();

            if (length < MinLookahead)
            {
                // 太短，直接返回原始数据（带标记）
                byte[] result = new byte[length + 1];
                result[0] = 0; // 标记为未压缩
                Array.Copy(data, offset, result, 1, length);
                return result;
            }

            var output = new MemoryStream();
            var writer = new BinaryWriter(output);

            // 写入原始长度
            writer.Write(length);

            int pos = offset;
            int anchor = offset;
            int end = offset + length;

            while (pos < end - MinLookahead)
            {
                int matchPos = FindMatch(data, pos, end, out int matchLength);

                if (matchPos >= 0 && matchLength >= MinMatch)
                {
                    // 写入字面量
                    int literalLength = pos - anchor;
                    WriteToken(writer, literalLength, matchLength);

                    // 写入字面量数据
                    if (literalLength > 0)
                    {
                        writer.Write(data, anchor, literalLength);
                    }

                    // 写入偏移量
                    int offset_value = pos - matchPos;
                    writer.Write((byte)(offset_value >> 8));
                    writer.Write((byte)offset_value);

                    pos += matchLength;
                    anchor = pos;
                }
                else
                {
                    pos++;
                }
            }

            // 写入最后的字面量
            int finalLiteralLength = end - anchor;
            WriteToken(writer, finalLiteralLength, 0);

            if (finalLiteralLength > 0)
            {
                writer.Write(data, anchor, finalLiteralLength);
            }

            return output.ToArray();
        }

        private static int FindMatch(byte[] data, int pos, int end, out int matchLength)
        {
            matchLength = 0;
            int bestMatchPos = -1;
            int bestMatchLength = 0;

            int searchStart = Math.Max(0, pos - MaxOffset);

            for (int i = searchStart; i < pos; i++)
            {
                int len = 0;
                int maxLen = Math.Min(end - pos, 255 + MinMatch);

                while (len < maxLen && data[i + len] == data[pos + len])
                {
                    len++;
                }

                if (len >= MinMatch && len > bestMatchLength)
                {
                    bestMatchPos = i;
                    bestMatchLength = len;
                }
            }

            matchLength = bestMatchLength;
            return bestMatchPos;
        }

        private static void WriteToken(BinaryWriter writer, int literalLength, int matchLength)
        {
            int token = Math.Min(literalLength, 15) << 4;
            token |= Math.Min(matchLength - MinMatch, 15);

            writer.Write((byte)token);

            // 写入扩展的字面量长度
            if (literalLength >= 15)
            {
                literalLength -= 15;
                while (literalLength >= 255)
                {
                    writer.Write((byte)255);
                    literalLength -= 255;
                }
                writer.Write((byte)literalLength);
            }

            // 写入扩展的匹配长度
            if (matchLength - MinMatch >= 15)
            {
                int extraLength = matchLength - MinMatch - 15;
                while (extraLength >= 255)
                {
                    writer.Write((byte)255);
                    extraLength -= 255;
                }
                writer.Write((byte)extraLength);
            }
        }

        #endregion

        #region 解压

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="compressed">压缩的数据</param>
        /// <returns>解压后的数据</returns>
        public static byte[] Decompress(byte[] compressed)
        {
            if (compressed == null || compressed.Length == 0)
                return Array.Empty<byte>();

            return Decompress(compressed, 0, compressed.Length);
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="compressed">压缩的数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="length">数据长度</param>
        /// <returns>解压后的数据</returns>
        public static byte[] Decompress(byte[] compressed, int offset, int length)
        {
            if (compressed == null)
                throw new ArgumentNullException(nameof(compressed));
            if (offset < 0 || offset > compressed.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > compressed.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length == 0)
                return Array.Empty<byte>();

            var input = new MemoryStream(compressed, offset, length);
            var reader = new BinaryReader(input);

            // 读取原始长度
            int originalLength = reader.ReadInt32();
            var output = new byte[originalLength];
            int outPos = 0;

            while (input.Position < input.Length && outPos < originalLength)
            {
                // 读取 token
                int token = reader.ReadByte();
                int literalLength = (token >> 4) & 0x0F;
                int matchLength = token & 0x0F;

                // 读取扩展的字面量长度
                if (literalLength == 15)
                {
                    int extra;
                    do
                    {
                        extra = reader.ReadByte();
                        literalLength += extra;
                    } while (extra == 255);
                }

                // 复制字面量
                if (literalLength > 0)
                {
                    Array.Copy(compressed, (int)input.Position, output, outPos, literalLength);
                    input.Position += literalLength;
                    outPos += literalLength;
                }

                if (input.Position >= input.Length || outPos >= originalLength)
                    break;

                // 读取偏移量
                int matchOffset = (reader.ReadByte() << 8) | reader.ReadByte();

                // 读取扩展的匹配长度
                matchLength += MinMatch;
                if ((token & 0x0F) == 15)
                {
                    int extra;
                    do
                    {
                        extra = reader.ReadByte();
                        matchLength += extra;
                    } while (extra == 255);
                }

                // 复制匹配
                int matchPos = outPos - matchOffset;
                for (int i = 0; i < matchLength; i++)
                {
                    output[outPos++] = output[matchPos++];
                }
            }

            return output;
        }

        #endregion

        #region 高级 API

        /// <summary>
        /// 压缩字符串
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>压缩后的 Base64 字符串</returns>
        public static string CompressString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            byte[] compressed = Compress(data);
            return Convert.ToBase64String(compressed);
        }

        /// <summary>
        /// 解压字符串
        /// </summary>
        /// <param name="compressedText">压缩的 Base64 字符串</param>
        /// <returns>原始文本</returns>
        public static string DecompressString(string compressedText)
        {
            if (string.IsNullOrEmpty(compressedText))
                return string.Empty;

            byte[] compressed = Convert.FromBase64String(compressedText);
            byte[] data = Decompress(compressed);
            return System.Text.Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// 获取压缩后的预计最大长度
        /// </summary>
        /// <param name="inputLength">输入长度</param>
        /// <returns>最大输出长度</returns>
        public static int CalculateMaxCompressedLength(int inputLength)
        {
            if (inputLength == 0)
                return 0;

            // LZ4 最坏情况下可能略微增加大小
            return inputLength + (inputLength / 255) + 16 + 4; // 4 for original length
        }

        /// <summary>
        /// 计算压缩比
        /// </summary>
        /// <param name="originalData">原始数据</param>
        /// <param name="compressedData">压缩数据</param>
        /// <returns>压缩比（0-1）</returns>
        public static double CalculateCompressionRatio(byte[] originalData, byte[] compressedData)
        {
            if (originalData == null || originalData.Length == 0)
                return 0;

            if (compressedData == null || compressedData.Length == 0)
                return 1;

            return (double)compressedData.Length / originalData.Length;
        }

        #endregion
    }
}
