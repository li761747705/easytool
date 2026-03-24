using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// Aho-Corasick 自动机工具类
    /// 用于多模式字符串匹配，线性时间复杂度
    /// 常用于敏感词过滤、关键词检测等场景
    /// </summary>
    public static class AhoCorasickUtil
    {
        /// <summary>
        /// 创建 Aho-Corasick 自动机
        /// </summary>
        public static AhoCorasickAutomaton Create()
        {
            return new AhoCorasickAutomaton();
        }

        /// <summary>
        /// 从关键词集合创建 Aho-Corasick 自动机
        /// </summary>
        public static AhoCorasickAutomaton Create(IEnumerable<string> keywords)
        {
            var automaton = new AhoCorasickAutomaton();
            foreach (var keyword in keywords)
            {
                automaton.AddKeyword(keyword);
            }
            automaton.Build();
            return automaton;
        }
    }

    /// <summary>
    /// Aho-Corasick 自动机实现
    /// </summary>
    public class AhoCorasickAutomaton
    {
        private class Node
        {
            public Dictionary<char, Node> Children { get; } = new Dictionary<char, Node>();
            public Node Fail { get; set; }
            public List<string> Output { get; } = new List<string>();
            public int Depth { get; set; }
        }

        private readonly Node _root;
        private bool _built;

        /// <summary>
        /// 已添加的关键词数量
        /// </summary>
        public int KeywordCount { get; private set; }

        /// <summary>
        /// 是否已构建
        /// </summary>
        public bool IsBuilt => _built;

        /// <summary>
        /// 创建 Aho-Corasick 自动机
        /// </summary>
        public AhoCorasickAutomaton()
        {
            _root = new Node { Depth = 0 };
            _built = false;
            KeywordCount = 0;
        }

        /// <summary>
        /// 添加关键词
        /// </summary>
        public void AddKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                throw new ArgumentException("Keyword cannot be null or empty");
            if (_built)
                throw new InvalidOperationException("Cannot add keywords after building");

            var current = _root;
            foreach (char c in keyword)
            {
                if (!current.Children.TryGetValue(c, out var child))
                {
                    child = new Node { Depth = current.Depth + 1 };
                    current.Children[c] = child;
                }
                current = child;
            }

            if (current.Output.Count == 0 || !current.Output.Contains(keyword))
            {
                current.Output.Add(keyword);
                KeywordCount++;
            }
        }

        /// <summary>
        /// 批量添加关键词
        /// </summary>
        public void AddKeywords(IEnumerable<string> keywords)
        {
            foreach (var keyword in keywords)
            {
                AddKeyword(keyword);
            }
        }

        /// <summary>
        /// 构建自动机（构建失败指针）
        /// </summary>
        public void Build()
        {
            if (_built) return;

            var queue = new Queue<Node>();

            // 第一层节点的失败指针都指向根节点
            foreach (var child in _root.Children.Values)
            {
                child.Fail = _root;
                queue.Enqueue(child);
            }

            // BFS构建失败指针
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var kvp in current.Children)
                {
                    char c = kvp.Key;
                    var child = kvp.Value;

                    // 沿着失败指针找到能匹配当前字符的节点
                    var fail = current.Fail;
                    while (fail != null && !fail.Children.ContainsKey(c))
                    {
                        fail = fail.Fail;
                    }

                    child.Fail = fail?.Children.GetValueOrDefault(c) ?? _root;

                    // 合并输出
                    child.Output.AddRange(child.Fail.Output);

                    queue.Enqueue(child);
                }
            }

            _built = true;
        }

        /// <summary>
        /// 在文本中搜索所有匹配
        /// </summary>
        public IEnumerable<MatchResult> Search(string text)
        {
            if (!_built)
                throw new InvalidOperationException("Automaton must be built before searching");
            if (string.IsNullOrEmpty(text))
                yield break;

            var current = _root;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                // 沿着失败指针找到能匹配的节点
                while (current != _root && !current.Children.ContainsKey(c))
                {
                    current = current.Fail;
                }

                if (current.Children.TryGetValue(c, out var next))
                {
                    current = next;
                }

                // 输出所有匹配
                foreach (var output in current.Output)
                {
                    yield return new MatchResult
                    {
                        Keyword = output,
                        StartIndex = i - output.Length + 1,
                        EndIndex = i
                    };
                }
            }
        }

        /// <summary>
        /// 检查文本是否包含任何关键词
        /// </summary>
        public bool ContainsAny(string text)
        {
            if (!_built || string.IsNullOrEmpty(text))
                return false;

            var current = _root;
            foreach (char c in text)
            {
                while (current != _root && !current.Children.ContainsKey(c))
                {
                    current = current.Fail;
                }

                if (current.Children.TryGetValue(c, out var next))
                {
                    current = next;
                }

                if (current.Output.Count > 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 替换文本中的所有关键词
        /// </summary>
        public string Replace(string text, char replaceChar = '*')
        {
            if (!_built || string.IsNullOrEmpty(text))
                return text;

            var chars = text.ToCharArray();
            foreach (var match in Search(text))
            {
                for (int i = match.StartIndex; i <= match.EndIndex; i++)
                {
                    chars[i] = replaceChar;
                }
            }
            return new string(chars);
        }

        /// <summary>
        /// 替换文本中的所有关键词（自定义替换字符串）
        /// </summary>
        public string Replace(string text, string replacement)
        {
            if (!_built || string.IsNullOrEmpty(text))
                return text;

            var sb = new StringBuilder();
            int lastIndex = 0;
            var matches = new List<MatchResult>(Search(text));

            // 按开始位置排序
            matches.Sort((a, b) => a.StartIndex.CompareTo(b.StartIndex));

            foreach (var match in matches)
            {
                if (match.StartIndex >= lastIndex)
                {
                    sb.Append(text.Substring(lastIndex, match.StartIndex - lastIndex));
                    sb.Append(replacement);
                    lastIndex = match.EndIndex + 1;
                }
            }

            sb.Append(text.Substring(lastIndex));
            return sb.ToString();
        }

        /// <summary>
        /// 高亮文本中的所有关键词
        /// </summary>
        public string Highlight(string text, string prefix = "[", string suffix = "]")
        {
            if (!_built || string.IsNullOrEmpty(text))
                return text;

            var sb = new StringBuilder();
            int lastIndex = 0;
            var matches = new List<MatchResult>(Search(text));
            matches.Sort((a, b) => a.StartIndex.CompareTo(b.StartIndex));

            // 合并重叠的匹配
            var merged = MergeOverlaps(matches);

            foreach (var match in merged)
            {
                if (match.StartIndex >= lastIndex)
                {
                    sb.Append(text.Substring(lastIndex, match.StartIndex - lastIndex));
                    sb.Append(prefix);
                    sb.Append(text.Substring(match.StartIndex, match.EndIndex - match.StartIndex + 1));
                    sb.Append(suffix);
                    lastIndex = match.EndIndex + 1;
                }
            }

            sb.Append(text.Substring(lastIndex));
            return sb.ToString();
        }

        private List<MatchResult> MergeOverlaps(List<MatchResult> matches)
        {
            if (matches.Count == 0) return matches;

            var result = new List<MatchResult>();
            var current = matches[0];

            for (int i = 1; i < matches.Count; i++)
            {
                if (matches[i].StartIndex <= current.EndIndex)
                {
                    // 合并重叠
                    current = new MatchResult
                    {
                        StartIndex = current.StartIndex,
                        EndIndex = Math.Max(current.EndIndex, matches[i].EndIndex),
                        Keyword = current.Keyword
                    };
                }
                else
                {
                    result.Add(current);
                    current = matches[i];
                }
            }
            result.Add(current);

            return result;
        }

        /// <summary>
        /// 清空自动机
        /// </summary>
        public void Clear()
        {
            _root.Children.Clear();
            _root.Fail = null;
            _root.Output.Clear();
            _built = false;
            KeywordCount = 0;
        }
    }

    /// <summary>
    /// 匹配结果
    /// </summary>
    public class MatchResult
    {
        /// <summary>
        /// 匹配的关键词
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 开始索引
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// 结束索引
        /// </summary>
        public int EndIndex { get; set; }

        /// <summary>
        /// 匹配长度
        /// </summary>
        public int Length => EndIndex - StartIndex + 1;

        public override string ToString()
        {
            return $"{{Keyword: {Keyword}, Start: {StartIndex}, End: {EndIndex}}}";
        }
    }
}
