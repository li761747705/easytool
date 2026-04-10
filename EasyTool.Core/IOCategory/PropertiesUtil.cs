using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// Properties 配置文件工具类
    /// 用于读写 Java 风格的 .properties 配置文件
    /// </summary>
    public static class PropertiesUtil
    {
        #region 读取方法

        /// <summary>
        /// 从文件加载 Properties
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码方式（默认UTF-8）</param>
        /// <returns>Properties 字典</returns>
        public static Dictionary<string, string> Load(string filePath, Encoding? encoding = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Properties 文件不存在", filePath);

            encoding ??= Encoding.UTF8;
            var lines = File.ReadAllLines(filePath, encoding);
            return ParseLines(lines);
        }

        /// <summary>
        /// 从文件异步加载 Properties
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>Properties 字典</returns>
        public static async System.Threading.Tasks.Task<Dictionary<string, string>> LoadAsync(string filePath, Encoding? encoding = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Properties 文件不存在", filePath);

            encoding ??= Encoding.UTF8;
            using var reader = new StreamReader(filePath, encoding);
            var content = await reader.ReadToEndAsync().ConfigureAwait(false);
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            return ParseLines(lines);
        }

        /// <summary>
        /// 从字符串加载 Properties
        /// </summary>
        /// <param name="content">Properties 内容</param>
        /// <returns>Properties 字典</returns>
        public static Dictionary<string, string> Parse(string content)
        {
            if (string.IsNullOrEmpty(content))
                return new Dictionary<string, string>();

            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            return ParseLines(lines);
        }

        /// <summary>
        /// 从流加载 Properties
        /// </summary>
        /// <param name="stream">输入流</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>Properties 字典</returns>
        public static Dictionary<string, string> LoadFromStream(Stream stream, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            using var reader = new StreamReader(stream, encoding);
            var content = reader.ReadToEnd();
            return Parse(content);
        }

        private static Dictionary<string, string> ParseLines(string[] lines)
        {
            var properties = new Dictionary<string, string>();
            int lineNumber = 0;

            foreach (var originalLine in lines)
            {
                lineNumber++;
                string line = originalLine.Trim();

                // 跳过空行和注释
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("!"))
                    continue;

                // 查找分隔符
                int separatorIndex = FindSeparator(line);
                if (separatorIndex < 0)
                    continue;

                string key = UnescapeKey(line.Substring(0, separatorIndex).Trim());
                string value = separatorIndex < line.Length - 1
                    ? UnescapeValue(line.Substring(separatorIndex + 1).TrimStart())
                    : string.Empty;

                properties[key] = value;
            }

            return properties;
        }

        private static int FindSeparator(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '=' || c == ':' || char.IsWhiteSpace(c))
                {
                    // 检查是否被转义
                    if (i > 0 && line[i - 1] == '\\')
                        continue;
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region 保存方法

        /// <summary>
        /// 保存 Properties 到文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="properties">Properties 字典</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="comment">注释（可选）</param>
        public static void Save(string filePath, Dictionary<string, string> properties, Encoding? encoding = null, string? comment = null)
        {
            encoding ??= Encoding.UTF8;
            var content = BuildContent(properties, comment);
            File.WriteAllText(filePath, content, encoding);
        }

        /// <summary>
        /// 异步保存 Properties 到文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="properties">Properties 字典</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="comment">注释</param>
        public static async System.Threading.Tasks.Task SaveAsync(string filePath, Dictionary<string, string> properties, Encoding? encoding = null, string? comment = null)
        {
            encoding ??= Encoding.UTF8;
            var content = BuildContent(properties, comment);
            using var writer = new StreamWriter(filePath, false, encoding);
            await writer.WriteAsync(content).ConfigureAwait(false);
        }

        /// <summary>
        /// 保存 Properties 到流
        /// </summary>
        /// <param name="stream">输出流</param>
        /// <param name="properties">Properties 字典</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="comment">注释</param>
        public static void SaveToStream(Stream stream, Dictionary<string, string> properties, Encoding? encoding = null, string? comment = null)
        {
            encoding ??= Encoding.UTF8;
            var content = BuildContent(properties, comment);
            using var writer = new StreamWriter(stream, encoding);
            writer.Write(content);
        }

        /// <summary>
        /// 将 Properties 转换为字符串
        /// </summary>
        /// <param name="properties">Properties 字典</param>
        /// <param name="comment">注释</param>
        /// <returns>Properties 格式字符串</returns>
        public static string ToString(Dictionary<string, string> properties, string? comment = null)
        {
            return BuildContent(properties, comment);
        }

        private static string BuildContent(Dictionary<string, string> properties, string? comment)
        {
            var sb = new StringBuilder();

            // 添加注释
            if (!string.IsNullOrEmpty(comment))
            {
                sb.AppendLine("# " + comment.Replace("\n", "\n# "));
                sb.AppendLine();
            }

            // 添加时间戳
            sb.AppendLine($"# {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            foreach (var kvp in properties)
            {
                sb.AppendLine($"{EscapeKey(kvp.Key)}={EscapeValue(kvp.Value)}");
            }

            return sb.ToString();
        }

        #endregion

        #region 单值操作

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>属性值</returns>
        public static string Get(string filePath, string key, string defaultValue = "", Encoding? encoding = null)
        {
            var properties = Load(filePath, encoding);
            return properties.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="encoding">编码方式</param>
        public static void Set(string filePath, string key, string value, Encoding? encoding = null)
        {
            var properties = File.Exists(filePath) ? Load(filePath, encoding) : new Dictionary<string, string>();
            properties[key] = value;
            Save(filePath, properties, encoding);
        }

        /// <summary>
        /// 删除属性
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="key">键</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>是否删除成功</returns>
        public static bool Remove(string filePath, string key, Encoding? encoding = null)
        {
            var properties = Load(filePath, encoding);
            if (properties.Remove(key))
            {
                Save(filePath, properties, encoding);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查属性是否存在
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="key">键</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>是否存在</returns>
        public static bool ContainsKey(string filePath, string key, Encoding? encoding = null)
        {
            var properties = Load(filePath, encoding);
            return properties.ContainsKey(key);
        }

        #endregion

        #region 类型转换获取

        /// <summary>
        /// 获取整数值
        /// </summary>
        public static int GetInt(string filePath, string key, int defaultValue = 0, Encoding? encoding = null)
        {
            var value = Get(filePath, key, null, encoding);
            if (value == null || !int.TryParse(value, out var result))
                return defaultValue;
            return result;
        }

        /// <summary>
        /// 获取长整数值
        /// </summary>
        public static long GetLong(string filePath, string key, long defaultValue = 0, Encoding? encoding = null)
        {
            var value = Get(filePath, key, null, encoding);
            if (value == null || !long.TryParse(value, out var result))
                return defaultValue;
            return result;
        }

        /// <summary>
        /// 获取双精度浮点值
        /// </summary>
        public static double GetDouble(string filePath, string key, double defaultValue = 0, Encoding? encoding = null)
        {
            var value = Get(filePath, key, null, encoding);
            if (value == null || !double.TryParse(value, out var result))
                return defaultValue;
            return result;
        }

        /// <summary>
        /// 获取布尔值
        /// </summary>
        public static bool GetBool(string filePath, string key, bool defaultValue = false, Encoding? encoding = null)
        {
            var value = Get(filePath, key, null, encoding);
            if (value == null)
                return defaultValue;

            return value.ToLower() switch
            {
                "true" or "yes" or "1" or "on" => true,
                "false" or "no" or "0" or "off" => false,
                _ => defaultValue
            };
        }

        /// <summary>
        /// 获取日期时间值
        /// </summary>
        public static DateTime GetDateTime(string filePath, string key, DateTime defaultValue = default, Encoding? encoding = null)
        {
            var value = Get(filePath, key, null, encoding);
            if (value == null || !DateTime.TryParse(value, out var result))
                return defaultValue;
            return result;
        }

        /// <summary>
        /// 获取枚举值
        /// </summary>
        public static T GetEnum<T>(string filePath, string key, T defaultValue = default, Encoding? encoding = null) where T : struct, Enum
        {
            var value = Get(filePath, key, null, encoding);
            if (value == null || !Enum.TryParse<T>(value, true, out var result))
                return defaultValue;
            return result;
        }

        /// <summary>
        /// 获取字符串列表（逗号分隔）
        /// </summary>
        public static List<string> GetList(string filePath, string key, Encoding? encoding = null)
        {
            var value = Get(filePath, key, "", encoding);
            if (string.IsNullOrEmpty(value))
                return new List<string>();

            return value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
        }

        #endregion

        #region 转义处理

        private static string EscapeKey(string key)
        {
            var sb = new StringBuilder();
            foreach (char c in key)
            {
                switch (c)
                {
                    case '=': sb.Append("\\="); break;
                    case ':': sb.Append("\\:"); break;
                    case ' ': sb.Append("\\ "); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\\': sb.Append("\\\\"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        private static string EscapeValue(string value)
        {
            var sb = new StringBuilder();
            foreach (char c in value)
            {
                switch (c)
                {
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\\': sb.Append("\\\\"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        private static string UnescapeKey(string key)
        {
            return Unescape(key);
        }

        private static string UnescapeValue(string value)
        {
            return Unescape(value);
        }

        private static string Unescape(string s)
        {
            var sb = new StringBuilder();
            bool escape = false;

            foreach (char c in s)
            {
                if (escape)
                {
                    switch (c)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '\\': sb.Append('\\'); break;
                        case '=': sb.Append('='); break;
                        case ':': sb.Append(':'); break;
                        case ' ': sb.Append(' '); break;
                        default: sb.Append(c); break;
                    }
                    escape = false;
                }
                else if (c == '\\')
                {
                    escape = true;
                }
                else
                {
                    sb.Append(c);
                }
            }

            // 处理结尾的转义符
            if (escape)
                sb.Append('\\');

            return sb.ToString();
        }

        #endregion

        #region PropertiesDocument 类

        /// <summary>
        /// 创建可操作的 Properties 文档对象
        /// </summary>
        /// <param name="filePath">文件路径（可选）</param>
        /// <returns>PropertiesDocument 对象</returns>
        public static PropertiesDocument CreateDocument(string? filePath = null)
        {
            if (filePath != null && File.Exists(filePath))
            {
                var properties = Load(filePath);
                return new PropertiesDocument(filePath, properties);
            }
            return new PropertiesDocument(filePath, new Dictionary<string, string>());
        }

        #endregion
    }

    /// <summary>
    /// 可操作的 Properties 文档对象
    /// </summary>
    public class PropertiesDocument
    {
        private readonly string? _filePath;
        private readonly Dictionary<string, string> _properties;
        private readonly List<string> _comments;
        private bool _modified;

        /// <summary>
        /// 属性数量
        /// </summary>
        public int Count => _properties.Count;

        /// <summary>
        /// 是否已修改
        /// </summary>
        public bool IsModified => _modified;

        /// <summary>
        /// 所有键
        /// </summary>
        public IEnumerable<string> Keys => _properties.Keys;

        /// <summary>
        /// 所有值
        /// </summary>
        public IEnumerable<string> Values => _properties.Values;

        /// <summary>
        /// 获取或设置属性值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public string this[string key]
        {
            get => _properties.TryGetValue(key, out var value) ? value : string.Empty;
            set
            {
                _properties[key] = value;
                _modified = true;
            }
        }

        internal PropertiesDocument(string? filePath, Dictionary<string, string> properties)
        {
            _filePath = filePath;
            _properties = properties;
            _comments = new List<string>();
            _modified = false;
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public string Get(string key, string defaultValue = "")
        {
            return _properties.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        public void Set(string key, string value)
        {
            _properties[key] = value;
            _modified = true;
        }

        /// <summary>
        /// 移除属性
        /// </summary>
        public bool Remove(string key)
        {
            if (_properties.Remove(key))
            {
                _modified = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否包含键
        /// </summary>
        public bool ContainsKey(string key)
        {
            return _properties.ContainsKey(key);
        }

        /// <summary>
        /// 添加注释
        /// </summary>
        public void AddComment(string comment)
        {
            _comments.Add(comment);
        }

        /// <summary>
        /// 保存到原文件
        /// </summary>
        public void Save()
        {
            if (_filePath == null)
                throw new InvalidOperationException("未指定文件路径");

            PropertiesUtil.Save(_filePath, _properties, null, string.Join("\n", _comments));
            _modified = false;
        }

        /// <summary>
        /// 保存到指定文件
        /// </summary>
        public void Save(string filePath)
        {
            PropertiesUtil.Save(filePath, _properties, null, string.Join("\n", _comments));
            _modified = false;
        }

        /// <summary>
        /// 重新加载文件
        /// </summary>
        public void Reload()
        {
            if (_filePath == null || !File.Exists(_filePath))
                return;

            var newProperties = PropertiesUtil.Load(_filePath);
            _properties.Clear();
            foreach (var kvp in newProperties)
            {
                _properties[kvp.Key] = kvp.Value;
            }
            _modified = false;
        }

        /// <summary>
        /// 转换为字典
        /// </summary>
        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(_properties);
        }

        /// <summary>
        /// 批量设置属性
        /// </summary>
        public void SetRange(Dictionary<string, string> properties)
        {
            foreach (var kvp in properties)
            {
                _properties[kvp.Key] = kvp.Value;
            }
            _modified = true;
        }
    }
}
