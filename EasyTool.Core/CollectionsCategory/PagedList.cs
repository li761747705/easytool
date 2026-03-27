using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 分页列表
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class PagedList<T> : IList<T>
    {
        private readonly List<T> _items;

        /// <summary>
        /// 当前页号（从1开始）
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// 是否是第一页
        /// </summary>
        public bool IsFirstPage => PageNumber == 1;

        /// <summary>
        /// 是否是最后一页
        /// </summary>
        public bool IsLastPage => PageNumber == TotalPages;

        /// <summary>
        /// 当前页记录数
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
            set => _items[index] = value;
        }

        /// <summary>
        /// 创建分页列表
        /// </summary>
        public PagedList(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "页号必须大于0");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "每页大小必须大于0");

            _items = new List<T>(items);
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        /// <summary>
        /// 从完整列表创建分页
        /// </summary>
        public static PagedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var list = new List<T>(source);
            var totalCount = list.Count;
            var skip = (pageNumber - 1) * pageSize;
            var items = list.Skip(skip).Take(pageSize);
            return new PagedList<T>(items, pageNumber, pageSize, totalCount);
        }

        /// <summary>
        /// 从查询创建分页
        /// </summary>
        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var totalCount = source.Count();
            var skip = (pageNumber - 1) * pageSize;
            var items = source.Skip(skip).Take(pageSize).ToList();
            return new PagedList<T>(items, pageNumber, pageSize, totalCount);
        }

        /// <summary>
        /// 获取页码范围
        /// </summary>
        public IEnumerable<int> GetPageRange(int displayCount = 5)
        {
            var start = Math.Max(1, PageNumber - displayCount / 2);
            var end = Math.Min(TotalPages, start + displayCount - 1);

            if (end - start + 1 < displayCount)
            {
                start = Math.Max(1, end - displayCount + 1);
            }

            for (int i = start; i <= end; i++)
            {
                yield return i;
            }
        }

        /// <summary>
        /// 获取分页信息
        /// </summary>
        public PageInfo GetPageInfo()
        {
            return new PageInfo
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                TotalCount = TotalCount,
                TotalPages = TotalPages,
                HasPreviousPage = HasPreviousPage,
                HasNextPage = HasNextPage
            };
        }

        #region IList<T> 实现

        public int IndexOf(T item) => _items.IndexOf(item);

        public void Insert(int index, T item) => _items.Insert(index, item);

        public void RemoveAt(int index) => _items.RemoveAt(index);

        public void Add(T item) => _items.Add(item);

        public void Clear() => _items.Clear();

        public bool Contains(T item) => _items.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public bool Remove(T item) => _items.Remove(item);

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        #endregion
    }

    /// <summary>
    /// 分页信息
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// 当前页号
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage { get; set; }
    }

    /// <summary>
    /// 分页工具类
    /// </summary>
    public static class PagedListExtensions
    {
        /// <summary>
        /// 转换为分页列表
        /// </summary>
        public static PagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            return PagedList<T>.Create(source, pageNumber, pageSize);
        }

        /// <summary>
        /// 转换为分页列表
        /// </summary>
        public static PagedList<T> ToPagedList<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            return PagedList<T>.Create(source, pageNumber, pageSize);
        }
    }
}
