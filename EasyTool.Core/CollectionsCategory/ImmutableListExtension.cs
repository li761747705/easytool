using System;
using System.Collections;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 不可变列表扩展
    /// </summary>
    public static class ImmutableListExtension
    {
        /// <summary>
        /// 创建不可变列表
        /// </summary>
        public static ImmutableList<T> ToImmutableList<T>(this IEnumerable<T> source)
        {
            return new ImmutableList<T>(source);
        }

        /// <summary>
        /// 添加元素并返回新列表
        /// </summary>
        public static ImmutableList<T> AddItem<T>(this ImmutableList<T> list, T item)
        {
            return list.Add(item);
        }

        /// <summary>
        /// 添加多个元素并返回新列表
        /// </summary>
        public static ImmutableList<T> AddRangeItems<T>(this ImmutableList<T> list, IEnumerable<T> items)
        {
            return list.AddRange(items);
        }

        /// <summary>
        /// 移除元素并返回新列表
        /// </summary>
        public static ImmutableList<T> RemoveItem<T>(this ImmutableList<T> list, T item)
        {
            return list.Remove(item);
        }

        /// <summary>
        /// 更新元素并返回新列表
        /// </summary>
        public static ImmutableList<T> SetItem<T>(this ImmutableList<T> list, int index, T item)
        {
            return list.SetItem(index, item);
        }

        /// <summary>
        /// 移除指定位置的元素并返回新列表
        /// </summary>
        public static ImmutableList<T> RemoveItemAt<T>(this ImmutableList<T> list, int index)
        {
            return list.RemoveAt(index);
        }

        /// <summary>
        /// 插入元素并返回新列表
        /// </summary>
        public static ImmutableList<T> InsertItem<T>(this ImmutableList<T> list, int index, T item)
        {
            return list.Insert(index, item);
        }
    }

    /// <summary>
    /// 不可变列表
    /// </summary>
    public sealed class ImmutableList<T> : IReadOnlyList<T>, IEquatable<ImmutableList<T>>
    {
        private readonly T[] _items;

        /// <summary>
        /// 空列表
        /// </summary>
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();

        /// <summary>
        /// 创建不可变列表
        /// </summary>
        public ImmutableList()
        {
            _items = Array.Empty<T>();
        }

        /// <summary>
        /// 从集合创建不可变列表
        /// </summary>
        public ImmutableList(IEnumerable<T> items)
        {
            _items = items as T[] ?? new List<T>(items).ToArray();
        }

        /// <summary>
        /// 从数组创建不可变列表
        /// </summary>
        public ImmutableList(T[] items)
        {
            _items = items ?? Array.Empty<T>();
        }

        /// <summary>
        /// 获取指定索引处的元素
        /// </summary>
        public T this[int index] => _items[index];

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _items.Length;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _items.Length == 0;

        /// <summary>
        /// 添加元素并返回新列表
        /// </summary>
        public ImmutableList<T> Add(T item)
        {
            var newArray = new T[_items.Length + 1];
            Array.Copy(_items, newArray, _items.Length);
            newArray[_items.Length] = item;
            return new ImmutableList<T>(newArray);
        }

        /// <summary>
        /// 添加多个元素并返回新列表
        /// </summary>
        public ImmutableList<T> AddRange(IEnumerable<T> items)
        {
            var itemsList = new List<T>(items);
            var newArray = new T[_items.Length + itemsList.Count];
            Array.Copy(_items, newArray, _items.Length);
            itemsList.CopyTo(newArray, _items.Length);
            return new ImmutableList<T>(newArray);
        }

        /// <summary>
        /// 移除元素并返回新列表
        /// </summary>
        public ImmutableList<T> Remove(T item)
        {
            var index = IndexOf(item);
            return index >= 0 ? RemoveAt(index) : this;
        }

        /// <summary>
        /// 移除满足条件的元素并返回新列表
        /// </summary>
        public ImmutableList<T> RemoveAll(Predicate<T> match)
        {
            var newList = new List<T>();
            foreach (var item in _items)
            {
                if (!match(item))
                    newList.Add(item);
            }
            return new ImmutableList<T>(newList);
        }

        /// <summary>
        /// 移除指定位置的元素并返回新列表
        /// </summary>
        public ImmutableList<T> RemoveAt(int index)
        {
            if (index < 0 || index >= _items.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var newArray = new T[_items.Length - 1];
            Array.Copy(_items, 0, newArray, 0, index);
            Array.Copy(_items, index + 1, newArray, index, _items.Length - index - 1);
            return new ImmutableList<T>(newArray);
        }

        /// <summary>
        /// 插入元素并返回新列表
        /// </summary>
        public ImmutableList<T> Insert(int index, T item)
        {
            if (index < 0 || index > _items.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var newArray = new T[_items.Length + 1];
            Array.Copy(_items, 0, newArray, 0, index);
            newArray[index] = item;
            Array.Copy(_items, index, newArray, index + 1, _items.Length - index);
            return new ImmutableList<T>(newArray);
        }

        /// <summary>
        /// 更新指定位置的元素并返回新列表
        /// </summary>
        public ImmutableList<T> SetItem(int index, T item)
        {
            if (index < 0 || index >= _items.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var newArray = (T[])_items.Clone();
            newArray[index] = item;
            return new ImmutableList<T>(newArray);
        }

        /// <summary>
        /// 查找元素索引
        /// </summary>
        public int IndexOf(T item)
        {
            return Array.IndexOf(_items, item);
        }

        /// <summary>
        /// 查找元素索引
        /// </summary>
        public int IndexOf(T item, int startIndex)
        {
            return Array.IndexOf(_items, item, startIndex);
        }

        /// <summary>
        /// 查找元素索引
        /// </summary>
        public int IndexOf(T item, int startIndex, int count)
        {
            return Array.IndexOf(_items, item, startIndex, count);
        }

        /// <summary>
        /// 是否包含元素
        /// </summary>
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>
        /// 复制到数组
        /// </summary>
        public void CopyTo(T[] array)
        {
            _items.CopyTo(array, 0);
        }

        /// <summary>
        /// 复制到数组
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 转换为数组
        /// </summary>
        public T[] ToArray()
        {
            return (T[])_items.Clone();
        }

        /// <summary>
        /// 查找元素
        /// </summary>
        public T? Find(Predicate<T> match)
        {
            foreach (var item in _items)
            {
                if (match(item))
                    return item;
            }
            return default;
        }

        /// <summary>
        /// 查找所有元素
        /// </summary>
        public ImmutableList<T> FindAll(Predicate<T> match)
        {
            var result = new List<T>();
            foreach (var item in _items)
            {
                if (match(item))
                    result.Add(item);
            }
            return new ImmutableList<T>(result);
        }

        /// <summary>
        /// 是否存在满足条件的元素
        /// </summary>
        public bool Exists(Predicate<T> match)
        {
            return FindIndex(match) >= 0;
        }

        /// <summary>
        /// 查找满足条件的元素索引
        /// </summary>
        public int FindIndex(Predicate<T> match)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (match(_items[i]))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 对每个元素执行操作
        /// </summary>
        public void ForEach(Action<T> action)
        {
            foreach (var item in _items)
            {
                action(item);
            }
        }

        /// <summary>
        /// 转换元素类型
        /// </summary>
        public ImmutableList<TResult> ConvertAll<TResult>(Converter<T, TResult> converter)
        {
            var result = new TResult[_items.Length];
            for (int i = 0; i < _items.Length; i++)
            {
                result[i] = converter(_items[i]);
            }
            return new ImmutableList<TResult>(result);
        }

        /// <summary>
        /// 获取范围
        /// </summary>
        public ImmutableList<T> GetRange(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (index + count > _items.Length)
                throw new ArgumentException("范围超出列表边界");

            var result = new T[count];
            Array.Copy(_items, index, result, 0, count);
            return new ImmutableList<T>(result);
        }

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        #endregion

        #region IEquatable

        public bool Equals(ImmutableList<T>? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (_items.Length != other._items.Length)
                return false;

            for (int i = 0; i < _items.Length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(_items[i], other._items[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ImmutableList<T>);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var item in _items)
            {
                hash.Add(item);
            }
            return hash.ToHashCode();
        }

        public static bool operator ==(ImmutableList<T>? left, ImmutableList<T>? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImmutableList<T>? left, ImmutableList<T>? right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"[{string.Join(", ", _items)}]";
        }
    }
}
