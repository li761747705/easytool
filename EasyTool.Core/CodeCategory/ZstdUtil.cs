using System;
using System.IO;
using System.Text;

namespace EasyTool.CodeCategory
{
    /// <summary>
    /// Zstandard (Zstd) 压缩工具类
    /// Zstd 是 Facebook 开发的快速压缩算法
    /// 提供了很好的压缩率和速度平衡
    /// 注意：这是一个简化实现，建议生产环境使用官方 Zstd.Net 库
    /// </summary>
    public static class ZstdUtil
    {
        // Zstd 魔数
        private const uint MagicNumber = 0xFD2FB528;

        // 帧头标志
        private const byte FrameHeaderSizeMin = 6;
        private const byte FrameHeaderSizeMax = 14;

        // 块类型
        private const byte BlockTypeRaw = 0;
        private const byte BlockTypeRle = 1;
        private const byte BlockTypeCompressed = 2;
        private const byte BlockTypeReserved = 3;

        // 默认压缩级别
        public const int DefaultCompressionLevel = 3;
        public const int MinCompressionLevel = 1;
        public const int MaxCompressionLevel = 22;

        /// <summary>
        /// 压缩数据
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="compressionLevel">压缩级别（1-22，默认3）</param>
        /// <returns>压缩后的数据</returns>
        public static byte[] Compress(byte[] data, int compressionLevel = DefaultCompressionLevel)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            if (compressionLevel < MinCompressionLevel || compressionLevel > MaxCompressionLevel)
                throw new ArgumentException($"Compression level must be between {MinCompressionLevel} and {MaxCompressionLevel}", nameof(compressionLevel));

            using var output = new MemoryStream();
            using var writer = new BinaryWriter(output);

            // 写入魔数
            writer.Write(MagicNumber);

            // 写入帧头
            byte frameHeaderDescriptor = 0x20; // 单段标志
            writer.Write(frameHeaderDescriptor);

            // 写入窗口描述符（可选，这里简化处理）
            byte windowDescriptor = CalculateWindowDescriptor(data.Length);
            writer.Write(windowDescriptor);

            // 写入字典ID（0表示无字典）
            // 根据帧头描述符，这里不需要

            // 写入内容大小（可选）
            byte fcsField = (byte)(frameHeaderDescriptor >> 6);
            if (fcsField == 0)
            {
                // 单段模式，写入原始内容大小（变长）
                WriteVariableLength(writer, (ulong)data.Length);
            }

            // 写入数据块
            int pos = 0;
            while (pos < data.Length)
            {
                int blockSize = Math.Min(data.Length - pos, 128 * 1024); // 128KB 块
                bool isLast = (pos + blockSize) >= data.Length;

                WriteBlock(writer, data, pos, blockSize, isLast, compressionLevel);
                pos += blockSize;
            }

            // 写入内容校验（可选，这里不包含）

            return output.ToArray();
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="compressed">压缩数据</param>
        /// <returns>原始数据</returns>
        public static byte[] Decompress(byte[] compressed)
        {
            if (compressed == null || compressed.Length < 4)
                return Array.Empty<byte>();

            using var input = new MemoryStream(compressed);
            using var reader = new BinaryReader(input);

            // 读取并验证魔数
            uint magic = reader.ReadUInt32();
            if (magic != MagicNumber)
                throw new InvalidDataException("Invalid Zstd magic number");

            // 读取帧头描述符
            byte frameHeaderDescriptor = reader.ReadByte();
            bool singleSegment = (frameHeaderDescriptor & 0x20) != 0;
            bool contentChecksumFlag = (frameHeaderDescriptor & 0x04) != 0;
            bool dictionaryIdFlag = (frameHeaderDescriptor & 0x01) != 0;
            byte fcsField = (byte)((frameHeaderDescriptor >> 6) & 0x03);

            // 读取窗口描述符（非单段模式）
            ulong windowSize = 0;
            if (!singleSegment)
            {
                byte windowDescriptor = reader.ReadByte();
                windowSize = CalculateWindowSize(windowDescriptor);
            }

            // 读取字典ID
            if (dictionaryIdFlag)
            {
                int dictIdSize = 1 << (frameHeaderDescriptor & 0x03);
                for (int i = 0; i < dictIdSize; i++)
                    reader.ReadByte();
            }

            // 读取内容大小
            ulong contentSize = 0;
            if (singleSegment || fcsField > 0)
            {
                contentSize = ReadVariableLength(reader);
                if (fcsField >= 2)
                {
                    contentSize |= (ulong)reader.ReadByte() << 8;
                    if (fcsField >= 3)
                    {
                        contentSize |= (ulong)reader.ReadByte() << 16;
                        contentSize |= (ulong)reader.ReadByte() << 24;
                    }
                }
            }

            // 读取并解压数据块
            using var output = new MemoryStream();
            bool lastBlock = false;

            while (!lastBlock)
            {
                // 读取块头
                uint blockHeader = reader.ReadUInt32();
                lastBlock = (blockHeader & 0x01) != 0;
                int blockType = (int)((blockHeader >> 1) & 0x03);
                int blockSize = (int)((blockHeader >> 3) & 0x7FFFFF);

                switch (blockType)
                {
                    case BlockTypeRaw:
                        byte[] rawData = reader.ReadBytes(blockSize);
                        output.Write(rawData, 0, rawData.Length);
                        break;

                    case BlockTypeRle:
                        byte rleByte = reader.ReadByte();
                        for (int i = 0; i < blockSize; i++)
                            output.WriteByte(rleByte);
                        break;

                    case BlockTypeCompressed:
                        byte[] compressedBlock = reader.ReadBytes(blockSize);
                        byte[] decompressed = DecompressBlock(compressedBlock);
                        output.Write(decompressed, 0, decompressed.Length);
                        break;

                    default:
                        throw new InvalidDataException($"Unknown block type: {blockType}");
                }
            }

            // 读取内容校验（如果有）
            if (contentChecksumFlag)
            {
                reader.ReadUInt32(); // 跳过校验和
            }

            return output.ToArray();
        }

        /// <summary>
        /// 压缩字符串
        /// </summary>
        public static string CompressToBase64(string text, int compressionLevel = DefaultCompressionLevel)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] compressed = Compress(data, compressionLevel);
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
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// 获取压缩绑定的最大输出大小
        /// </summary>
        public static int Bound(int sourceSize)
        {
            if (sourceSize < 0)
                throw new ArgumentException("Source size cannot be negative", nameof(sourceSize));

            return sourceSize + (sourceSize / 255) + 16;
        }

        private static void WriteBlock(BinaryWriter writer, byte[] data, int offset, int length, bool isLast, int compressionLevel)
        {
            // 简化实现：使用 LZ4 风格的快速压缩
            byte[] compressed = CompressBlockSimple(data, offset, length);

            if (compressed.Length >= length)
            {
                // 原始数据更好
                uint header = (uint)length << 3 | (uint)BlockTypeRaw << 1 | (isLast ? 1u : 0u);
                writer.Write(header);
                writer.Write(data, offset, length);
            }
            else
            {
                uint header = (uint)compressed.Length << 3 | (uint)BlockTypeCompressed << 1 | (isLast ? 1u : 0u);
                writer.Write(header);
                writer.Write(compressed);
            }
        }

        private static byte[] CompressBlockSimple(byte[] data, int offset, int length)
        {
            using var output = new MemoryStream();

            int pos = offset;
            int end = offset + length;

            while (pos < end)
            {
                // 查找匹配
                int bestMatch = 0;
                int bestLength = 0;

                int searchStart = Math.Max(offset, pos - 8192);
                for (int i = searchStart; i < pos; i++)
                {
                    int matchLen = 0;
                    while (pos + matchLen < end && data[i + matchLen] == data[pos + matchLen] && matchLen < 255)
                        matchLen++;

                    if (matchLen > bestLength)
                    {
                        bestLength = matchLen;
                        bestMatch = i;
                    }
                }

                if (bestLength >= 4)
                {
                    // 写入匹配
                    int distance = pos - bestMatch;
                    output.WriteByte((byte)(bestLength - 4));
                    output.WriteByte((byte)(distance & 0xFF));
                    output.WriteByte((byte)((distance >> 8) & 0xFF));
                    pos += bestLength;
                }
                else
                {
                    // 写入字面量
                    output.WriteByte(0xFF); // 标记为字面量
                    output.WriteByte(data[pos]);
                    pos++;
                }
            }

            return output.ToArray();
        }

        private static byte[] DecompressBlock(byte[] compressed)
        {
            using var output = new MemoryStream();
            int pos = 0;

            while (pos < compressed.Length)
            {
                byte marker = compressed[pos++];

                if (marker == 0xFF)
                {
                    // 字面量
                    if (pos < compressed.Length)
                        output.WriteByte(compressed[pos++]);
                }
                else
                {
                    // 匹配
                    int length = marker + 4;
                    if (pos + 1 < compressed.Length)
                    {
                        int distance = compressed[pos] | (compressed[pos + 1] << 8);
                        pos += 2;

                        int srcPos = (int)output.Position - distance;
                        for (int i = 0; i < length; i++)
                        {
                            byte b = output.ToArray()[srcPos + i];
                            output.WriteByte(b);
                        }
                    }
                }
            }

            return output.ToArray();
        }

        private static byte CalculateWindowDescriptor(int size)
        {
            // 计算适合的窗口大小
            int exponent = 10; // 最小 1KB
            while ((1 << exponent) < size && exponent < 30)
                exponent++;

            return (byte)(exponent - 10);
        }

        private static ulong CalculateWindowSize(byte descriptor)
        {
            int exponent = (descriptor & 0x1F) + 10;
            int mantissa = (descriptor >> 5) & 0x07;

            return (1ul << exponent) + (ulong)mantissa * (1ul << (exponent - 3));
        }

        private static void WriteVariableLength(BinaryWriter writer, ulong value)
        {
            while (value >= 128)
            {
                writer.Write((byte)(value | 0x80));
                value >>= 7;
            }
            writer.Write((byte)value);
        }

        private static ulong ReadVariableLength(BinaryReader reader)
        {
            ulong result = 0;
            int shift = 0;
            byte b;

            do
            {
                b = reader.ReadByte();
                result |= (ulong)(b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return result;
        }
    }
}
