using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EasyTool.ReflectCategory
{
    /// <summary>
    /// 表达式工具类
    /// </summary>
    public static class ExpressionUtil
    {
        #region 属性访问

        /// <summary>
        /// 获取属性名称
        /// </summary>
        public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            if (propertyExpression.Body is UnaryExpression unaryExpression &&
                unaryExpression.Operand is MemberExpression operand)
            {
                return operand.Member.Name;
            }

            throw new ArgumentException("表达式不是有效的属性访问表达式");
        }

        /// <summary>
        /// 获取属性信息
        /// </summary>
        public static PropertyInfo GetProperty<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExpression &&
                memberExpression.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo;
            }

            if (propertyExpression.Body is UnaryExpression unaryExpression &&
                unaryExpression.Operand is MemberExpression operand &&
                operand.Member is PropertyInfo propInfo)
            {
                return propInfo;
            }

            throw new ArgumentException("表达式不是有效的属性访问表达式");
        }

        /// <summary>
        /// 创建属性获取器
        /// </summary>
        public static Func<T, TProperty> CreateGetter<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            return propertyExpression.Compile();
        }

        /// <summary>
        /// 创建属性设置器
        /// </summary>
        public static Action<T, TProperty> CreateSetter<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            var parameter = Expression.Parameter(typeof(TProperty), "value");
            var property = GetProperty(propertyExpression);

            var setter = Expression.Lambda<Action<T, TProperty>>(
                Expression.Call(propertyExpression.Parameters[0], property.GetSetMethod()!, parameter),
                propertyExpression.Parameters[0], parameter);

            return setter.Compile();
        }

        #endregion

        #region 条件表达式

        /// <summary>
        /// 组合多个条件表达式（AND）
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(params Expression<Func<T, bool>>[] expressions)
        {
            if (expressions == null || expressions.Length == 0)
                return _ => true;

            if (expressions.Length == 1)
                return expressions[0];

            var parameter = expressions[0].Parameters[0];
            var body = expressions[0].Body;

            for (int i = 1; i < expressions.Length; i++)
            {
                var visitor = new ParameterReplacer(expressions[i].Parameters[0], parameter);
                body = Expression.AndAlso(body, visitor.Visit(expressions[i].Body));
            }

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        /// <summary>
        /// 组合多个条件表达式（OR）
        /// </summary>
        public static Expression<Func<T, bool>> Or<T>(params Expression<Func<T, bool>>[] expressions)
        {
            if (expressions == null || expressions.Length == 0)
                return _ => false;

            if (expressions.Length == 1)
                return expressions[0];

            var parameter = expressions[0].Parameters[0];
            var body = expressions[0].Body;

            for (int i = 1; i < expressions.Length; i++)
            {
                var visitor = new ParameterReplacer(expressions[i].Parameters[0], parameter);
                body = Expression.OrElse(body, visitor.Visit(expressions[i].Body));
            }

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        /// <summary>
        /// 取反条件表达式
        /// </summary>
        public static Expression<Func<T, bool>> Not<T>(Expression<Func<T, bool>> expression)
        {
            var body = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(body, expression.Parameters[0]);
        }

        #endregion

        #region 排序表达式

        /// <summary>
        /// 创建排序表达式
        /// </summary>
        public static Expression<Func<T, TKey>> CreateOrderBy<T, TKey>(Expression<Func<T, TKey>> keySelector)
        {
            return keySelector;
        }

        /// <summary>
        /// 应用排序
        /// </summary>
        public static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string propertyName, bool ascending = true)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = ascending ? "OrderBy" : "OrderByDescending";
            var method = typeof(Queryable).GetMethods()
                .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                .Single();

            var genericMethod = method.MakeGenericMethod(typeof(T), property.Type);
            return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { source, lambda })!;
        }

        /// <summary>
        /// 应用后续排序
        /// </summary>
        public static IOrderedQueryable<T> ApplyThenBy<T>(IOrderedQueryable<T> source, string propertyName, bool ascending = true)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = ascending ? "ThenBy" : "ThenByDescending";
            var method = typeof(Queryable).GetMethods()
                .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                .Single();

            var genericMethod = method.MakeGenericMethod(typeof(T), property.Type);
            return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { source, lambda })!;
        }

        #endregion

        #region 构造表达式

        /// <summary>
        /// 创建等于条件
        /// </summary>
        public static Expression<Func<T, bool>> CreateEqual<T, TValue>(Expression<Func<T, TValue>> propertyExpression, TValue value)
        {
            var parameter = propertyExpression.Parameters[0];
            var property = propertyExpression.Body;
            var constant = Expression.Constant(value, typeof(TValue));
            var body = Expression.Equal(property, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        /// <summary>
        /// 创建大于条件
        /// </summary>
        public static Expression<Func<T, bool>> CreateGreaterThan<T, TValue>(Expression<Func<T, TValue>> propertyExpression, TValue value)
            where TValue : IComparable<TValue>
        {
            var parameter = propertyExpression.Parameters[0];
            var property = propertyExpression.Body;
            var constant = Expression.Constant(value, typeof(TValue));
            var body = Expression.GreaterThan(property, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        /// <summary>
        /// 创建小于条件
        /// </summary>
        public static Expression<Func<T, bool>> CreateLessThan<T, TValue>(Expression<Func<T, TValue>> propertyExpression, TValue value)
            where TValue : IComparable<TValue>
        {
            var parameter = propertyExpression.Parameters[0];
            var property = propertyExpression.Body;
            var constant = Expression.Constant(value, typeof(TValue));
            var body = Expression.LessThan(property, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        /// <summary>
        /// 创建包含条件
        /// </summary>
        public static Expression<Func<T, bool>> CreateContains<T>(Expression<Func<T, string>> propertyExpression, string value)
        {
            var parameter = propertyExpression.Parameters[0];
            var property = propertyExpression.Body;
            var constant = Expression.Constant(value);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            var body = Expression.Call(property, containsMethod, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        /// <summary>
        /// 创建范围条件
        /// </summary>
        public static Expression<Func<T, bool>> CreateInRange<T, TValue>(
            Expression<Func<T, TValue>> propertyExpression,
            TValue min, TValue max) where TValue : IComparable<TValue>
        {
            var greaterThanOrEqual = CreateGreaterThanOrEqual(propertyExpression, min);
            var lessThanOrEqual = CreateLessThanOrEqual(propertyExpression, max);
            return And(greaterThanOrEqual, lessThanOrEqual);
        }

        /// <summary>
        /// 创建大于等于条件
        /// </summary>
        public static Expression<Func<T, bool>> CreateGreaterThanOrEqual<T, TValue>(
            Expression<Func<T, TValue>> propertyExpression, TValue value)
            where TValue : IComparable<TValue>
        {
            var parameter = propertyExpression.Parameters[0];
            var property = propertyExpression.Body;
            var constant = Expression.Constant(value, typeof(TValue));
            var body = Expression.GreaterThanOrEqual(property, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        /// <summary>
        /// 创建小于等于条件
        /// </summary>
        public static Expression<Func<T, bool>> CreateLessThanOrEqual<T, TValue>(
            Expression<Func<T, TValue>> propertyExpression, TValue value)
            where TValue : IComparable<TValue>
        {
            var parameter = propertyExpression.Parameters[0];
            var property = propertyExpression.Body;
            var constant = Expression.Constant(value, typeof(TValue));
            var body = Expression.LessThanOrEqual(property, constant);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        #endregion

        #region 编译执行

        /// <summary>
        /// 编译并执行表达式
        /// </summary>
        public static TResult Execute<T, TResult>(Expression<Func<T, TResult>> expression, T instance)
        {
            var func = expression.Compile();
            return func(instance);
        }

        /// <summary>
        /// 编译表达式
        /// </summary>
        public static Func<T, TResult> Compile<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return expression.Compile();
        }

        #endregion
    }

    /// <summary>
    /// 参数替换器
    /// </summary>
    internal class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : node;
        }
    }
}