using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 敏感词过滤工具类
    /// 使用 DFA（Deterministic Finite Automaton）算法实现高效敏感词检测
    /// </summary>
    /// <remarks>
    /// 线程安全：是。使用 ReaderWriterLockSlim 保护并发读写。
    /// 读操作（Contains、FindAll、Filter 等）可并发执行，写操作（Init、AddWord 等）互斥。
    /// 时间复杂度：O(n)，n 为文本长度。
    /// </remarks>
    public static class SensitiveWordUtil
    {
        private static readonly ReaderWriterLockSlim _rwLock = new();
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

            _rwLock.EnterWriteLock();
            try
            {
                _sensitiveWords = new HashSet<string>(words.Where(w => !string.IsNullOrWhiteSpace(w)));
                _sensitiveWordsMap = BuildDFA(_sensitiveWords);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 从文件初始化敏感词库
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码（默认UTF-8）</param>
        /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
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

            _rwLock.EnterWriteLock();
            try
            {
                _sensitiveWords.Add(word);
                _sensitiveWordsMap = BuildDFA(_sensitiveWords);
            }
            finally
            {
                _rwLock.ExitWriteLock();
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

            _rwLock.EnterWriteLock();
            try
            {
                foreach (var word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word))
                        _sensitiveWords.Add(word);
                }
                _sensitiveWordsMap = BuildDFA(_sensitiveWords);
            }
            finally
            {
                _rwLock.ExitWriteLock();
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

            _rwLock.EnterWriteLock();
            try
            {
                _sensitiveWords.Remove(word);
                _sensitiveWordsMap = BuildDFA(_sensitiveWords);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 清空敏感词库
        /// </summary>
        public static void Clear()
        {
            _rwLock.EnterWriteLock();
            try
            {
                _sensitiveWords.Clear();
                _sensitiveWordsMap.Clear();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 获取敏感词数量
        /// </summary>
        public static int Count
        {
            get
            {
                _rwLock.EnterReadLock();
                try
                {
                    return _sensitiveWords.Count;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
        }

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
            if (string.IsNullOrEmpty(text))
                return false;

            _rwLock.EnterReadLock();
            try
            {
                if (_sensitiveWordsMap.Count == 0)
                    return false;

                var snapshot = _sensitiveWordsMap;
                for (int i = 0; i < text.Length; i++)
                {
                    if (CheckSensitiveWord(snapshot, text, i, out _))
                    {
                        return true;
                    }
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
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

            if (string.IsNullOrEmpty(text))
                return result;

            _rwLock.EnterReadLock();
            try
            {
                if (_sensitiveWordsMap.Count == 0)
                    return result;

                var snapshot = _sensitiveWordsMap;
                for (int i = 0; i < text.Length; i++)
                {
                    if (CheckSensitiveWord(snapshot, text, i, out int length))
                    {
                        result.Add(text.Substring(i, length));
                        i += length - 1;
                    }
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
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

            if (string.IsNullOrEmpty(text))
                return result;

            _rwLock.EnterReadLock();
            try
            {
                if (_sensitiveWordsMap.Count == 0)
                    return result;

                var snapshot = _sensitiveWordsMap;
                for (int i = 0; i < text.Length; i++)
                {
                    if (CheckSensitiveWord(snapshot, text, i, out int length))
                    {
                        result.Add((i, text.Substring(i, length)));
                        i += length - 1;
                    }
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
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

            if (string.IsNullOrEmpty(text))
                return result;

            _rwLock.EnterReadLock();
            try
            {
                if (_sensitiveWordsMap.Count == 0)
                    return result;

                var snapshot = _sensitiveWordsMap;
                for (int i = 0; i < text.Length; i++)
                {
                    if (CheckSensitiveWord(snapshot, text, i, out int length))
                    {
                        var word = text.Substring(i, length);
                        if (result.ContainsKey(word))
                            result[word]++;
                        else
                            result[word] = 1;
                        i += length - 1;
                    }
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            return result;
        }

        /// <summary>
        /// 检查敏感词（使用指定的 DFA 快照，保证线程安全）
        /// </summary>
        private static bool CheckSensitiveWord(Dictionary<char, object> dfaMap, string text, int beginIndex, out int length)
        {
            length = 0;
            var currentMap = dfaMap;
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
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;

            _rwLock.EnterReadLock();
            try
            {
                if (_sensitiveWordsMap.Count == 0)
                    return text;

                var snapshot = _sensitiveWordsMap;
                var result = new StringBuilder(text);

                for (int i = 0; i < result.Length; i++)
                {
                    // 使用原始文本进行 DFA 查找，避免循环内 ToString() 的 O(n²) 开销
                    if (CheckSensitiveWord(snapshot, text, i, out int length))
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
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 过滤敏感词（使用自定义替换策略）
        /// </summary>
        /// <param name="text">待过滤文本</param>
        /// <param name="replacer">替换函数（参数为敏感词，返回替换后的文本）</param>
        /// <returns>过滤后的文本</returns>
        /// <remarks>replacer 为 null 时返回原始文本</remarks>
        public static string Filter(string text, Func<string, string> replacer)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;
            if (replacer == null)
                return text;

            _rwLock.EnterReadLock();
            try
            {
                if (_sensitiveWordsMap.Count == 0)
                    return text;

                var snapshot = _sensitiveWordsMap;
                var positions = new List<(int StartIndex, int Length)>();

                for (int i = 0; i < text.Length; i++)
                {
                    if (CheckSensitiveWord(snapshot, text, i, out int length))
                    {
                        positions.Add((i, length));
                        i += length - 1;
                    }
                }

                if (positions.Count == 0)
                    return text;

                var result = new StringBuilder();
                int lastIndex = 0;

                foreach (var (startIndex, len) in positions)
                {
                    result.Append(text.Substring(lastIndex, startIndex - lastIndex));
                    result.Append(replacer(text.Substring(startIndex, len)));
                    lastIndex = startIndex + len;
                }

                if (lastIndex < text.Length)
                {
                    result.Append(text.Substring(lastIndex));
                }

                return result.ToString();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 高亮显示敏感词
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="prefix">高亮前缀（如 &lt;span style="color:red"&gt;）</param>
        /// <param name="suffix">高亮后缀（如 &lt;/span&gt;）</param>
        /// <returns>处理后的文本</returns>
        public static string Highlight(string text, string prefix = "<em>", string suffix = "</em>")
        {
            return Filter(text, word => $"{prefix}{word}{suffix}");
        }

        #endregion
    }
}
