using System;
using System.Collections.Generic;
using System.IO;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Snappy 压缩工具类
    /// Snappy 是 Google 开发的快速压缩算法，注重速度而非压缩率
    /// 广泛用于大数据处理框架如 Hadoop、Spark
    /// </summary>
    public static class SnappyUtil
    {
        private const int MaxBlockSize = 65536;
        private const int MaxInputSize = 2147483647;

        // 操作类型
        private const byte Literal = 0;
        private const byte Copy1ByteOffset = 1;
        private const byte Copy2ByteOffset = 2;
        private const byte Copy4ByteOffset = 3;

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <returns>压缩后的数据</returns>
        public static byte[] Compress(byte[] data)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using var output = new MemoryStream();
            using var writer = new BinaryWriter(output);

            // 写入变长长度
            WriteVarInt(writer, data.Length);

            int pos = 0;
            while (pos < data.Length)
            {
                int remaining = data.Length - pos;
                int blockSize = Math.Min(remaining, MaxBlockSize);

                CompressBlock(data, pos, blockSize, writer);
                pos += blockSize;
            }

            return output.ToArray();
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="compressed">压缩数据</param>
        /// <returns>原始数据</returns>
        public static byte[] Decompress(byte[] compressed)
        {
            if (compressed == null || compressed.Length == 0)
                return Array.Empty<byte>();

            using var input = new MemoryStream(compressed);
            using var reader = new BinaryReader(input);

            // 读取原始长度
            int originalLength = ReadVarInt(reader);
            byte[] result = new byte[originalLength];

            int pos = 0;
            while (pos < originalLength)
            {
                int remaining = originalLength - pos;
                int blockSize = Math.Min(remaining, MaxBlockSize);

                DecompressBlock(reader, result, pos, blockSize);
                pos += blockSize;
            }

            return result;
        }

        /// <summary>
        /// 压缩字符串
        /// </summary>
        public static string CompressToBase64(string text)
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
        public static string DecompressFromBase64(string compressedBase64)
        {
            if (string.IsNullOrEmpty(compressedBase64))
                return string.Empty;

            byte[] compressed = Convert.FromBase64String(compressedBase64);
            byte[] data = Decompress(compressed);
            return System.Text.Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// 获取压缩后预估大小
        /// </summary>
        public static int MaxCompressedLength(int sourceLength)
        {
            if (sourceLength < 0)
                throw new ArgumentException("Source length cannot be negative", nameof(sourceLength));

            // 变长整数最大 5 字节 + 每个块最大开销
            int blocks = (sourceLength + MaxBlockSize - 1) / MaxBlockSize;
            return 5 + sourceLength + blocks * 4;
        }

        private static void CompressBlock(byte[] input, int inputOffset, int inputLength, BinaryWriter writer)
        {
            int pos = inputOffset;
            int end = inputOffset + inputLength;

            while (pos < end)
            {
                // 查找最长匹配
                int matchOffset = 0;
                int matchLength = 0;

                // 简化的哈希表查找
                if (pos + 4 <= end)
                {
                    FindMatch(input, pos, end, ref matchOffset, ref matchLength);
                }

                if (matchLength >= 4)
                {
                    // 写入字面量（如果有）
                    // 写入复制操作
                    int offset = pos - matchOffset - 1;
                    int length = matchLength - 4;

                    if (offset < 2048 && length < 8)
                    {
                        // Copy1 或 Copy2
                        writer.Write((byte)((length << 2) | Copy1ByteOffset | (offset > 255 ? 0x80 : 0)));
                        if (offset > 255)
                            writer.Write((byte)((offset >> 8) | ((length - 8) << 5)));
                        writer.Write((byte)(offset & 0xFF));
                    }
                    else if (offset < 65536)
                    {
                        writer.Write((byte)((length << 2) | Copy2ByteOffset));
                        writer.Write((byte)(offset & 0xFF));
                        writer.Write((byte)((offset >> 8) & 0xFF));
                    }
                    else
                    {
                        writer.Write((byte)((length << 2) | Copy4ByteOffset));
                        writer.Write((byte)(offset & 0xFF));
                        writer.Write((byte)((offset >> 8) & 0xFF));
                        writer.Write((byte)((offset >> 16) & 0xFF));
                        writer.Write((byte)((offset >> 24) & 0xFF));
                    }

                    pos += matchLength;
                }
                else
                {
                    // 写入字面量
                    int literalLength = 1;
                    while (pos + literalLength < end && literalLength < 60)
                    {
                        if (FindMatchAt(input, pos + literalLength, end))
                            break;
                        literalLength++;
                    }

                    WriteLiteral(input, pos, literalLength, writer);
                    pos += literalLength;
                }
            }
        }

        private static void FindMatch(byte[] input, int pos, int end, ref int matchOffset, ref int matchLength)
        {
            // 简化的匹配查找
            int searchStart = Math.Max(0, pos - 65536);
            int bestLength = 0;
            int bestOffset = 0;

            for (int i = searchStart; i < pos; i++)
            {
                int length = 0;
                int maxLen = Math.Min(end - pos, 64);

                while (length < maxLen && input[i + length] == input[pos + length])
                {
                    length++;
                }

                if (length > bestLength)
                {
                    bestLength = length;
                    bestOffset = i;
                }
            }

            if (bestLength >= 4)
            {
                matchOffset = bestOffset;
                matchLength = bestLength;
            }
        }

        private static bool FindMatchAt(byte[] input, int pos, int end)
        {
            if (pos + 4 > end)
                return false;

            int searchStart = Math.Max(0, pos - 65536);
            for (int i = searchStart; i < pos; i++)
            {
                int length = 0;
                while (length < 4 && input[i + length] == input[pos + length])
                    length++;

                if (length >= 4)
                    return true;
            }

            return false;
        }

        private static void WriteLiteral(byte[] input, int offset, int length, BinaryWriter writer)
        {
            if (length < 60)
            {
                writer.Write((byte)((length - 1) << 2));
            }
            else if (length < 256)
            {
                writer.Write((byte)(60 << 2));
                writer.Write((byte)(length - 1));
            }
            else
            {
                writer.Write((byte)(61 << 2));
                writer.Write((byte)((length - 1) & 0xFF));
                writer.Write((byte)(((length - 1) >> 8) & 0xFF));
            }

            writer.Write(input, offset, length);
        }

        private static void DecompressBlock(BinaryReader reader, byte[] output, int outputOffset, int outputLength)
        {
            int pos = outputOffset;
            int end = outputOffset + outputLength;

            while (pos < end)
            {
                byte op = reader.ReadByte();
                int opType = op & 0x03;

                if (opType == Literal)
                {
                    int length;
                    if ((op >> 2) < 60)
                    {
                        length = (op >> 2) + 1;
                    }
                    else if ((op >> 2) == 60)
                    {
                        length = reader.ReadByte() + 1;
                    }
                    else
                    {
                        int extraBytes = (op >> 2) - 60 + 1;
                        length = 0;
                        for (int i = 0; i < extraBytes; i++)
                        {
                            length |= reader.ReadByte() << (i * 8);
                        }
                        length += 1;
                    }

                    byte[] literal = reader.ReadBytes(length);
                    Array.Copy(literal, 0, output, pos, length);
                    pos += length;
                }
                else
                {
                    int length, offset;

                    if (opType == Copy1ByteOffset)
                    {
                        length = ((op >> 2) & 0x07) + 4;
                        offset = ((op & 0xE0) << 3) | reader.ReadByte();
                    }
                    else if (opType == Copy2ByteOffset)
                    {
                        length = (op >> 2) + 1;
                        offset = reader.ReadByte() | (reader.ReadByte() << 8);
                    }
                    else
                    {
                        length = (op >> 2) + 1;
                        offset = reader.ReadByte() | (reader.ReadByte() << 8) |
                                 (reader.ReadByte() << 16) | (reader.ReadByte() << 24);
                    }

                    int srcPos = pos - offset;
                    for (int i = 0; i < length; i++)
                    {
                        output[pos++] = output[srcPos++];
                    }
                }
            }
        }

        private static void WriteVarInt(BinaryWriter writer, int value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)(value | 0x80));
                value >>= 7;
            }
            writer.Write((byte)value);
        }

        private static int ReadVarInt(BinaryReader reader)
        {
            int result = 0;
            int shift = 0;
            byte b;

            do
            {
                b = reader.ReadByte();
                result |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return result;
        }
    }
}
