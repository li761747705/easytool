using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 跳表工具类
    /// 一种随机化的数据结构，基于并联的链表，实现高效查找、插入、删除
    /// 平均时间复杂度 O(log n)
    /// </summary>
    /// <remarks>
    /// 线程安全：否。外部需要同步访问。
    /// </remarks>
    public static class SkipListUtil
    {
        /// <summary>
        /// 创建跳表
        /// </summary>
        public static SkipList<TKey, TValue> Create<TKey, TValue>()
            where TKey : IComparable<TKey>
        {
            return new SkipList<TKey, TValue>();
        }

        /// <summary>
        /// 创建指定最大层级的跳表
        /// </summary>
        public static SkipList<TKey, TValue> Create<TKey, TValue>(int maxLevel, double probability = 0.5)
            where TKey : IComparable<TKey>
        {
            return new SkipList<TKey, TValue>(maxLevel, probability);
        }
    }

    /// <summary>
    /// 跳表实现
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public class SkipList<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private class SkipListNode
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public SkipListNode[] Forward { get; set; }

            public SkipListNode(int level, TKey key = default, TValue value = default)
            {
                Key = key;
                Value = value;
                Forward = new SkipListNode[level + 1];
            }
        }

        private readonly SkipListNode _header;
        private readonly int _maxLevel;
        private readonly double _probability;
        private readonly Random _random;
        private int _level;
        private int _count;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 键集合
        /// </summary>
        public ICollection<TKey> Keys => GetElements().Select(x => x.Key).ToList();

        /// <summary>
        /// 值集合
        /// </summary>
        public ICollection<TValue> Values => GetElements().Select(x => x.Value).ToList();

        /// <summary>
        /// 索引访问
        /// </summary>
        public TValue this[TKey key]
        {
            get => Get(key);
            set => Add(key, value);
        }

        /// <summary>
        /// 创建跳表（默认最大16层）
        /// </summary>
        public SkipList() : this(16, 0.5) { }

        /// <summary>
        /// 创建指定层级的跳表
        /// </summary>
        public SkipList(int maxLevel, double probability = 0.5)
        {
            if (maxLevel <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLevel));
            if (probability <= 0 || probability >= 1)
                throw new ArgumentOutOfRangeException(nameof(probability));

            _maxLevel = maxLevel;
            _probability = probability;
            _random = new Random();
            _level = 0;
            _count = 0;
            _header = new SkipListNode(_maxLevel);
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            var update = new SkipListNode[_maxLevel + 1];
            var current = _header;

            for (int i = _level; i >= 0; i--)
            {
                while (current.Forward[i] != null && current.Forward[i].Key.CompareTo(key) < 0)
                {
                    current = current.Forward[i];
                }
                update[i] = current;
            }

            current = current.Forward[0];

            if (current != null && current.Key.CompareTo(key) == 0)
            {
                current.Value = value;
                return;
            }

            int newLevel = RandomLevel();

            if (newLevel > _level)
            {
                for (int i = _level + 1; i <= newLevel; i++)
                {
                    update[i] = _header;
                }
                _level = newLevel;
            }

            var newNode = new SkipListNode(newLevel, key, value);

            for (int i = 0; i <= newLevel; i++)
            {
                newNode.Forward[i] = update[i].Forward[i];
                update[i].Forward[i] = newNode;
            }

            _count++;
        }

        /// <summary>
        /// 添加键值对
        /// </summary>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public TValue Get(TKey key)
        {
            if (!TryGetValue(key, out var value))
                throw new KeyNotFoundException($"Key '{key}' not found");
            return value;
        }

        /// <summary>
        /// 尝试获取值
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var current = _header;

            for (int i = _level; i >= 0; i--)
            {
                while (current.Forward[i] != null && current.Forward[i].Key.CompareTo(key) < 0)
                {
                    current = current.Forward[i];
                }
            }

            current = current.Forward[0];

            if (current != null && current.Key.CompareTo(key) == 0)
            {
                value = current.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 是否包含键
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return TryGetValue(key, out _);
        }

        /// <summary>
        /// 是否包含键值对
        /// </summary>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!TryGetValue(item.Key, out var value))
                return false;
            return EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(TKey key)
        {
            var update = new SkipListNode[_maxLevel + 1];
            var current = _header;

            for (int i = _level; i >= 0; i--)
            {
                while (current.Forward[i] != null && current.Forward[i].Key.CompareTo(key) < 0)
                {
                    current = current.Forward[i];
                }
                update[i] = current;
            }

            current = current.Forward[0];

            if (current == null || current.Key.CompareTo(key) != 0)
                return false;

            for (int i = 0; i <= _level; i++)
            {
                if (update[i].Forward[i] != current)
                    break;
                update[i].Forward[i] = current.Forward[i];
            }

            while (_level > 0 && _header.Forward[_level] == null)
            {
                _level--;
            }

            _count--;
            return true;
        }

        /// <summary>
        /// 移除键值对
        /// </summary>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!Contains(item))
                return false;
            return Remove(item.Key);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i <= _maxLevel; i++)
            {
                _header.Forward[i] = null;
            }
            _level = 0;
            _count = 0;
        }

        /// <summary>
        /// 复制到数组
        /// </summary>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var item in GetElements())
            {
                array[arrayIndex++] = item;
            }
        }

        /// <summary>
        /// 获取第一个元素
        /// </summary>
        public KeyValuePair<TKey, TValue>? First()
        {
            if (_header.Forward[0] == null)
                return null;
            return new KeyValuePair<TKey, TValue>(_header.Forward[0].Key, _header.Forward[0].Value);
        }

        /// <summary>
        /// 获取范围
        /// </summary>
        public IEnumerable<KeyValuePair<TKey, TValue>> GetRange(TKey start, TKey end)
        {
            var current = _header;

            for (int i = _level; i >= 0; i--)
            {
                while (current.Forward[i] != null && current.Forward[i].Key.CompareTo(start) < 0)
                {
                    current = current.Forward[i];
                }
            }

            current = current.Forward[0];

            while (current != null && current.Key.CompareTo(end) <= 0)
            {
                yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                current = current.Forward[0];
            }
        }

        private int RandomLevel()
        {
            int level = 0;
            while (_random.NextDouble() < _probability && level < _maxLevel)
            {
                level++;
            }
            return level;
        }

        private IEnumerable<KeyValuePair<TKey, TValue>> GetElements()
        {
            var current = _header.Forward[0];
            while (current != null)
            {
                yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                current = current.Forward[0];
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return GetElements().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
