using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 关键词提取工具类
    /// </summary>
    public static class KeywordExtractor
    {
        /// <summary>
        /// 中文停用词
        /// </summary>
        private static readonly HashSet<string> ChineseStopWords = new()
        {
            "的", "了", "在", "是", "我", "有", "和", "就", "不", "人", "都", "一", "一个",
            "上", "也", "很", "到", "说", "要", "去", "你", "会", "着", "没有", "看", "好",
            "自己", "这", "那", "什么", "他", "她", "它", "们", "这个", "那个", "哪个",
            "怎么", "为什么", "因为", "所以", "但是", "然后", "如果", "可以", "可能",
            "已经", "还是", "只是", "就是", "这样", "那样", "怎样", "这么", "那么",
            "更", "最", "比", "而", "且", "或", "与", "及", "等", "等等", "之", "于",
            "以", "为", "让", "把", "被", "从", "向", "对", "给", "跟", "像", "关于",
            "通过", "按照", "根据", "由于", "为了", "既然", "无论", "不管", "即使",
            "虽然", "即使", "哪怕", "只要", "除非", "假如", "倘若", "若是", "要是"
        };

        /// <summary>
        /// 英文停用词
        /// </summary>
        private static readonly HashSet<string> EnglishStopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with",
            "by", "from", "as", "is", "was", "are", "were", "been", "be", "have", "has", "had",
            "do", "does", "did", "will", "would", "could", "should", "may", "might", "must",
            "shall", "can", "need", "dare", "ought", "used", "it", "its", "this", "that",
            "these", "those", "i", "you", "he", "she", "we", "they", "what", "which", "who",
            "whom", "whose", "where", "when", "why", "how", "all", "each", "every", "both",
            "few", "more", "most", "other", "some", "such", "no", "not", "only", "same", "so",
            "than", "too", "very", "just", "also", "now", "here", "there", "then", "once"
        };

        /// <summary>
        /// 使用TF-IDF算法提取关键词
        /// </summary>
        public static List<KeywordResult> ExtractByTfIdf(string text, int topN = 10, int minWordLength = 2)
        {
            // 分词（简单实现：按空格和标点分割）
            var words = Tokenize(text, minWordLength);

            // 计算词频
            var wordFreq = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (IsStopWord(word)) continue;
                if (!wordFreq.ContainsKey(word))
                    wordFreq[word] = 0;
                wordFreq[word]++;
            }

            // 计算TF-IDF（简化版，使用词频和词长作为权重）
            var results = new List<KeywordResult>();
            var totalWords = words.Count;

            foreach (var kvp in wordFreq)
            {
                var tf = (double)kvp.Value / totalWords;
                var wordLength = kvp.Key.Length;
                
                // 词长权重：较长的词可能更重要
                var lengthWeight = Math.Min(wordLength / 4.0, 1.0);
                
                // 简化的IDF：使用词的稀有度
                var idf = Math.Log((double)totalWords / kvp.Value + 1);
                
                var score = tf * idf * (1 + lengthWeight);

                results.Add(new KeywordResult
                {
                    Word = kvp.Key,
                    Frequency = kvp.Value,
                    Score = score
                });
            }

            return results.OrderByDescending(r => r.Score).Take(topN).ToList();
        }

        /// <summary>
        /// 提取高频词
        /// </summary>
        public static List<KeywordResult> ExtractTopWords(string text, int topN = 10, int minWordLength = 2)
        {
            var words = Tokenize(text, minWordLength);

            var wordFreq = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (IsStopWord(word)) continue;
                if (!wordFreq.ContainsKey(word))
                    wordFreq[word] = 0;
                wordFreq[word]++;
            }

            return wordFreq
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .Select(kvp => new KeywordResult
                {
                    Word = kvp.Key,
                    Frequency = kvp.Value,
                    Score = kvp.Value
                })
                .ToList();
        }

        /// <summary>
        /// 提取n-gram
        /// </summary>
        public static List<KeywordResult> ExtractNgrams(string text, int n = 2, int topN = 10)
        {
            var ngrams = new Dictionary<string, int>();
            var cleanText = Regex.Replace(text, @"[\s\p{P}]+", " ").Trim();

            for (int i = 0; i <= cleanText.Length - n; i++)
            {
                var ngram = cleanText.Substring(i, n);
                if (!ngrams.ContainsKey(ngram))
                    ngrams[ngram] = 0;
                ngrams[ngram]++;
            }

            return ngrams
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .Select(kvp => new KeywordResult
                {
                    Word = kvp.Key,
                    Frequency = kvp.Value,
                    Score = kvp.Value
                })
                .ToList();
        }

        /// <summary>
        /// 提取中文短语（双字词组）
        /// </summary>
        public static List<KeywordResult> ExtractChinesePhrases(string text, int topN = 10)
        {
            var phrases = new Dictionary<string, int>();
            var chinesePattern = new Regex(@"[\u4e00-\u9fa5]{2,}");

            foreach (Match match in chinesePattern.Matches(text))
            {
                var phrase = match.Value;
                if (!phrases.ContainsKey(phrase))
                    phrases[phrase] = 0;
                phrases[phrase]++;
            }

            // 过滤停用词
            var filtered = phrases
                .Where(kvp => !IsStopWord(kvp.Key))
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .Select(kvp => new KeywordResult
                {
                    Word = kvp.Key,
                    Frequency = kvp.Value,
                    Score = kvp.Value * kvp.Key.Length // 长词权重更高
                });

            return filtered.ToList();
        }

        /// <summary>
        /// 提取英文短语
        /// </summary>
        public static List<KeywordResult> ExtractEnglishPhrases(string text, int topN = 10)
        {
            var phrases = new Dictionary<string, int>();
            var wordPattern = new Regex(@"\b[a-zA-Z]{2,}\b");

            foreach (Match match in wordPattern.Matches(text))
            {
                var word = match.Value.ToLower();
                if (!IsStopWord(word))
                {
                    if (!phrases.ContainsKey(word))
                        phrases[word] = 0;
                    phrases[word]++;
                }
            }

            return phrases
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .Select(kvp => new KeywordResult
                {
                    Word = kvp.Key,
                    Frequency = kvp.Value,
                    Score = kvp.Value
                })
                .ToList();
        }

        /// <summary>
        /// 分词
        /// </summary>
        private static List<string> Tokenize(string text, int minWordLength = 2)
        {
            var words = new List<string>();

            // 提取中文词
            var chinesePattern = new Regex(@"[\u4e00-\u9fa5]+");
            foreach (Match match in chinesePattern.Matches(text))
            {
                var word = match.Value;
                // 中文简单分词：提取双字词
                for (int i = 0; i < word.Length - 1; i++)
                {
                    words.Add(word.Substring(i, 2));
                }
                if (word.Length >= minWordLength)
                {
                    words.Add(word);
                }
            }

            // 提取英文词
            var englishPattern = new Regex(@"\b[a-zA-Z]{2,}\b");
            foreach (Match match in englishPattern.Matches(text))
            {
                words.Add(match.Value.ToLower());
            }

            // 提取数字
            var numberPattern = new Regex(@"\b\d+(\.\d+)?\b");
            foreach (Match match in numberPattern.Matches(text))
            {
                words.Add(match.Value);
            }

            return words;
        }

        /// <summary>
        /// 判断是否为停用词
        /// </summary>
        private static bool IsStopWord(string word)
        {
            return ChineseStopWords.Contains(word) || EnglishStopWords.Contains(word);
        }

        /// <summary>
        /// 添加自定义停用词
        /// </summary>
        public static void AddStopWords(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                if (Regex.IsMatch(word, @"[\u4e00-\u9fa5]"))
                {
                    ChineseStopWords.Add(word);
                }
                else
                {
                    EnglishStopWords.Add(word.ToLower());
                }
            }
        }
    }

    /// <summary>
    /// 关键词结果
    /// </summary>
    public class KeywordResult
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string Word { get; set; } = "";

        /// <summary>
        /// 出现频率
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 权重分数
        /// </summary>
        public double Score { get; set; }

        public override string ToString()
        {
            return $"{Word} (频率:{Frequency}, 分数:{Score:F4})";
        }
    }
}
