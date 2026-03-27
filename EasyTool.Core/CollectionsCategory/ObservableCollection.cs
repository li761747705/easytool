using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 可观察集合
    /// 当集合发生变化时触发事件
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class ObservableCollection<T> : IList<T>, INotifyCollectionChanged
    {
        private readonly List<T> _items = new();

        /// <summary>
        /// 集合变化事件
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 获取或设置指定索引的元素
        /// </summary>
        public T this[int index]
        {
            get => _items[index];
            set
            {
                var oldItem = _items[index];
                _items[index] = value;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace, value, oldItem, index));
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        public void Add(T item)
        {
            _items.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item, _items.Count - 1));
        }

        /// <summary>
        /// 添加多个元素
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            var index = _items.Count;
            var list = new List<T>(items);
            _items.AddRange(list);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, list, index));
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        public bool Remove(T item)
        {
            var index = _items.IndexOf(item);
            if (index < 0)
                return false;

            _items.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, item, index));
            return true;
        }

        /// <summary>
        /// 移除指定位置的元素
        /// </summary>
        public void RemoveAt(int index)
        {
            var item = _items[index];
            _items.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, item, index));
        }

        /// <summary>
        /// 清空集合
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// 移动元素
        /// </summary>
        public void Move(int oldIndex, int newIndex)
        {
            var item = _items[oldIndex];
            _items.RemoveAt(oldIndex);
            _items.Insert(newIndex, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
        }

        /// <summary>
        /// 查找元素索引
        /// </summary>
        public int IndexOf(T item) => _items.IndexOf(item);

        /// <summary>
        /// 是否包含元素
        /// </summary>
        public bool Contains(T item) => _items.Contains(item);

        /// <summary>
        /// 复制到数组
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        /// <summary>
        /// 获取枚举器
        /// </summary>
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// 获取枚举器
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        /// <summary>
        /// 触发集合变化事件
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}
