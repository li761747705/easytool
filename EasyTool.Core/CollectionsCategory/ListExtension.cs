using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// List 集合扩展方法
    /// </summary>
    public static class ListExtension
    {
        #region 列表合并

        /// <summary>
        /// 将指定的列表连接起来，形成一个新的列表。
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="lists">要连接的列表</param>
        /// <returns>连接后的新列表</returns>
        public static List<T> Concat<T>(this IEnumerable<List<T>> lists)
        {
            return lists.SelectMany(x => x).ToList();
        }

        /// <summary>
        /// 将指定的列表连接起来，形成一个新的列表。
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="lists">要连接的列表</param>
        /// <returns>连接后的新列表</returns>
        public static List<T> Concat<T>(this List<T> list, params List<T>[] lists)
        {
            return Concat(new[] { list }.Concat(lists));
        }

        #endregion

        #region 列表分页

        /// <summary>
        /// 将列表中的元素分页显示。
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="list">要分页的列表</param>
        /// <param name="pageSize">每页显示的元素数量</param>
        /// <param name="pageIndex">要显示的页码，从 0 开始</param>
        /// <returns>指定页的元素列表</returns>
        public static List<T> Page<T>(this List<T> list, int pageSize, int pageIndex)
        {
            return list.Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();
        }

        #endregion

        #region 列表比较

        /// <summary>
        /// 判断两个列表是否相等。
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="list1">要比较的第一个列表</param>
        /// <param name="list2">要比较的第二个列表</param>
        /// <returns>如果两个列表相等，则返回 true；否则返回 false</returns>
        public static bool Equals<T>(this List<T>? list1, List<T>? list2)
        {
            if (list1 == null && list2 == null)
            {
                return true;
            }
            else if (list1 == null || list2 == null)
            {
                return false;
            }
            else if (list1.Count != list2.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < list1.Count; i++)
                {
                    if (!EqualityComparer<T>.Default.Equals(list1[i], list2[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        #endregion
    }
}
