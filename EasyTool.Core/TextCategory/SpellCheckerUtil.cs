using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 拼写检查器
    /// 提供英文拼写检查和纠错功能
    /// </summary>
    public static class SpellCheckerUtil
    {
        private static readonly HashSet<string> _dictionary = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly char[] _alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        static SpellCheckerUtil()
        {
            InitializeDictionary();
        }

        /// <summary>
        /// 检查单词拼写是否正确
        /// </summary>
        /// <param name="word">单词</param>
        /// <returns>是否正确</returns>
        public static bool IsCorrect(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return true;

            return _dictionary.Contains(word.Trim().ToLowerInvariant());
        }

        /// <summary>
        /// 获取拼写建议
        /// </summary>
        /// <param name="word">单词</param>
        /// <param name="maxSuggestions">最大建议数量</param>
        /// <returns>建议列表</returns>
        public static List<string> GetSuggestions(string word, int maxSuggestions = 5)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new List<string>();

            word = word.Trim().ToLowerInvariant();

            // 如果拼写正确，返回空列表
            if (_dictionary.Contains(word))
                return new List<string>();

            var candidates = new Dictionary<string, int>();

            // 编辑距离为1的候选词
            var edits1 = GetEdits1(word);
            foreach (var edit in edits1)
            {
                if (_dictionary.Contains(edit))
                {
                    candidates[edit] = 1;
                }
            }

            // 编辑距离为2的候选词（如果没有找到距离1的）
            if (candidates.Count == 0)
            {
                foreach (var edit1 in edits1)
                {
                    var edits2 = GetEdits1(edit1);
                    foreach (var edit2 in edits2)
                    {
                        if (_dictionary.Contains(edit2) && !candidates.ContainsKey(edit2))
                        {
                            candidates[edit2] = 2;
                        }
                    }
                }
            }

            return candidates
                .OrderBy(kvp => kvp.Value)
                .ThenBy(kvp => LevenshteinDistance(word, kvp.Key))
                .Take(maxSuggestions)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// 检查文本中的拼写错误
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>错误单词及其建议</returns>
        public static Dictionary<string, List<string>> CheckText(string text)
        {
            var result = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(text))
                return result;

            var words = ExtractWords(text);

            foreach (var word in words)
            {
                if (!IsCorrect(word) && !result.ContainsKey(word))
                {
                    result[word] = GetSuggestions(word);
                }
            }

            return result;
        }

        /// <summary>
        /// 自动纠正拼写错误
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>纠正后的文本</returns>
        public static string AutoCorrect(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var words = ExtractWords(text);
            var result = text;

            foreach (var word in words)
            {
                if (!IsCorrect(word))
                {
                    var suggestions = GetSuggestions(word, 1);
                    if (suggestions.Count > 0)
                    {
                        result = ReplaceWord(result, word, suggestions[0]);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 添加单词到词典
        /// </summary>
        /// <param name="words">单词列表</param>
        public static void AddToDictionary(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    _dictionary.Add(word.Trim().ToLowerInvariant());
                }
            }
        }

        /// <summary>
        /// 从文件加载词典
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void LoadDictionary(string filePath)
        {
            try
            {
                var lines = System.IO.File.ReadAllLines(filePath);
                AddToDictionary(lines);
            }
            catch (Exception)
            {
                // 忽略错误
            }
        }

        /// <summary>
        /// 获取词典大小
        /// </summary>
        /// <returns>词典单词数量</returns>
        public static int GetDictionarySize()
        {
            return _dictionary.Count;
        }

        #region 私有方法

        private static void InitializeDictionary()
        {
            // 常用英语单词
            var commonWords = new[]
            {
                "the", "be", "to", "of", "and", "a", "in", "that", "have", "i",
                "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
                "this", "but", "his", "by", "from", "they", "we", "say", "her", "she",
                "or", "an", "will", "my", "one", "all", "would", "there", "their", "what",
                "so", "up", "out", "if", "about", "who", "get", "which", "go", "me",
                "when", "make", "can", "like", "time", "no", "just", "him", "know", "take",
                "people", "into", "year", "your", "good", "some", "could", "them", "see", "other",
                "than", "then", "now", "look", "only", "come", "its", "over", "think", "also",
                "back", "after", "use", "two", "how", "our", "work", "first", "well", "way",
                "even", "new", "want", "because", "any", "these", "give", "day", "most", "us",
                "hello", "world", "computer", "program", "software", "hardware", "system", "network",
                "internet", "website", "application", "development", "design", "testing", "code",
                "data", "database", "server", "client", "user", "password", "email", "message",
                "file", "folder", "directory", "document", "image", "video", "audio", "music",
                "game", "play", "player", "team", "sport", "football", "basketball", "tennis",
                "school", "student", "teacher", "class", "lesson", "book", "read", "write",
                "learn", "study", "exam", "test", "question", "answer", "problem", "solution",
                "work", "job", "office", "company", "business", "money", "price", "cost",
                "buy", "sell", "shop", "store", "market", "product", "service", "customer",
                "food", "drink", "water", "coffee", "tea", "breakfast", "lunch", "dinner",
                "house", "home", "room", "door", "window", "bed", "table", "chair", "kitchen",
                "car", "bus", "train", "plane", "airport", "station", "road", "street", "city",
                "country", "world", "earth", "sun", "moon", "star", "sky", "weather", "rain",
                "love", "hate", "happy", "sad", "angry", "tired", "hungry", "thirsty", "sleep",
                "family", "mother", "father", "brother", "sister", "child", "baby", "friend",
                "health", "doctor", "hospital", "medicine", "sick", "healthy", "exercise",
                "phone", "call", "number", "address", "name", "age", "birthday", "date",
                "time", "hour", "minute", "second", "week", "month", "year", "today",
                "tomorrow", "yesterday", "morning", "afternoon", "evening", "night",
                "spring", "summer", "autumn", "winter", "hot", "cold", "warm", "cool",
                "big", "small", "large", "little", "long", "short", "high", "low",
                "fast", "slow", "quick", "easy", "hard", "simple", "complex", "different"
            };

            foreach (var word in commonWords)
            {
                _dictionary.Add(word.ToLowerInvariant());
            }
        }

        private static HashSet<string> GetEdits1(string word)
        {
            var edits = new HashSet<string>();

            // 删除
            for (int i = 0; i < word.Length; i++)
            {
                edits.Add(word.Substring(0, i) + word.Substring(i + 1));
            }

            // 交换
            for (int i = 0; i < word.Length - 1; i++)
            {
                edits.Add(word.Substring(0, i) + word[i + 1] + word[i] + word.Substring(i + 2));
            }

            // 替换
            for (int i = 0; i < word.Length; i++)
            {
                foreach (var c in _alphabet)
                {
                    edits.Add(word.Substring(0, i) + c + word.Substring(i + 1));
                }
            }

            // 插入
            for (int i = 0; i <= word.Length; i++)
            {
                foreach (var c in _alphabet)
                {
                    edits.Add(word.Substring(0, i) + c + word.Substring(i));
                }
            }

            return edits;
        }

        private static int LevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        private static List<string> ExtractWords(string text)
        {
            var words = new List<string>();
            var currentWord = new System.Text.StringBuilder();

            foreach (var c in text)
            {
                if (char.IsLetter(c))
                {
                    currentWord.Append(c);
                }
                else if (currentWord.Length > 0)
                {
                    words.Add(currentWord.ToString());
                    currentWord.Clear();
                }
            }

            if (currentWord.Length > 0)
            {
                words.Add(currentWord.ToString());
            }

            return words;
        }

        private static string ReplaceWord(string text, string oldWord, string newWord)
        {
            // 保持原始大小写
            var index = text.IndexOf(oldWord, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return text;

            var originalWord = text.Substring(index, oldWord.Length);

            // 调整新词的大小写
            string replacement;
            if (char.IsUpper(originalWord[0]))
            {
                replacement = char.ToUpper(newWord[0]) + newWord.Substring(1);
            }
            else
            {
                replacement = newWord;
            }

            return text.Substring(0, index) + replacement + text.Substring(index + oldWord.Length);
        }

        #endregion
    }
}
