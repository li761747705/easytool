using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyTool.ConvertCategory
{
    /// <summary>
    /// MessagePack 转换工具类（轻量级实现，无需第三方库）
    /// 支持基本的 MessagePack 序列化和反序列化
    /// </summary>
    public static class MsgPackConvertUtil
    {
        #region 序列化

        /// <summary>
        /// 将对象序列化为 MessagePack 字节数组
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <returns>MessagePack 字节数组</returns>
        public static byte[] Serialize<T>(T obj)
        {
            using var stream = new MemoryStream();
            SerializeValue(obj, stream);
            return stream.ToArray();
        }

        /// <summary>
        /// 将对象序列化为 MessagePack 并写入流
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="stream">目标流</param>
        public static void Serialize<T>(T obj, Stream stream)
        {
            SerializeValue(obj, stream);
        }

        /// <summary>
        /// 将字典序列化为 MessagePack 字节数组
        /// </summary>
        /// <param name="dict">要序列化的字典</param>
        /// <returns>MessagePack 字节数组</returns>
        public static byte[] SerializeDictionary(IDictionary dict)
        {
            using var stream = new MemoryStream();
            SerializeDictionary(dict, stream);
            return stream.ToArray();
        }

        private static void SerializeValue(object? value, Stream stream)
        {
            if (value == null)
            {
                WriteNil(stream);
                return;
            }

            var type = value.GetType();

            // 布尔值
            if (type == typeof(bool))
            {
                WriteBool((bool)value, stream);
                return;
            }

            // 整数类型
            if (type == typeof(sbyte)) { WriteInteger((sbyte)value, stream); return; }
            if (type == typeof(byte)) { WriteInteger((byte)value, stream); return; }
            if (type == typeof(short)) { WriteInteger((short)value, stream); return; }
            if (type == typeof(ushort)) { WriteInteger((ushort)value, stream); return; }
            if (type == typeof(int)) { WriteInteger((int)value, stream); return; }
            if (type == typeof(uint)) { WriteInteger((uint)value, stream); return; }
            if (type == typeof(long)) { WriteInteger((long)value, stream); return; }
            if (type == typeof(ulong)) { WriteInteger((ulong)value, stream); return; }

            // 浮点数
            if (type == typeof(float)) { WriteFloat((float)value, stream); return; }
            if (type == typeof(double)) { WriteDouble((double)value, stream); return; }

            // 字符串
            if (type == typeof(string))
            {
                WriteString((string)value, stream);
                return;
            }

            // 字节数组
            if (type == typeof(byte[]))
            {
                WriteBinary((byte[])value, stream);
                return;
            }

            // 数组和列表
            if (value is IEnumerable enumerable and not string and not IDictionary)
            {
                SerializeArray(enumerable, stream);
                return;
            }

            // 字典
            if (value is IDictionary dict)
            {
                SerializeDictionary(dict, stream);
                return;
            }

            // 其他对象
            SerializeObject(value, stream);
        }

        private static void WriteNil(Stream stream)
        {
            stream.WriteByte(0xC0);
        }

        private static void WriteBool(bool value, Stream stream)
        {
            stream.WriteByte(value ? (byte)0xC3 : (byte)0xC2);
        }

        private static void WriteInteger(sbyte value, Stream stream)
        {
            if (value >= 0)
            {
                WriteInteger((ulong)value, stream);
            }
            else
            {
                stream.WriteByte(0xD0);
                stream.WriteByte((byte)value);
            }
        }

        private static void WriteInteger(byte value, Stream stream)
        {
            WriteInteger((ulong)value, stream);
        }

        private static void WriteInteger(short value, Stream stream)
        {
            if (value >= 0)
            {
                WriteInteger((ulong)value, stream);
            }
            else if (value >= sbyte.MinValue)
            {
                stream.WriteByte(0xD0);
                stream.WriteByte((byte)(sbyte)value);
            }
            else
            {
                stream.WriteByte(0xD1);
                WriteBigEndianInt16(value, stream);
            }
        }

        private static void WriteInteger(ushort value, Stream stream)
        {
            WriteInteger((ulong)value, stream);
        }

        private static void WriteInteger(int value, Stream stream)
        {
            if (value >= 0)
            {
                WriteInteger((ulong)value, stream);
            }
            else if (value >= sbyte.MinValue)
            {
                stream.WriteByte(0xD0);
                stream.WriteByte((byte)(sbyte)value);
            }
            else if (value >= short.MinValue)
            {
                stream.WriteByte(0xD1);
                WriteBigEndianInt16((short)value, stream);
            }
            else
            {
                stream.WriteByte(0xD2);
                WriteBigEndianInt32(value, stream);
            }
        }

        private static void WriteInteger(uint value, Stream stream)
        {
            WriteInteger((ulong)value, stream);
        }

        private static void WriteInteger(long value, Stream stream)
        {
            if (value >= 0)
            {
                WriteInteger((ulong)value, stream);
            }
            else if (value >= sbyte.MinValue)
            {
                stream.WriteByte(0xD0);
                stream.WriteByte((byte)(sbyte)value);
            }
            else if (value >= short.MinValue)
            {
                stream.WriteByte(0xD1);
                WriteBigEndianInt16((short)value, stream);
            }
            else if (value >= int.MinValue)
            {
                stream.WriteByte(0xD2);
                WriteBigEndianInt32((int)value, stream);
            }
            else
            {
                stream.WriteByte(0xD3);
                WriteBigEndianInt64(value, stream);
            }
        }

        private static void WriteInteger(ulong value, Stream stream)
        {
            if (value <= 127)
            {
                // Positive FixInt
                stream.WriteByte((byte)value);
            }
            else if (value <= byte.MaxValue)
            {
                stream.WriteByte(0xCC);
                stream.WriteByte((byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                stream.WriteByte(0xCD);
                WriteBigEndianUInt16((ushort)value, stream);
            }
            else if (value <= uint.MaxValue)
            {
                stream.WriteByte(0xCE);
                WriteBigEndianUInt32((uint)value, stream);
            }
            else
            {
                stream.WriteByte(0xCF);
                WriteBigEndianUInt64(value, stream);
            }
        }

        private static void WriteFloat(float value, Stream stream)
        {
            stream.WriteByte(0xCA);
            var bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, 4);
        }

        private static void WriteDouble(double value, Stream stream)
        {
            stream.WriteByte(0xCB);
            var bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, 8);
        }

        private static void WriteString(string value, Stream stream)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var length = bytes.Length;

            if (length <= 31)
            {
                // FixStr
                stream.WriteByte((byte)(0xA0 | length));
            }
            else if (length <= byte.MaxValue)
            {
                stream.WriteByte(0xD9);
                stream.WriteByte((byte)length);
            }
            else if (length <= ushort.MaxValue)
            {
                stream.WriteByte(0xDA);
                WriteBigEndianUInt16((ushort)length, stream);
            }
            else
            {
                stream.WriteByte(0xDB);
                WriteBigEndianUInt32((uint)length, stream);
            }

            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteBinary(byte[] value, Stream stream)
        {
            var length = value.Length;

            if (length <= byte.MaxValue)
            {
                stream.WriteByte(0xC4);
                stream.WriteByte((byte)length);
            }
            else if (length <= ushort.MaxValue)
            {
                stream.WriteByte(0xC5);
                WriteBigEndianUInt16((ushort)length, stream);
            }
            else
            {
                stream.WriteByte(0xC6);
                WriteBigEndianUInt32((uint)length, stream);
            }

            stream.Write(value, 0, length);
        }

        private static void SerializeArray(IEnumerable enumerable, Stream stream)
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
            {
                list.Add(item);
            }

            var count = list.Count;

            if (count <= 15)
            {
                // FixArray
                stream.WriteByte((byte)(0x90 | count));
            }
            else if (count <= ushort.MaxValue)
            {
                stream.WriteByte(0xDC);
                WriteBigEndianUInt16((ushort)count, stream);
            }
            else
            {
                stream.WriteByte(0xDD);
                WriteBigEndianUInt32((uint)count, stream);
            }

            foreach (var item in list)
            {
                SerializeValue(item, stream);
            }
        }

        private static void SerializeDictionary(IDictionary dict, Stream stream)
        {
            var count = dict.Count;

            if (count <= 15)
            {
                // FixMap
                stream.WriteByte((byte)(0x80 | count));
            }
            else if (count <= ushort.MaxValue)
            {
                stream.WriteByte(0xDE);
                WriteBigEndianUInt16((ushort)count, stream);
            }
            else
            {
                stream.WriteByte(0xDF);
                WriteBigEndianUInt32((uint)count, stream);
            }

            foreach (DictionaryEntry entry in dict)
            {
                SerializeValue(entry.Key, stream);
                SerializeValue(entry.Value, stream);
            }
        }

        private static void SerializeObject(object obj, Stream stream)
        {
            var type = obj.GetType();
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var count = 0;
            foreach (var prop in properties)
            {
                if (prop.CanRead)
                    count++;
            }

            if (count <= 15)
            {
                stream.WriteByte((byte)(0x80 | count));
            }
            else if (count <= ushort.MaxValue)
            {
                stream.WriteByte(0xDE);
                WriteBigEndianUInt16((ushort)count, stream);
            }
            else
            {
                stream.WriteByte(0xDF);
                WriteBigEndianUInt32((uint)count, stream);
            }

            foreach (var prop in properties)
            {
                if (!prop.CanRead)
                    continue;

                WriteString(prop.Name, stream);
                SerializeValue(prop.GetValue(obj), stream);
            }
        }

        private static void WriteBigEndianInt16(short value, Stream stream)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteBigEndianUInt16(ushort value, Stream stream)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteBigEndianInt32(int value, Stream stream)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteBigEndianUInt32(uint value, Stream stream)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteBigEndianInt64(long value, Stream stream)
        {
            stream.WriteByte((byte)(value >> 56));
            stream.WriteByte((byte)(value >> 48));
            stream.WriteByte((byte)(value >> 40));
            stream.WriteByte((byte)(value >> 32));
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        private static void WriteBigEndianUInt64(ulong value, Stream stream)
        {
            stream.WriteByte((byte)(value >> 56));
            stream.WriteByte((byte)(value >> 48));
            stream.WriteByte((byte)(value >> 40));
            stream.WriteByte((byte)(value >> 32));
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }

        #endregion

        #region 反序列化

        /// <summary>
        /// 从 MessagePack 字节数组反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="data">MessagePack 字节数组</param>
        /// <returns>反序列化的对象</returns>
        public static T? Deserialize<T>(byte[] data)
        {
            using var stream = new MemoryStream(data);
            return Deserialize<T>(stream);
        }

        /// <summary>
        /// 从流中反序列化对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="stream">数据流</param>
        /// <returns>反序列化的对象</returns>
        public static T? Deserialize<T>(Stream stream)
        {
            var value = DeserializeValue(stream);
            return ConvertValue<T>(value);
        }

        /// <summary>
        /// 从 MessagePack 字节数组反序列化为字典
        /// </summary>
        /// <param name="data">MessagePack 字节数组</param>
        /// <returns>字典对象</returns>
        public static Dictionary<string, object?> DeserializeToDictionary(byte[] data)
        {
            using var stream = new MemoryStream(data);
            return DeserializeToDictionary(stream);
        }

        /// <summary>
        /// 从流中反序列化为字典
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <returns>字典对象</returns>
        public static Dictionary<string, object?> DeserializeToDictionary(Stream stream)
        {
            var value = DeserializeValue(stream);
            if (value is Dictionary<string, object?> dict)
            {
                return dict;
            }
            return new Dictionary<string, object?>();
        }

        private static object? DeserializeValue(Stream stream)
        {
            var header = stream.ReadByte();
            if (header < 0)
                throw new EndOfStreamException();

            // Positive FixInt (0x00 - 0x7F)
            if (header <= 0x7F)
            {
                return (byte)header;
            }

            // FixMap (0x80 - 0x8F)
            if ((header & 0xF0) == 0x80)
            {
                var count = header & 0x0F;
                return DeserializeMap(stream, count);
            }

            // FixArray (0x90 - 0x9F)
            if ((header & 0xF0) == 0x90)
            {
                var count = header & 0x0F;
                return DeserializeArray(stream, count);
            }

            // FixStr (0xA0 - 0xBF)
            if ((header & 0xE0) == 0xA0)
            {
                var length = header & 0x1F;
                return DeserializeString(stream, length);
            }

            // Negative FixInt (0xE0 - 0xFF)
            if (header >= 0xE0)
            {
                return (sbyte)(byte)header;
            }

            // 其他格式
            switch (header)
            {
                case 0xC0: // nil
                    return null;

                case 0xC2: // false
                    return false;

                case 0xC3: // true
                    return true;

                case 0xC4: // bin 8
                    return DeserializeBinary(stream, ReadUInt8(stream));

                case 0xC5: // bin 16
                    return DeserializeBinary(stream, (int)ReadBigEndianUInt16(stream));

                case 0xC6: // bin 32
                    return DeserializeBinary(stream, (int)ReadBigEndianUInt32(stream));

                case 0xC7: // ext 8
                case 0xC8: // ext 16
                case 0xC9: // ext 32
                    throw new NotSupportedException("Extension types are not supported");

                case 0xCA: // float 32
                    return ReadFloat(stream);

                case 0xCB: // float 64
                    return ReadDouble(stream);

                case 0xCC: // uint 8
                    return ReadUInt8(stream);

                case 0xCD: // uint 16
                    return ReadBigEndianUInt16(stream);

                case 0xCE: // uint 32
                    return ReadBigEndianUInt32(stream);

                case 0xCF: // uint 64
                    return ReadBigEndianUInt64(stream);

                case 0xD0: // int 8
                    return ReadInt8(stream);

                case 0xD1: // int 16
                    return ReadBigEndianInt16(stream);

                case 0xD2: // int 32
                    return ReadBigEndianInt32(stream);

                case 0xD3: // int 64
                    return ReadBigEndianInt64(stream);

                case 0xD9: // str 8
                    return DeserializeString(stream, ReadUInt8(stream));

                case 0xDA: // str 16
                    return DeserializeString(stream, (int)ReadBigEndianUInt16(stream));

                case 0xDB: // str 32
                    return DeserializeString(stream, (int)ReadBigEndianUInt32(stream));

                case 0xDC: // array 16
                    return DeserializeArray(stream, (int)ReadBigEndianUInt16(stream));

                case 0xDD: // array 32
                    return DeserializeArray(stream, (int)ReadBigEndianUInt32(stream));

                case 0xDE: // map 16
                    return DeserializeMap(stream, (int)ReadBigEndianUInt16(stream));

                case 0xDF: // map 32
                    return DeserializeMap(stream, (int)ReadBigEndianUInt32(stream));

                default:
                    throw new NotSupportedException($"Unknown format: 0x{header:X2}");
            }
        }

        private static sbyte ReadInt8(Stream stream)
        {
            var b = stream.ReadByte();
            if (b < 0) throw new EndOfStreamException();
            return (sbyte)b;
        }

        private static byte ReadUInt8(Stream stream)
        {
            var b = stream.ReadByte();
            if (b < 0) throw new EndOfStreamException();
            return (byte)b;
        }

        private static short ReadBigEndianInt16(Stream stream)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            if (b1 < 0 || b2 < 0) throw new EndOfStreamException();
            return (short)((b1 << 8) | b2);
        }

        private static ushort ReadBigEndianUInt16(Stream stream)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            if (b1 < 0 || b2 < 0) throw new EndOfStreamException();
            return (ushort)((b1 << 8) | b2);
        }

        private static int ReadBigEndianInt32(Stream stream)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            var b3 = stream.ReadByte();
            var b4 = stream.ReadByte();
            if (b1 < 0 || b2 < 0 || b3 < 0 || b4 < 0) throw new EndOfStreamException();
            return (b1 << 24) | (b2 << 16) | (b3 << 8) | b4;
        }

        private static uint ReadBigEndianUInt32(Stream stream)
        {
            var b1 = stream.ReadByte();
            var b2 = stream.ReadByte();
            var b3 = stream.ReadByte();
            var b4 = stream.ReadByte();
            if (b1 < 0 || b2 < 0 || b3 < 0 || b4 < 0) throw new EndOfStreamException();
            return (uint)((b1 << 24) | (b2 << 16) | (b3 << 8) | b4);
        }

        private static long ReadBigEndianInt64(Stream stream)
        {
            var bytes = new byte[8];
            var read = stream.Read(bytes, 0, 8);
            if (read < 8) throw new EndOfStreamException();

            return ((long)bytes[0] << 56) | ((long)bytes[1] << 48) |
                   ((long)bytes[2] << 40) | ((long)bytes[3] << 32) |
                   ((long)bytes[4] << 24) | ((long)bytes[5] << 16) |
                   ((long)bytes[6] << 8) | bytes[7];
        }

        private static ulong ReadBigEndianUInt64(Stream stream)
        {
            var bytes = new byte[8];
            var read = stream.Read(bytes, 0, 8);
            if (read < 8) throw new EndOfStreamException();

            return ((ulong)bytes[0] << 56) | ((ulong)bytes[1] << 48) |
                   ((ulong)bytes[2] << 40) | ((ulong)bytes[3] << 32) |
                   ((ulong)bytes[4] << 24) | ((ulong)bytes[5] << 16) |
                   ((ulong)bytes[6] << 8) | bytes[7];
        }

        private static float ReadFloat(Stream stream)
        {
            var bytes = new byte[4];
            var read = stream.Read(bytes, 0, 4);
            if (read < 4) throw new EndOfStreamException();
            return BitConverter.ToSingle(bytes, 0);
        }

        private static double ReadDouble(Stream stream)
        {
            var bytes = new byte[8];
            var read = stream.Read(bytes, 0, 8);
            if (read < 8) throw new EndOfStreamException();
            return BitConverter.ToDouble(bytes, 0);
        }

        private static string DeserializeString(Stream stream, int length)
        {
            var bytes = new byte[length];
            var read = stream.Read(bytes, 0, length);
            if (read < length) throw new EndOfStreamException();
            return Encoding.UTF8.GetString(bytes);
        }

        private static byte[] DeserializeBinary(Stream stream, int length)
        {
            var bytes = new byte[length];
            var read = stream.Read(bytes, 0, length);
            if (read < length) throw new EndOfStreamException();
            return bytes;
        }

        private static List<object?> DeserializeArray(Stream stream, int count)
        {
            var list = new List<object?>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(DeserializeValue(stream));
            }
            return list;
        }

        private static Dictionary<string, object?> DeserializeMap(Stream stream, int count)
        {
            var dict = new Dictionary<string, object?>(count);
            for (int i = 0; i < count; i++)
            {
                var key = DeserializeValue(stream);
                var value = DeserializeValue(stream);
                dict[key?.ToString() ?? ""] = value;
            }
            return dict;
        }

        private static T? ConvertValue<T>(object? value)
        {
            if (value == null)
                return default;

            if (value is T typedValue)
                return typedValue;

            var targetType = typeof(T);

            if (targetType == typeof(string))
            {
                return (T)(object)value.ToString()!;
            }

            return (T)Convert.ChangeType(value, targetType);
        }

        #endregion
    }
}
