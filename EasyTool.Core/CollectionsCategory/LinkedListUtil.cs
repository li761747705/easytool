using System;
using System.Collections.Generic;

namespace EasyTool.CollectionsCategory
{
    /// <summary>
    /// 双向链表工具类
    /// 提供链表节点移动等组合操作功能
    /// </summary>
    public static class LinkedListUtil
    {
        /// <summary>
        /// 将双向链表中的某个节点移动到链表的结尾处。
        /// </summary>
        /// <typeparam name="T">双向链表元素类型</typeparam>
        /// <param name="list">双向链表</param>
        /// <param name="node">要移动的节点</param>
        public static void MoveLast<T>(LinkedList<T> list, LinkedListNode<T> node)
        {
            list.Remove(node);
            list.AddLast(node);
        }

        /// <summary>
        /// 将双向链表中移动到最前方
        /// </summary>
        /// <typeparam name="T">双向链表元素类型</typeparam>
        /// <param name="list">双向链表</param>
        /// <param name="node">要移动的节点</param>
        public static void MoveFirst<T>(LinkedList<T> list, LinkedListNode<T> node)
        {
            list.Remove(node);
            list.AddFirst(node);
        }
    }
}
