using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyTool.DatabaseCategory
{
    /// <summary>
    /// SQL 构建器
    /// 提供流畅的 SQL 语句构建接口
    /// </summary>
    public class SqlBuilder
    {
        private readonly StringBuilder _sql;
        private readonly List<string> _selectColumns;
        private readonly List<string> _fromTables;
        private readonly List<string> _joins;
        private readonly List<string> _whereConditions;
        private readonly List<string> _groupByColumns;
        private readonly List<string> _havingConditions;
        private readonly List<string> _orderByColumns;
        private readonly Dictionary<string, object?> _parameters;
        private string? _insertTable;
        private string? _updateTable;
        private string? _deleteTable;
        private readonly List<string> _insertColumns;
        private readonly List<string> _updateSets;
        private int _skip;
        private int _take;
        private bool _distinct;
        private int _paramIndex;

        /// <summary>
        /// 创建 SQL 构建器
        /// </summary>
        public SqlBuilder()
        {
            _sql = new StringBuilder();
            _selectColumns = new List<string>();
            _fromTables = new List<string>();
            _joins = new List<string>();
            _whereConditions = new List<string>();
            _groupByColumns = new List<string>();
            _havingConditions = new List<string>();
            _orderByColumns = new List<string>();
            _parameters = new Dictionary<string, object?>();
            _insertColumns = new List<string>();
            _updateSets = new List<string>();
            _paramIndex = 0;
        }

        #region SELECT

        /// <summary>
        /// SELECT 语句
        /// </summary>
        /// <param name="columns">列名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Select(params string[] columns)
        {
            _selectColumns.AddRange(columns);
            return this;
        }

        /// <summary>
        /// SELECT DISTINCT
        /// </summary>
        /// <param name="columns">列名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder SelectDistinct(params string[] columns)
        {
            _distinct = true;
            return Select(columns);
        }

        /// <summary>
        /// SELECT COUNT(*)
        /// </summary>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder SelectCount()
        {
            return Select("COUNT(*)");
        }

        /// <summary>
        /// SELECT COUNT(column)
        /// </summary>
        /// <param name="column">列名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder SelectCount(string column)
        {
            return Select($"COUNT({column})");
        }

        #endregion

        #region FROM

        /// <summary>
        /// FROM 语句
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="alias">别名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder From(string table, string? alias = null)
        {
            var from = string.IsNullOrEmpty(alias) ? table : $"{table} AS {alias}";
            _fromTables.Add(from);
            return this;
        }

        /// <summary>
        /// FROM 子查询
        /// </summary>
        /// <param name="subQuery">子查询</param>
        /// <param name="alias">别名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder FromSubQuery(SqlBuilder subQuery, string alias)
        {
            var sql = subQuery.Build();
            foreach (var param in subQuery.GetParameters())
            {
                _parameters[param.Key] = param.Value;
            }
            _fromTables.Add($"({sql}) AS {alias}");
            return this;
        }

        #endregion

        #region JOIN

        /// <summary>
        /// INNER JOIN
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="alias">别名</param>
        /// <param name="on">连接条件</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder InnerJoin(string table, string? alias, string on)
        {
            var join = string.IsNullOrEmpty(alias)
                ? $"INNER JOIN {table} ON {on}"
                : $"INNER JOIN {table} AS {alias} ON {on}";
            _joins.Add(join);
            return this;
        }

        /// <summary>
        /// LEFT JOIN
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="alias">别名</param>
        /// <param name="on">连接条件</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder LeftJoin(string table, string? alias, string on)
        {
            var join = string.IsNullOrEmpty(alias)
                ? $"LEFT JOIN {table} ON {on}"
                : $"LEFT JOIN {table} AS {alias} ON {on}";
            _joins.Add(join);
            return this;
        }

        /// <summary>
        /// RIGHT JOIN
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="alias">别名</param>
        /// <param name="on">连接条件</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder RightJoin(string table, string? alias, string on)
        {
            var join = string.IsNullOrEmpty(alias)
                ? $"RIGHT JOIN {table} ON {on}"
                : $"RIGHT JOIN {table} AS {alias} ON {on}";
            _joins.Add(join);
            return this;
        }

        #endregion

        #region WHERE

        /// <summary>
        /// WHERE 条件
        /// </summary>
        /// <param name="condition">条件</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Where(string condition)
        {
            _whereConditions.Add(condition);
            return this;
        }

        /// <summary>
        /// WHERE 等于条件
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="value">值</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder WhereEquals(string column, object? value)
        {
            var paramName = AddParameter(value);
            return Where($"{column} = {paramName}");
        }

        /// <summary>
        /// WHERE IN 条件
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="values">值集合</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder WhereIn(string column, IEnumerable<object> values)
        {
            var paramNames = values.Select(v => AddParameter(v));
            return Where($"{column} IN ({string.Join(", ", paramNames)})");
        }

        /// <summary>
        /// WHERE BETWEEN 条件
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="start">开始值</param>
        /// <param name="end">结束值</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder WhereBetween(string column, object start, object end)
        {
            var startParam = AddParameter(start);
            var endParam = AddParameter(end);
            return Where($"{column} BETWEEN {startParam} AND {endParam}");
        }

        /// <summary>
        /// WHERE LIKE 条件
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="pattern">模式</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder WhereLike(string column, string pattern)
        {
            var paramName = AddParameter(pattern);
            return Where($"{column} LIKE {paramName}");
        }

        /// <summary>
        /// WHERE IS NULL
        /// </summary>
        /// <param name="column">列名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder WhereIsNull(string column)
        {
            return Where($"{column} IS NULL");
        }

        /// <summary>
        /// WHERE IS NOT NULL
        /// </summary>
        /// <param name="column">列名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder WhereIsNotNull(string column)
        {
            return Where($"{column} IS NOT NULL");
        }

        /// <summary>
        /// AND 条件
        /// </summary>
        /// <param name="condition">条件</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder And(string condition)
        {
            if (_whereConditions.Count > 0)
            {
                _whereConditions[_whereConditions.Count - 1] = $"({string.Join(" AND ", _whereConditions)})";
            }
            _whereConditions.Add(condition);
            return this;
        }

        /// <summary>
        /// OR 条件
        /// </summary>
        /// <param name="condition">条件</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Or(string condition)
        {
            if (_whereConditions.Count > 0)
            {
                _whereConditions[_whereConditions.Count - 1] = $"({string.Join(" OR ", _whereConditions)})";
            }
            _whereConditions.Add(condition);
            return this;
        }

        #endregion

        #region GROUP BY / HAVING

        /// <summary>
        /// GROUP BY
        /// </summary>
        /// <param name="columns">列名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder GroupBy(params string[] columns)
        {
            _groupByColumns.AddRange(columns);
            return this;
        }

        /// <summary>
        /// HAVING 条件
        /// </summary>
        /// <param name="condition">条件</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Having(string condition)
        {
            _havingConditions.Add(condition);
            return this;
        }

        #endregion

        #region ORDER BY

        /// <summary>
        /// ORDER BY 升序
        /// </summary>
        /// <param name="columns">列名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder OrderBy(params string[] columns)
        {
            _orderByColumns.AddRange(columns.Select(c => $"{c} ASC"));
            return this;
        }

        /// <summary>
        /// ORDER BY 降序
        /// </summary>
        /// <param name="columns">列名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder OrderByDescending(params string[] columns)
        {
            _orderByColumns.AddRange(columns.Select(c => $"{c} DESC"));
            return this;
        }

        #endregion

        #region LIMIT / OFFSET

        /// <summary>
        /// LIMIT
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Take(int count)
        {
            _take = count;
            return this;
        }

        /// <summary>
        /// OFFSET
        /// </summary>
        /// <param name="count">偏移量</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Skip(int count)
        {
            _skip = count;
            return this;
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="page">页码（从1开始）</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Page(int page, int pageSize)
        {
            _skip = (page - 1) * pageSize;
            _take = pageSize;
            return this;
        }

        #endregion

        #region INSERT

        /// <summary>
        /// INSERT INTO
        /// </summary>
        /// <param name="table">表名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder InsertInto(string table)
        {
            _insertTable = table;
            return this;
        }

        /// <summary>
        /// 添加列值
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="value">值</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Value(string column, object? value)
        {
            var paramName = AddParameter(value);
            _insertColumns.Add(column);
            return this;
        }

        /// <summary>
        /// 批量添加列值
        /// </summary>
        /// <param name="values">列值字典</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Values(Dictionary<string, object?> values)
        {
            foreach (var kvp in values)
            {
                var paramName = AddParameter(kvp.Value);
                _insertColumns.Add(kvp.Key);
            }
            return this;
        }

        #endregion

        #region UPDATE

        /// <summary>
        /// UPDATE
        /// </summary>
        /// <param name="table">表名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Update(string table)
        {
            _updateTable = table;
            return this;
        }

        /// <summary>
        /// SET 列值
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="value">值</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Set(string column, object? value)
        {
            var paramName = AddParameter(value);
            _updateSets.Add($"{column} = {paramName}");
            return this;
        }

        /// <summary>
        /// 批量 SET
        /// </summary>
        /// <param name="values">列值字典</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder SetMany(Dictionary<string, object?> values)
        {
            foreach (var kvp in values)
            {
                Set(kvp.Key, kvp.Value);
            }
            return this;
        }

        #endregion

        #region DELETE

        /// <summary>
        /// DELETE FROM
        /// </summary>
        /// <param name="table">表名</param>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder DeleteFrom(string table)
        {
            _deleteTable = table;
            return this;
        }

        #endregion

        #region Build

        /// <summary>
        /// 构建 SQL 语句
        /// </summary>
        /// <returns>SQL 字符串</returns>
        public string Build()
        {
            _sql.Clear();

            // INSERT
            if (_insertTable != null)
            {
                BuildInsert();
            }
            // UPDATE
            else if (_updateTable != null)
            {
                BuildUpdate();
            }
            // DELETE
            else if (_deleteTable != null)
            {
                BuildDelete();
            }
            // SELECT
            else
            {
                BuildSelect();
            }

            return _sql.ToString();
        }

        private void BuildSelect()
        {
            _sql.Append("SELECT ");

            if (_distinct)
            {
                _sql.Append("DISTINCT ");
            }

            if (_selectColumns.Count == 0)
            {
                _sql.Append("*");
            }
            else
            {
                _sql.Append(string.Join(", ", _selectColumns));
            }

            if (_fromTables.Count > 0)
            {
                _sql.Append(" FROM ");
                _sql.Append(string.Join(", ", _fromTables));
            }

            if (_joins.Count > 0)
            {
                _sql.Append(" ");
                _sql.Append(string.Join(" ", _joins));
            }

            if (_whereConditions.Count > 0)
            {
                _sql.Append(" WHERE ");
                _sql.Append(string.Join(" AND ", _whereConditions));
            }

            if (_groupByColumns.Count > 0)
            {
                _sql.Append(" GROUP BY ");
                _sql.Append(string.Join(", ", _groupByColumns));
            }

            if (_havingConditions.Count > 0)
            {
                _sql.Append(" HAVING ");
                _sql.Append(string.Join(" AND ", _havingConditions));
            }

            if (_orderByColumns.Count > 0)
            {
                _sql.Append(" ORDER BY ");
                _sql.Append(string.Join(", ", _orderByColumns));
            }

            if (_take > 0)
            {
                _sql.Append($" LIMIT {_take}");
            }

            if (_skip > 0)
            {
                _sql.Append($" OFFSET {_skip}");
            }
        }

        private void BuildInsert()
        {
            var paramNames = _parameters.Keys.Take(_insertColumns.Count).ToList();

            _sql.Append($"INSERT INTO {_insertTable} ");
            _sql.Append($"({string.Join(", ", _insertColumns)}) ");
            _sql.Append($"VALUES ({string.Join(", ", paramNames)})");
        }

        private void BuildUpdate()
        {
            _sql.Append($"UPDATE {_updateTable} ");
            _sql.Append($"SET {string.Join(", ", _updateSets)}");

            if (_whereConditions.Count > 0)
            {
                _sql.Append(" WHERE ");
                _sql.Append(string.Join(" AND ", _whereConditions));
            }
        }

        private void BuildDelete()
        {
            _sql.Append($"DELETE FROM {_deleteTable}");

            if (_whereConditions.Count > 0)
            {
                _sql.Append(" WHERE ");
                _sql.Append(string.Join(" AND ", _whereConditions));
            }
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <returns>参数字典</returns>
        public Dictionary<string, object?> GetParameters()
        {
            return new Dictionary<string, object?>(_parameters);
        }

        private string AddParameter(object? value)
        {
            var paramName = $"@p{_paramIndex++}";
            _parameters[paramName] = value;
            return paramName;
        }

        /// <summary>
        /// 重置构建器
        /// </summary>
        /// <returns>SqlBuilder</returns>
        public SqlBuilder Reset()
        {
            _sql.Clear();
            _selectColumns.Clear();
            _fromTables.Clear();
            _joins.Clear();
            _whereConditions.Clear();
            _groupByColumns.Clear();
            _havingConditions.Clear();
            _orderByColumns.Clear();
            _parameters.Clear();
            _insertColumns.Clear();
            _updateSets.Clear();
            _insertTable = null;
            _updateTable = null;
            _deleteTable = null;
            _skip = 0;
            _take = 0;
            _distinct = false;
            _paramIndex = 0;
            return this;
        }

        #endregion
    }

    /// <summary>
    /// SQL 构建工具类
    /// </summary>
    public static class SqlBuilderUtil
    {
        /// <summary>
        /// 创建 SQL 构建器
        /// </summary>
        /// <returns>SqlBuilder</returns>
        public static SqlBuilder Create()
        {
            return new SqlBuilder();
        }

        /// <summary>
        /// 快速创建 SELECT 查询
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="columns">列名</param>
        /// <returns>SqlBuilder</returns>
        public static SqlBuilder SelectFrom(string table, params string[] columns)
        {
            return new SqlBuilder().Select(columns).From(table);
        }

        /// <summary>
        /// 快速创建 INSERT 语句
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="values">列值字典</param>
        /// <returns>SqlBuilder</returns>
        public static SqlBuilder Insert(string table, Dictionary<string, object?> values)
        {
            return new SqlBuilder().InsertInto(table).Values(values);
        }

        /// <summary>
        /// 快速创建 UPDATE 语句
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="values">列值字典</param>
        /// <param name="where">WHERE 条件</param>
        /// <returns>SqlBuilder</returns>
        public static SqlBuilder Update(string table, Dictionary<string, object?> values, string? where = null)
        {
            var builder = new SqlBuilder().Update(table).SetMany(values);
            if (!string.IsNullOrEmpty(where))
            {
                builder.Where(where);
            }
            return builder;
        }

        /// <summary>
        /// 快速创建 DELETE 语句
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="where">WHERE 条件</param>
        /// <returns>SqlBuilder</returns>
        public static SqlBuilder Delete(string table, string? where = null)
        {
            var builder = new SqlBuilder().DeleteFrom(table);
            if (!string.IsNullOrEmpty(where))
            {
                builder.Where(where);
            }
            return builder;
        }
    }
}
