using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 配置文件工具类
    /// 支持INI格式配置文件
    /// </summary>
    public static class ConfigUtil
    {
        /// <summary>
        /// 读取INI配置值
        /// </summary>
        public static string? GetIniValue(string filePath, string section, string key)
        {
            if (!File.Exists(filePath))
                return null;

            var lines = File.ReadAllLines(filePath);
            var currentSection = "";
            var sectionHeader = $"[{section}]";

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed;
                    continue;
                }

                if (currentSection == sectionHeader)
                {
                    if (trimmed.StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.StartsWith($"{key} =", StringComparison.OrdinalIgnoreCase))
                    {
                        var valueStart = trimmed.IndexOf('=') + 1;
                        return trimmed.Substring(valueStart).Trim();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 设置INI配置值
        /// </summary>
        public static void SetIniValue(string filePath, string section, string key, string value)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var lines = File.Exists(filePath) ? File.ReadAllLines(filePath).ToList() : new List<string>();
            var sectionHeader = $"[{section}]";
            var sectionIndex = -1;
            var keyIndex = -1;

            // 查找section
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim() == sectionHeader)
                {
                    sectionIndex = i;
                    break;
                }
            }

            // 如果section不存在，添加它
            if (sectionIndex < 0)
            {
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                    lines.Add("");
                lines.Add(sectionHeader);
                lines.Add($"{key}={value}");
            }
            else
            {
                // 查找key
                for (int i = sectionIndex + 1; i < lines.Count; i++)
                {
                    var line = lines[i].Trim();
                    if (line.StartsWith("[") && line.EndsWith("]"))
                        break; // 进入下一个section

                    if (line.StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith($"{key} =", StringComparison.OrdinalIgnoreCase))
                    {
                        keyIndex = i;
                        break;
                    }
                }

                if (keyIndex >= 0)
                {
                    lines[keyIndex] = $"{key}={value}";
                }
                else
                {
                    lines.Insert(sectionIndex + 1, $"{key}={value}");
                }
            }

            File.WriteAllLines(filePath, lines);
        }

        /// <summary>
        /// 读取INI配置的所有键值对
        /// </summary>
        public static Dictionary<string, string> GetIniSection(string filePath, string section)
        {
            var result = new Dictionary<string, string>();

            if (!File.Exists(filePath))
                return result;

            var lines = File.ReadAllLines(filePath);
            var currentSection = "";
            var sectionHeader = $"[{section}]";

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed;
                    continue;
                }

                if (currentSection == sectionHeader && trimmed.Contains("="))
                {
                    var eqIndex = trimmed.IndexOf('=');
                    var key = trimmed.Substring(0, eqIndex).Trim();
                    var value = trimmed.Substring(eqIndex + 1).Trim();
                    result[key] = value;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取INI文件所有节名
        /// </summary>
        public static List<string> GetIniSections(string filePath)
        {
            var sections = new List<string>();

            if (!File.Exists(filePath))
                return sections;

            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    var sectionName = trimmed.Substring(1, trimmed.Length - 2);
                    sections.Add(sectionName);
                }
            }

            return sections;
        }

        /// <summary>
        /// 删除INI键
        /// </summary>
        public static void RemoveIniKey(string filePath, string section, string key)
        {
            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath).ToList();
            var sectionHeader = $"[{section}]";
            var inSection = false;
            var keyIndex = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();

                if (line == sectionHeader)
                {
                    inSection = true;
                    continue;
                }

                if (inSection)
                {
                    if (line.StartsWith("[") && line.EndsWith("]"))
                        break;

                    if (line.StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith($"{key} =", StringComparison.OrdinalIgnoreCase))
                    {
                        keyIndex = i;
                        break;
                    }
                }
            }

            if (keyIndex >= 0)
            {
                lines.RemoveAt(keyIndex);
                File.WriteAllLines(filePath, lines);
            }
        }

        /// <summary>
        /// 删除INI节
        /// </summary>
        public static void RemoveIniSection(string filePath, string section)
        {
            if (!File.Exists(filePath))
                return;

            var lines = File.ReadAllLines(filePath).ToList();
            var sectionHeader = $"[{section}]";
            var startIndex = -1;
            var endIndex = lines.Count;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();

                if (line == sectionHeader)
                {
                    startIndex = i;
                }
                else if (startIndex >= 0 && line.StartsWith("[") && line.EndsWith("]"))
                {
                    endIndex = i;
                    break;
                }
            }

            if (startIndex >= 0)
            {
                lines.RemoveRange(startIndex, endIndex - startIndex);
                File.WriteAllLines(filePath, lines);
            }
        }
    }
}