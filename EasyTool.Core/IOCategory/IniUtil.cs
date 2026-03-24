using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// INI 文件工具类
    /// 提供 INI 配置文件的读写功能
    /// </summary>
    public static class IniUtil
    {
        /// <summary>
        /// 读取 INI 文件
        /// </summary>
        public static IniFile Read(string filePath)
        {
            var ini = new IniFile();
            ini.Load(filePath);
            return ini;
        }

        /// <summary>
        /// 读取 INI 文件中的值
        /// </summary>
        public static string GetValue(string filePath, string section, string key, string defaultValue = "")
        {
            var ini = Read(filePath);
            return ini.GetValue(section, key, defaultValue);
        }

        /// <summary>
        /// 写入值到 INI 文件
        /// </summary>
        public static void SetValue(string filePath, string section, string key, string value)
        {
            var ini = Read(filePath);
            ini.SetValue(section, key, value);
            ini.Save(filePath);
        }

        /// <summary>
        /// 创建空的 INI 文件对象
        /// </summary>
        public static IniFile Create()
        {
            return new IniFile();
        }
    }

    /// <summary>
    /// INI 文件对象
    /// </summary>
    public class IniFile
    {
        private readonly Dictionary<string, Dictionary<string, string>> _sections;
        private readonly List<string> _sectionOrder;
        private string _commentPrefix = ";";

        /// <summary>
        /// 注释前缀
        /// </summary>
        public string CommentPrefix
        {
            get => _commentPrefix;
            set => _commentPrefix = value ?? ";";
        }

        /// <summary>
        /// 节名称列表
        /// </summary>
        public IEnumerable<string> Sections => _sectionOrder;

        /// <summary>
        /// 创建 INI 文件对象
        /// </summary>
        public IniFile()
        {
            _sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            _sectionOrder = new List<string>();
        }

        /// <summary>
        /// 从文件加载
        /// </summary>
        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            string currentSection = "";

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                // 跳过空行和注释
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(_commentPrefix) || trimmed.StartsWith("#"))
                    continue;

                // 节
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim();
                    if (!_sections.ContainsKey(currentSection))
                    {
                        _sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        _sectionOrder.Add(currentSection);
                    }
                    continue;
                }

                // 键值对
                int equalIndex = trimmed.IndexOf('=');
                if (equalIndex > 0)
                {
                    string key = trimmed.Substring(0, equalIndex).Trim();
                    string value = trimmed.Substring(equalIndex + 1).Trim();

                    // 移除行内注释
                    int commentIndex = value.IndexOf(_commentPrefix);
                    if (commentIndex >= 0)
                    {
                        value = value.Substring(0, commentIndex).Trim();
                    }

                    // 处理引号包裹的值
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    if (!_sections.ContainsKey(currentSection))
                    {
                        _sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        if (!string.IsNullOrEmpty(currentSection))
                            _sectionOrder.Add(currentSection);
                    }

                    _sections[currentSection][key] = value;
                }
            }
        }

        /// <summary>
        /// 保存到文件
        /// </summary>
        public void Save(string filePath)
        {
            var sb = new StringBuilder();

            // 先写入空节的值
            if (_sections.TryGetValue("", out var globalSection))
            {
                foreach (var kvp in globalSection)
                {
                    sb.AppendLine($"{kvp.Key}={FormatValue(kvp.Value)}");
                }
                sb.AppendLine();
            }

            // 写入各节
            foreach (var section in _sectionOrder)
            {
                if (string.IsNullOrEmpty(section)) continue;

                sb.AppendLine($"[{section}]");
                if (_sections.TryGetValue(section, out var sectionData))
                {
                    foreach (var kvp in sectionData)
                    {
                        sb.AppendLine($"{kvp.Key}={FormatValue(kvp.Value)}");
                    }
                }
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string FormatValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(";") || value.Contains("#") || value.Contains(" "))
                return $"\"{value}\"";
            return value;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public string GetValue(string section, string key, string defaultValue = "")
        {
            if (_sections.TryGetValue(section, out var sectionData))
            {
                if (sectionData.TryGetValue(key, out var value))
                    return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取值并转换为指定类型
        /// </summary>
        public T GetValue<T>(string section, string key, T defaultValue = default)
        {
            string value = GetValue(section, key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            try
            {
                var type = typeof(T);
                if (type == typeof(string))
                    return (T)(object)value;
                if (type == typeof(int))
                    return (T)(object)int.Parse(value);
                if (type == typeof(long))
                    return (T)(object)long.Parse(value);
                if (type == typeof(double))
                    return (T)(object)double.Parse(value);
                if (type == typeof(bool))
                    return (T)(object)ParseBool(value);
                if (type == typeof(DateTime))
                    return (T)(object)DateTime.Parse(value);

                return (T)Convert.ChangeType(value, type);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static bool ParseBool(string value)
        {
            return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("on", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 设置值
        /// </summary>
        public void SetValue(string section, string key, string value)
        {
            if (!_sections.TryGetValue(section, out var sectionData))
            {
                sectionData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _sections[section] = sectionData;
                if (!string.IsNullOrEmpty(section) && !_sectionOrder.Contains(section))
                    _sectionOrder.Add(section);
            }

            sectionData[key] = value;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        public void SetValue<T>(string section, string key, T value)
        {
            SetValue(section, key, value?.ToString());
        }

        /// <summary>
        /// 删除键
        /// </summary>
        public bool DeleteKey(string section, string key)
        {
            if (_sections.TryGetValue(section, out var sectionData))
            {
                return sectionData.Remove(key);
            }
            return false;
        }

        /// <summary>
        /// 删除节
        /// </summary>
        public bool DeleteSection(string section)
        {
            _sectionOrder.Remove(section);
            return _sections.Remove(section);
        }

        /// <summary>
        /// 获取节中的所有键值对
        /// </summary>
        public Dictionary<string, string> GetSection(string section)
        {
            if (_sections.TryGetValue(section, out var sectionData))
            {
                return new Dictionary<string, string>(sectionData);
            }
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// 节是否存在
        /// </summary>
        public bool HasSection(string section)
        {
            return _sections.ContainsKey(section);
        }

        /// <summary>
        /// 键是否存在
        /// </summary>
        public bool HasKey(string section, string key)
        {
            return _sections.TryGetValue(section, out var sectionData) && sectionData.ContainsKey(key);
        }

        /// <summary>
        /// 清空所有内容
        /// </summary>
        public void Clear()
        {
            _sections.Clear();
            _sectionOrder.Clear();
        }
    }
}
