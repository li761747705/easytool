using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 敏感词过滤工具类
    /// 使用 DFA（Deterministic Finite Automaton）算法实现高效敏感词检测
    /// </summary>
    public static class SensitiveWordUtil
    {
        private static readonly object _lock = new();
        private static Dictionary<char, object> _sensitiveWordsMap = new();
        private static HashSet<string> _sensitiveWords = new();
        private static char[] _separatorChars = { ',', '，', '\n', '\r', ';' };

        #region 初始化

        /// <summary>
        /// 初始化敏感词库
        /// </summary>
        /// <param name="words">敏感词列表</param>
        public static void Init(IEnumerable<string> words)
        {
            if (words == null)
                return;

            lock (_lock)
            {
                _sensitiveWords = new HashSet<string>(words.Where(w => !string.IsNullOrWhiteSpace(w)));
                _sensitiveWordsMap = BuildDFA(_sensitiveWords);
            }
        }

        /// <summary>
        /// 从文件初始化敏感词库
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码（默认UTF-8）</param>
        public static void InitFromFile(string filePath, Encoding? encoding = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"敏感词文件不存在: {filePath}");

            encoding ??= Encoding.UTF8;
            var content = File.ReadAllText(filePath, encoding);
            var words = content.Split(_separatorChars, StringSplitOptions.RemoveEmptyEntries);
            Init(words);
        }

        /// <summary>
        /// 添加敏感词
        /// </summary>
        /// <param name="word">敏感词</param>
        public static void AddWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;

            lock (_lock)
            {
                _sensitiveWords.Add(word);
                _sensitiveWordsMap = BuildDFA(_sensitiveWords);
            }
        }

        /// <summary>
        /// 批量添加敏感词
        /// </summary>
        /// <param name="words">敏感词列表</param>
        public static void AddWords(IEnumerable<string> words)
        {
            if (words == null)
                return;

            lock (_lock)
            {
                foreach (var word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word))
                        _sensitiveWords.Add(word);
                }
                _sensitiveWordsMap = BuildDFA(_sensitiveWords);
            }
        }

        /// <summary>
        /// 移除敏感词
        /// </summary>
        /// <param name="word">敏感词</param>
        public static void RemoveWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return;

            lock (_lock)
            {
                _sensitiveWords.Remove(word);
                _sensitiveWordsMap = BuildDFA(_sensitiveWords);
            }
        }

        /// <summary>
        /// 清空敏感词库
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _sensitiveWords.Clear();
                _sensitiveWordsMap.Clear();
            }
        }

        /// <summary>
        /// 获取敏感词数量
        /// </summary>
        public static int Count => _sensitiveWords.Count;

        #endregion

        #region DFA构建

        private static Dictionary<char, object> BuildDFA(HashSet<string> words)
        {
            var map = new Dictionary<char, object>();

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word))
                    continue;

                var currentMap = map;
                for (int i = 0; i < word.Length; i++)
                {
                    var c = word[i];

                    if (!currentMap.TryGetValue(c, out var value))
                    {
                        value = new Dictionary<char, object>();
                        currentMap[c] = value;
                    }

                    var childMap = (Dictionary<char, object>)value;

                    if (i == word.Length - 1)
                    {
                        childMap['\0'] = new Dictionary<char, object>(); // 标记词尾
                    }
                    else
                    {
                        currentMap = childMap;
                    }
                }
            }

            return map;
        }

        #endregion

        #region 检测

        /// <summary>
        /// 检测文本是否包含敏感词
        /// </summary>
        /// <param name="text">待检测文本</param>
        /// <returns>是否包含敏感词</returns>
        public static bool Contains(string text)
        {
            if (string.IsNullOrEmpty(text) || _sensitiveWordsMap.Count == 0)
                return false;

            for (int i = 0; i < text.Length; i++)
            {
                if (CheckSensitiveWord(text, i, out _))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取文本中的所有敏感词
        /// </summary>
        /// <param name="text">待检测文本</param>
        /// <returns>敏感词列表</returns>
        public static List<string> FindAll(string text)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(text) || _sensitiveWordsMap.Count == 0)
                return result;

            for (int i = 0; i < text.Length; i++)
            {
                if (CheckSensitiveWord(text, i, out int length))
                {
                    result.Add(text.Substring(i, length));
                    i += length - 1;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取文本中敏感词的位置信息
        /// </summary>
        /// <param name="text">待检测文本</param>
        /// <returns>敏感词位置列表（起始位置, 敏感词）</returns>
        public static List<(int StartIndex, string Word)> FindAllWithPosition(string text)
        {
            var result = new List<(int, string)>();

            if (string.IsNullOrEmpty(text) || _sensitiveWordsMap.Count == 0)
                return result;

            for (int i = 0; i < text.Length; i++)
            {
                if (CheckSensitiveWord(text, i, out int length))
                {
                    result.Add((i, text.Substring(i, length)));
                    i += length - 1;
                }
            }

            return result;
        }

        /// <summary>
        /// 统计文本中敏感词出现次数
        /// </summary>
        /// <param name="text">待检测文本</param>
        /// <returns>敏感词及其出现次数</returns>
        public static Dictionary<string, int> CountWords(string text)
        {
            var result = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(text) || _sensitiveWordsMap.Count == 0)
                return result;

            foreach (var word in FindAll(text))
            {
                if (result.ContainsKey(word))
                    result[word]++;
                else
                    result[word] = 1;
            }

            return result;
        }

        private static bool CheckSensitiveWord(string text, int beginIndex, out int length)
        {
            length = 0;
            var currentMap = _sensitiveWordsMap;
            bool found = false;

            for (int i = beginIndex; i < text.Length; i++)
            {
                var c = text[i];

                if (!currentMap.TryGetValue(c, out var value))
                {
                    break;
                }

                length++;
                currentMap = (Dictionary<char, object>)value;

                if (currentMap.ContainsKey('\0'))
                {
                    found = true;
                }
            }

            return found && length > 0;
        }

        #endregion

        #region 过滤

        /// <summary>
        /// 过滤敏感词（替换为指定字符）
        /// </summary>
        /// <param name="text">待过滤文本</param>
        /// <param name="replaceChar">替换字符（默认 *）</param>
        /// <returns>过滤后的文本</returns>
        public static string Filter(string text, char replaceChar = '*')
        {
            if (string.IsNullOrEmpty(text) || _sensitiveWordsMap.Count == 0)
                return text ?? string.Empty;

            var result = new StringBuilder(text);

            for (int i = 0; i < result.Length; i++)
            {
                if (CheckSensitiveWord(result.ToString(), i, out int length))
                {
                    for (int j = i; j < i + length && j < result.Length; j++)
                    {
                        result[j] = replaceChar;
                    }
                    i += length - 1;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 过滤敏感词（使用自定义替换策略）
        /// </summary>
        /// <param name="text">待过滤文本</param>
        /// <param name="replacer">替换函数（参数为敏感词，返回替换后的文本）</param>
        /// <returns>过滤后的文本</returns>
        public static string Filter(string text, Func<string, string> replacer)
        {
            if (string.IsNullOrEmpty(text) || _sensitiveWordsMap.Count == 0 || replacer == null)
                return text ?? string.Empty;

            var positions = FindAllWithPosition(text);
            if (positions.Count == 0)
                return text;

            var result = new StringBuilder();
            int lastIndex = 0;

            foreach (var (startIndex, word) in positions)
            {
                result.Append(text.Substring(lastIndex, startIndex - lastIndex));
                result.Append(replacer(word));
                lastIndex = startIndex + word.Length;
            }

            if (lastIndex < text.Length)
            {
                result.Append(text.Substring(lastIndex));
            }

            return result.ToString();
        }

        /// <summary>
        /// 高亮显示敏感词
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="prefix">高亮前缀（如 &lt;span style=\"color:red\"&gt;）</param>
        /// <param name="suffix">高亮后缀（如 &lt;/span&gt;）</param>
        /// <returns>处理后的文本</returns>
        public static string Highlight(string text, string prefix = "<em>", string suffix = "</em>")
        {
            return Filter(text, word => $"{prefix}{word}{suffix}");
        }

        #endregion
    }
}
