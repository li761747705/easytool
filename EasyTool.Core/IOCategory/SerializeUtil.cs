using System;
using System.IO;
using System.Text;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 序列化工具类
    /// </summary>
    public static class SerializeUtil
    {
        #region 二进制序列化

        /// <summary>
        /// 二进制序列化
        /// </summary>
        public static byte[] Serialize<T>(T obj)
        {
            using var stream = new MemoryStream();
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(stream, obj!);
            return stream.ToArray();
        }

        /// <summary>
        /// 二进制反序列化
        /// </summary>
        public static T? Deserialize<T>(byte[] data)
        {
            using var stream = new MemoryStream(data);
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            return (T?)formatter.Deserialize(stream);
        }

        /// <summary>
        /// 序列化到文件
        /// </summary>
        public static void SerializeToFile<T>(T obj, string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var stream = File.Create(filePath);
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(stream, obj!);
        }

        /// <summary>
        /// 从文件反序列化
        /// </summary>
        public static T? DeserializeFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);

            using var stream = File.OpenRead(filePath);
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            return (T?)formatter.Deserialize(stream);
        }

        #endregion

        #region Base64

        /// <summary>
        /// 对象转Base64字符串
        /// </summary>
        public static string ToBase64<T>(T obj)
        {
            var data = Serialize(obj);
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Base64字符串转对象
        /// </summary>
        public static T? FromBase64<T>(string base64)
        {
            var data = Convert.FromBase64String(base64);
            return Deserialize<T>(data);
        }

        #endregion

        #region JSON

        /// <summary>
        /// JSON序列化
        /// </summary>
        public static string ToJson<T>(T obj, bool indented = false)
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = indented,
                PropertyNamingPolicy = null
            };
            return System.Text.Json.JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// JSON反序列化
        /// </summary>
        public static T? FromJson<T>(string json)
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// JSON序列化到文件
        /// </summary>
        public static void ToJsonFile<T>(T obj, string filePath, bool indented = false)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = ToJson(obj, indented);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// 从文件反序列化JSON
        /// </summary>
        public static T? FromJsonFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);

            var json = File.ReadAllText(filePath, Encoding.UTF8);
            return FromJson<T>(json);
        }

        #endregion

        #region 深拷贝

        /// <summary>
        /// 深拷贝对象
        /// </summary>
        public static T DeepClone<T>(T obj)
        {
            if (obj == null)
                return default!;

            // 使用JSON序列化实现深拷贝
            var json = ToJson(obj);
            return FromJson<T>(json)!;
        }

        /// <summary>
        /// 尝试深拷贝
        /// </summary>
        public static bool TryDeepClone<T>(T obj, out T? clone)
        {
            try
            {
                clone = DeepClone(obj);
                return true;
            }
            catch
            {
                clone = default;
                return false;
            }
        }

        #endregion

        #region 对象比较

        /// <summary>
        /// 比较两个对象是否相等（通过序列化比较）
        /// </summary>
        public static bool Equals<T>(T obj1, T obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;
            if (obj1 == null || obj2 == null)
                return false;

            return ToJson(obj1) == ToJson(obj2);
        }

        /// <summary>
        /// 获取对象的哈希值
        /// </summary>
        public static int GetHashCode<T>(T obj)
        {
            if (obj == null)
                return 0;

            return ToJson(obj).GetHashCode();
        }

        #endregion
    }
}
