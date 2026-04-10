using System;
using System.Collections.Concurrent;
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
        /// 中文停用词（使用 ConcurrentDictionary 保证线程安全）
        /// </summary>
        private static readonly ConcurrentDictionary<string, byte> ChineseStopWords = new()
        {
            ["的"] = 0, ["了"] = 0, ["在"] = 0, ["是"] = 0, ["我"] = 0, ["有"] = 0, ["和"] = 0,
            ["就"] = 0, ["不"] = 0, ["人"] = 0, ["都"] = 0, ["一"] = 0, ["一个"] = 0,
            ["上"] = 0, ["也"] = 0, ["很"] = 0, ["到"] = 0, ["说"] = 0, ["要"] = 0,
            ["去"] = 0, ["你"] = 0, ["会"] = 0, ["着"] = 0, ["没有"] = 0, ["看"] = 0, ["好"] = 0,
            ["自己"] = 0, ["这"] = 0, ["那"] = 0, ["什么"] = 0, ["他"] = 0, ["她"] = 0,
            ["它"] = 0, ["们"] = 0, ["这个"] = 0, ["那个"] = 0, ["哪个"] = 0,
            ["怎么"] = 0, ["为什么"] = 0, ["因为"] = 0, ["所以"] = 0, ["但是"] = 0,
            ["然后"] = 0, ["如果"] = 0, ["可以"] = 0, ["可能"] = 0,
            ["已经"] = 0, ["还是"] = 0, ["只是"] = 0, ["就是"] = 0, ["这样"] = 0,
            ["那样"] = 0, ["怎样"] = 0, ["这么"] = 0, ["那么"] = 0,
            ["更"] = 0, ["最"] = 0, ["比"] = 0, ["而"] = 0, ["且"] = 0, ["或"] = 0,
            ["与"] = 0, ["及"] = 0, ["等"] = 0, ["等等"] = 0, ["之"] = 0, ["于"] = 0,
            ["以"] = 0, ["为"] = 0, ["让"] = 0, ["把"] = 0, ["被"] = 0, ["从"] = 0,
            ["向"] = 0, ["对"] = 0, ["给"] = 0, ["跟"] = 0, ["像"] = 0, ["关于"] = 0,
            ["通过"] = 0, ["按照"] = 0, ["根据"] = 0, ["由于"] = 0, ["为了"] = 0,
            ["既然"] = 0, ["无论"] = 0, ["不管"] = 0, ["即使"] = 0,
            ["虽然"] = 0, ["哪怕"] = 0, ["只要"] = 0, ["除非"] = 0, ["假如"] = 0,
            ["倘若"] = 0, ["若是"] = 0, ["要是"] = 0
        };

        /// <summary>
        /// 英文停用词（使用 ConcurrentDictionary 保证线程安全）
        /// </summary>
        private static readonly ConcurrentDictionary<string, byte> EnglishStopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["a"] = 0, ["an"] = 0, ["the"] = 0, ["and"] = 0, ["or"] = 0, ["but"] = 0,
            ["in"] = 0, ["on"] = 0, ["at"] = 0, ["to"] = 0, ["for"] = 0, ["of"] = 0,
            ["with"] = 0, ["by"] = 0, ["from"] = 0, ["as"] = 0, ["is"] = 0, ["was"] = 0,
            ["are"] = 0, ["were"] = 0, ["been"] = 0, ["be"] = 0, ["have"] = 0, ["has"] = 0,
            ["had"] = 0, ["do"] = 0, ["does"] = 0, ["did"] = 0, ["will"] = 0, ["would"] = 0,
            ["could"] = 0, ["should"] = 0, ["may"] = 0, ["might"] = 0, ["must"] = 0,
            ["shall"] = 0, ["can"] = 0, ["need"] = 0, ["dare"] = 0, ["ought"] = 0,
            ["used"] = 0, ["it"] = 0, ["its"] = 0, ["this"] = 0, ["that"] = 0,
            ["these"] = 0, ["those"] = 0, ["i"] = 0, ["you"] = 0, ["he"] = 0, ["she"] = 0,
            ["we"] = 0, ["they"] = 0, ["what"] = 0, ["which"] = 0, ["who"] = 0,
            ["whom"] = 0, ["whose"] = 0, ["where"] = 0, ["when"] = 0, ["why"] = 0,
            ["how"] = 0, ["all"] = 0, ["each"] = 0, ["every"] = 0, ["both"] = 0,
            ["few"] = 0, ["more"] = 0, ["most"] = 0, ["other"] = 0, ["some"] = 0,
            ["such"] = 0, ["no"] = 0, ["not"] = 0, ["only"] = 0, ["same"] = 0, ["so"] = 0,
            ["than"] = 0, ["too"] = 0, ["very"] = 0, ["just"] = 0, ["also"] = 0,
            ["now"] = 0, ["here"] = 0, ["there"] = 0, ["then"] = 0, ["once"] = 0
        };

        /// <summary>
        /// 编译后的正则表达式（性能优化）
        /// </summary>
        private static readonly Regex ChinesePhraseRegex = new Regex(@"[\u4e00-\u9fa5]{2,}", RegexOptions.Compiled);
        private static readonly Regex EnglishWordRegex = new Regex(@"\b[a-zA-Z]{2,}\b", RegexOptions.Compiled);
        private static readonly Regex ChineseWordRegex = new Regex(@"[\u4e00-\u9fa5]+", RegexOptions.Compiled);
        private static readonly Regex NumberPatternRegex = new Regex(@"\b\d+(\.\d+)?\b", RegexOptions.Compiled);
        private static readonly Regex CleanTextRegex = new Regex(@"[\s\p{P}]+", RegexOptions.Compiled);
        private static readonly Regex ChineseCharRegex = new Regex(@"[\u4e00-\u9fa5]", RegexOptions.Compiled);

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
            var cleanText = CleanTextRegex.Replace(text, " ").Trim();

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

            foreach (Match match in ChinesePhraseRegex.Matches(text))
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

            foreach (Match match in EnglishWordRegex.Matches(text))
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
            foreach (Match match in ChineseWordRegex.Matches(text))
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
            foreach (Match match in EnglishWordRegex.Matches(text))
            {
                words.Add(match.Value.ToLower());
            }

            // 提取数字
            foreach (Match match in NumberPatternRegex.Matches(text))
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
            return ChineseStopWords.ContainsKey(word) || EnglishStopWords.ContainsKey(word);
        }

        /// <summary>
        /// 添加自定义停用词（线程安全）
        /// </summary>
        public static void AddStopWords(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                if (ChineseCharRegex.IsMatch(word))
                {
                    ChineseStopWords.TryAdd(word, 0);
                }
                else
                {
                    EnglishStopWords.TryAdd(word.ToLower(), 0);
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
