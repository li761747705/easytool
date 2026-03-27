using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 分词模式
    /// </summary>
    public enum SegmentMode
    {
        /// <summary>
        /// 精确模式
        /// </summary>
        Exact,

        /// <summary>
        /// 全模式
        /// </summary>
        Full,

        /// <summary>
        /// 搜索引擎模式
        /// </summary>
        Search
    }

    /// <summary>
    /// 中文分词工具类
    /// 提供基础的中文分词功能（基于词典）
    /// </summary>
    public static class SegmenterUtil
    {
        private static readonly HashSet<string> _defaultDictionary = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<char> _punctuation = new HashSet<char>
        {
            '\uFF0C', '\u3002', '\uFF01', '\uFF1F', '\uFF1B', '\uFF1A', // 中文标点：，。！？；：
            '\u201C', '\u201D', '\u2018', '\u2019', // 中文引号：""''
            '\uFF08', '\uFF09', '\u3010', '\u3011', '\u300A', '\u300B', // 中文括号：（）【】《》
            '\u3001', '\u2026', // 中文其他：、…
            ',', '.', '!', '?', ';', ':', '"', '\'', // 英文标点
            '(', ')', '[', ']', '<', '>', '/', '\\', '@', '#', '$', '%', '^', '&', '*' // 英文符号
        };

        private static readonly HashSet<string> _stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "的", "了", "在", "是", "我", "有", "和", "就", "不", "人", "都", "一", "一个",
            "上", "也", "很", "到", "说", "要", "去", "你", "会", "着", "没有", "看", "好",
            "自己", "这", "那", "但", "而", "与", "或", "因为", "所以", "如果", "虽然",
            "可以", "什么", "怎么", "如何", "为什么", "哪", "哪里", "哪个", "谁", "多少"
        };

        static SegmenterUtil()
        {
            // 初始化默认词典
            InitializeDefaultDictionary();
        }

        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="mode">分词模式</param>
        /// <returns>词语列表</returns>
        public static List<string> Segment(string text, SegmentMode mode = SegmentMode.Exact)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var result = new List<string>();
            var words = new List<string>();
            var buffer = new StringBuilder();

            int i = 0;
            while (i < text.Length)
            {
                // 跳过空白字符
                if (char.IsWhiteSpace(text[i]))
                {
                    if (buffer.Length > 0)
                    {
                        ProcessBuffer(buffer, words);
                        buffer.Clear();
                    }
                    i++;
                    continue;
                }

                // 处理标点符号
                if (_punctuation.Contains(text[i]))
                {
                    if (buffer.Length > 0)
                    {
                        ProcessBuffer(buffer, words);
                        buffer.Clear();
                    }
                    i++;
                    continue;
                }

                // 处理英文和数字
                if (IsEnglishOrDigit(text[i]))
                {
                    if (buffer.Length > 0 && !IsEnglishOrDigit(buffer[buffer.Length - 1]))
                    {
                        ProcessBuffer(buffer, words);
                        buffer.Clear();
                    }
                    buffer.Append(text[i]);
                    i++;
                    continue;
                }

                // 处理中文
                buffer.Append(text[i]);
                i++;

                // 尝试匹配词典中的词
                if (buffer.Length > 0 && !IsEnglishOrDigit(buffer[0]))
                {
                    var matched = TryMatchWord(buffer.ToString(), out var matchedWord);
                    if (matched)
                    {
                        // 检查是否可以匹配更长的词
                        if (i < text.Length && !IsEnglishOrDigit(text[i]))
                        {
                            var extended = buffer.ToString() + text[i];
                            if (_defaultDictionary.Contains(extended))
                            {
                                continue;
                            }
                        }

                        words.Add(matchedWord);
                        buffer.Clear();
                    }
                }
            }

            // 处理剩余的 buffer
            if (buffer.Length > 0)
            {
                ProcessBuffer(buffer, words);
            }

            return mode switch
            {
                SegmentMode.Full => GetAllPossibleWords(text),
                SegmentMode.Search => GetSearchModeWords(words),
                _ => words
            };
        }

        /// <summary>
        /// 分词并过滤停用词
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="mode">分词模式</param>
        /// <returns>词语列表（不含停用词）</returns>
        public static List<string> SegmentWithoutStopWords(string text, SegmentMode mode = SegmentMode.Exact)
        {
            return Segment(text, mode)
                .Where(w => !_stopWords.Contains(w))
                .ToList();
        }

        /// <summary>
        /// 提取关键词
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="topN">返回前N个关键词</param>
        /// <returns>关键词列表</returns>
        public static List<string> ExtractKeywords(string text, int topN = 10)
        {
            var words = SegmentWithoutStopWords(text);
            var frequency = new Dictionary<string, int>();

            foreach (var word in words)
            {
                if (word.Length < 2)
                    continue;

                if (frequency.ContainsKey(word))
                    frequency[word]++;
                else
                    frequency[word] = 1;
            }

            return frequency
                .OrderByDescending(kvp => kvp.Value)
                .Take(topN)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// 添加自定义词典
        /// </summary>
        /// <param name="words">词语列表</param>
        public static void AddToDictionary(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    _defaultDictionary.Add(word.Trim());
                }
            }
        }

        /// <summary>
        /// 添加停用词
        /// </summary>
        /// <param name="words">停用词列表</param>
        public static void AddStopWords(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    _stopWords.Add(word.Trim());
                }
            }
        }

        /// <summary>
        /// 检查是否为中文
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns>是否为中文</returns>
        public static bool IsChinese(char c)
        {
            return c >= 0x4E00 && c <= 0x9FA5;
        }

        /// <summary>
        /// 检查是否为英文或数字
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns>是否为英文或数字</returns>
        public static bool IsEnglishOrDigit(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   char.IsDigit(c);
        }

        /// <summary>
        /// 统计词频
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>词频字典</returns>
        public static Dictionary<string, int> GetWordFrequency(string text)
        {
            var words = SegmentWithoutStopWords(text);
            var frequency = new Dictionary<string, int>();

            foreach (var word in words)
            {
                if (frequency.ContainsKey(word))
                    frequency[word]++;
                else
                    frequency[word] = 1;
            }

            return frequency;
        }

        #region 私有方法

        private static void InitializeDefaultDictionary()
        {
            // 常用词汇
            var commonWords = new[]
            {
                "中国", "北京", "上海", "广州", "深圳", "杭州", "南京", "武汉", "成都", "西安",
                "计算机", "互联网", "软件", "硬件", "程序", "开发", "设计", "测试", "运维", "管理",
                "公司", "企业", "集团", "有限", "责任", "股份", "有限", "科技", "技术", "信息",
                "手机", "电脑", "笔记本", "平板", "显示器", "键盘", "鼠标", "耳机", "音箱",
                "汽车", "火车", "飞机", "地铁", "公交", "出租车", "自行车", "电动车",
                "今天", "明天", "昨天", "上午", "下午", "晚上", "中午", "早上", "傍晚",
                "时间", "地点", "人物", "事件", "原因", "结果", "过程", "方法", "步骤",
                "学习", "工作", "生活", "娱乐", "运动", "休息", "旅游", "购物", "吃饭",
                "银行", "医院", "学校", "超市", "商场", "餐厅", "酒店", "公园", "图书馆",
                "苹果", "香蕉", "橙子", "西瓜", "葡萄", "草莓", "芒果", "桃子", "梨子",
                "开始", "结束", "继续", "暂停", "停止", "运行", "执行", "完成", "失败", "成功",
                "问题", "答案", "解决", "方案", "建议", "意见", "观点", "看法", "想法", "思路",
                "重要", "紧急", "必要", "可能", "必须", "应该", "需要", "想要", "希望", "期待",
                "人工智能", "机器学习", "深度学习", "自然语言", "计算机视觉", "数据分析",
                "云计算", "大数据", "区块链", "物联网", "虚拟现实", "增强现实",
                "程序员", "工程师", "设计师", "产品经理", "项目经理", "架构师", "测试工程师"
            };

            foreach (var word in commonWords)
            {
                _defaultDictionary.Add(word);
            }
        }

        private static void ProcessBuffer(StringBuilder buffer, List<string> words)
        {
            var text = buffer.ToString();

            if (string.IsNullOrWhiteSpace(text))
                return;

            // 如果是英文或数字，直接添加
            if (text.All(c => IsEnglishOrDigit(c) || char.IsWhiteSpace(c)))
            {
                words.Add(text.Trim());
                return;
            }

            // 对于中文，进行最大匹配分词
            var segments = MaxMatchSegment(text);
            words.AddRange(segments);
        }

        private static List<string> MaxMatchSegment(string text)
        {
            var result = new List<string>();
            var maxLength = 5; // 最大词长
            var i = 0;

            while (i < text.Length)
            {
                var matched = false;

                // 从最大长度开始匹配
                for (var len = Math.Min(maxLength, text.Length - i); len >= 1; len--)
                {
                    var word = text.Substring(i, len);

                    if (_defaultDictionary.Contains(word))
                    {
                        result.Add(word);
                        i += len;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    // 单字切分
                    result.Add(text[i].ToString());
                    i++;
                }
            }

            return result;
        }

        private static bool TryMatchWord(string text, out string matchedWord)
        {
            matchedWord = string.Empty;

            if (string.IsNullOrEmpty(text))
                return false;

            // 精确匹配
            if (_defaultDictionary.Contains(text))
            {
                matchedWord = text;
                return true;
            }

            // 尝试匹配最长前缀词
            for (int len = text.Length; len >= 1; len--)
            {
                var prefix = text.Substring(0, len);
                if (_defaultDictionary.Contains(prefix))
                {
                    matchedWord = prefix;
                    return true;
                }
            }

            return false;
        }

        private static List<string> GetAllPossibleWords(string text)
        {
            var result = new List<string>();

            for (int i = 0; i < text.Length; i++)
            {
                for (int len = 1; len <= text.Length - i && len <= 5; len++)
                {
                    var word = text.Substring(i, len);
                    if (_defaultDictionary.Contains(word))
                    {
                        result.Add(word);
                    }
                }
            }

            return result;
        }

        private static List<string> GetSearchModeWords(List<string> words)
        {
            var result = new List<string>();

            foreach (var word in words)
            {
                result.Add(word);

                // 对长词进行二次切分
                if (word.Length > 2)
                {
                    for (int len = 2; len < word.Length; len++)
                    {
                        for (int i = 0; i <= word.Length - len; i++)
                        {
                            var subWord = word.Substring(i, len);
                            if (_defaultDictionary.Contains(subWord))
                            {
                                result.Add(subWord);
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
