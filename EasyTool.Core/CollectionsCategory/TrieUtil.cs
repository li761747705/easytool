using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// Trie 树（前缀树）工具类
    /// 用于高效的字符串前缀搜索、自动补全等场景
    /// </summary>
    public static class TrieUtil
    {
        /// <summary>
        /// 创建 Trie 树
        /// </summary>
        public static Trie Create()
        {
            return new Trie();
        }

        /// <summary>
        /// 从字符串集合创建 Trie 树
        /// </summary>
        public static Trie Create(IEnumerable<string> words)
        {
            var trie = new Trie();
            foreach (var word in words)
            {
                trie.Add(word);
            }
            return trie;
        }
    }

    /// <summary>
    /// Trie 树实现
    /// </summary>
    public class Trie
    {
        private readonly TrieNode _root;

        /// <summary>
        /// 已存储的单词数量
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 创建 Trie 树
        /// </summary>
        public Trie()
        {
            _root = new TrieNode();
            Count = 0;
        }

        /// <summary>
        /// 添加单词
        /// </summary>
        public void Add(string word)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));

            var node = _root;
            foreach (char c in word)
            {
                if (!node.Children.ContainsKey(c))
                {
                    node.Children[c] = new TrieNode();
                }
                node = node.Children[c];
            }

            if (!node.IsEndOfWord)
            {
                node.IsEndOfWord = true;
                Count++;
            }
        }

        /// <summary>
        /// 批量添加单词
        /// </summary>
        public void AddRange(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                Add(word);
            }
        }

        /// <summary>
        /// 移除单词
        /// </summary>
        public bool Remove(string word)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));

            return Remove(_root, word, 0);
        }

        private bool Remove(TrieNode node, string word, int index)
        {
            if (index == word.Length)
            {
                if (!node.IsEndOfWord)
                    return false;

                node.IsEndOfWord = false;
                Count--;
                return node.Children.Count == 0;
            }

            char c = word[index];
            if (!node.Children.ContainsKey(c))
                return false;

            bool shouldDeleteChild = Remove(node.Children[c], word, index + 1);

            if (shouldDeleteChild)
            {
                node.Children.Remove(c);
                return !node.IsEndOfWord && node.Children.Count == 0;
            }

            return false;
        }

        /// <summary>
        /// 是否包含完整单词
        /// </summary>
        public bool Contains(string word)
        {
            var node = FindNode(word);
            return node != null && node.IsEndOfWord;
        }

        /// <summary>
        /// 是否包含指定前缀
        /// </summary>
        public bool StartsWith(string prefix)
        {
            return FindNode(prefix) != null;
        }

        /// <summary>
        /// 获取所有以指定前缀开头的单词
        /// </summary>
        public IEnumerable<string> GetWordsWithPrefix(string prefix)
        {
            var node = FindNode(prefix);
            if (node == null)
                return Enumerable.Empty<string>();

            return GetAllWords(node, prefix);
        }

        /// <summary>
        /// 自动补全（获取以指定前缀开头的所有单词）
        /// </summary>
        public IEnumerable<string> AutoComplete(string prefix, int maxResults = 10)
        {
            return GetWordsWithPrefix(prefix).Take(maxResults);
        }

        /// <summary>
        /// 获取所有单词
        /// </summary>
        public IEnumerable<string> GetAllWords()
        {
            return GetAllWords(_root, "");
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _root.Children.Clear();
            Count = 0;
        }

        /// <summary>
        /// 获取最长公共前缀
        /// </summary>
        public string GetLongestCommonPrefix()
        {
            var prefix = new System.Text.StringBuilder();
            var node = _root;

            while (node.Children.Count == 1 && !node.IsEndOfWord)
            {
                var child = node.Children.First();
                prefix.Append(child.Key);
                node = child.Value;
            }

            return prefix.ToString();
        }

        /// <summary>
        /// 计算与指定单词匹配的前缀长度
        /// </summary>
        public int MatchPrefixLength(string word)
        {
            if (word == null)
                return 0;

            var node = _root;
            int length = 0;

            foreach (char c in word)
            {
                if (!node.Children.ContainsKey(c))
                    break;

                length++;
                node = node.Children[c];
            }

            return length;
        }

        private TrieNode FindNode(string prefix)
        {
            if (prefix == null)
                return null;

            var node = _root;
            foreach (char c in prefix)
            {
                if (!node.Children.ContainsKey(c))
                    return null;
                node = node.Children[c];
            }
            return node;
        }

        private IEnumerable<string> GetAllWords(TrieNode node, string prefix)
        {
            if (node.IsEndOfWord)
            {
                yield return prefix;
            }

            foreach (var child in node.Children)
            {
                foreach (var word in GetAllWords(child.Value, prefix + child.Key))
                {
                    yield return word;
                }
            }
        }

        private class TrieNode
        {
            public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();
            public bool IsEndOfWord { get; set; }
        }
    }
}
