using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.DataCategory
{
    /// <summary>
    /// SQL 查询构建器
    /// 支持安全的参数化查询，防止 SQL 注入
    /// </summary>
    public class QueryBuilder
    {
        private readonly StringBuilder _sql;
        private readonly Dictionary<string, object> _parameters;
        private readonly List<string> _selectColumns;
        private readonly List<string> _fromTables;
        private readonly List<string> _joinClauses;
        private readonly List<string> _whereConditions;
        private readonly List<string> _groupByColumns;
        private readonly List<string> _havingConditions;
        private readonly List<string> _orderByColumns;
        private string? _limitClause;
        private string? _offsetClause;
        private bool _isDistinct;

        /// <summary>
        /// 获取生成的 SQL
        /// </summary>
        public string Sql => _sql.ToString();

        /// <summary>
        /// 获取参数字典
        /// </summary>
        public Dictionary<string, object> Parameters => new Dictionary<string, object>(_parameters);

        /// <summary>
        /// 创建查询构建器
        /// </summary>
        public QueryBuilder()
        {
            _sql = new StringBuilder();
            _parameters = new Dictionary<string, object>();
            _selectColumns = new List<string>();
            _fromTables = new List<string>();
            _joinClauses = new List<string>();
            _whereConditions = new List<string>();
            _groupByColumns = new List<string>();
            _havingConditions = new List<string>();
            _orderByColumns = new List<string>();
            _isDistinct = false;
        }

        /// <summary>
        /// SELECT 子句
        /// </summary>
        /// <param name="columns">列名</param>
        /// <returns>构建器</returns>
        public QueryBuilder Select(string columns)
        {
            _selectColumns.Add(columns);
            return this;
        }

        /// <summary>
        /// SELECT 子句（多列）
        /// </summary>
        /// <param name="columns">列名数组</param>
        /// <returns>构建器</returns>
        public QueryBuilder Select(params string[] columns)
        {
            foreach (var column in columns)
            {
                _selectColumns.Add(column);
            }
            return this;
        }

        /// <summary>
        /// SELECT DISTINCT 子句
        /// </summary>
        /// <param name="columns">列名</param>
        /// <returns>构建器</returns>
        public QueryBuilder SelectDistinct(string columns)
        {
            _isDistinct = true;
            _selectColumns.Add(columns);
            return this;
        }

        /// <summary>
        /// FROM 子句
        /// </summary>
        /// <param name="table">表名</param>
        /// <returns>构建器</returns>
        public QueryBuilder From(string table)
        {
            _fromTables.Add(table);
            return this;
        }

        /// <summary>
        /// FROM 子句（带别名）
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="alias">别名</param>
        /// <returns>构建器</returns>
        public QueryBuilder From(string table, string alias)
        {
            _fromTables.Add($"{table} AS {alias}");
            return this;
        }

        /// <summary>
        /// JOIN 子句
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="onClause">ON 条件</param>
        /// <returns>构建器</returns>
        public QueryBuilder Join(string table, string onClause)
        {
            _joinClauses.Add($"JOIN {table} ON {onClause}");
            return this;
        }

        /// <summary>
        /// LEFT JOIN 子句
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="onClause">ON 条件</param>
        /// <returns>构建器</returns>
        public QueryBuilder LeftJoin(string table, string onClause)
        {
            _joinClauses.Add($"LEFT JOIN {table} ON {onClause}");
            return this;
        }

        /// <summary>
        /// RIGHT JOIN 子句
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="onClause">ON 条件</param>
        /// <returns>构建器</returns>
        public QueryBuilder RightJoin(string table, string onClause)
        {
            _joinClauses.Add($"RIGHT JOIN {table} ON {onClause}");
            return this;
        }

        /// <summary>
        /// INNER JOIN 子句
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="onClause">ON 条件</param>
        /// <returns>构建器</returns>
        public QueryBuilder InnerJoin(string table, string onClause)
        {
            _joinClauses.Add($"INNER JOIN {table} ON {onClause}");
            return this;
        }

        /// <summary>
        /// WHERE 子句
        /// </summary>
        /// <param name="condition">条件表达式</param>
        /// <returns>构建器</returns>
        public QueryBuilder Where(string condition)
        {
            _whereConditions.Add(condition);
            return this;
        }

        /// <summary>
        /// WHERE 子句（参数化）
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="value">值</param>
        /// <returns>构建器</returns>
        public QueryBuilder Where(string column, object value)
        {
            var paramName = GenerateParamName(column);
            _whereConditions.Add($"{column} = @{paramName}");
            _parameters[paramName] = value;
            return this;
        }

        /// <summary>
        /// WHERE 子句（带操作符）
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="op">操作符</param>
        /// <param name="value">值</param>
        /// <returns>构建器</returns>
        public QueryBuilder Where(string column, string op, object value)
        {
            var paramName = GenerateParamName(column);
            _whereConditions.Add($"{column} {op} @{paramName}");
            _parameters[paramName] = value;
            return this;
        }

        /// <summary>
        /// WHERE IN 子句
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="values">值列表</param>
        /// <returns>构建器</returns>
        public QueryBuilder WhereIn(string column, IEnumerable<object> values)
        {
            var paramNames = new List<string>();
            var index = 0;
            foreach (var value in values)
            {
                var paramName = GenerateParamName(column, index++);
                paramNames.Add($"@{paramName}");
                _parameters[paramName] = value;
            }
            _whereConditions.Add($"{column} IN ({string.Join(", ", paramNames)})");
            return this;
        }

        /// <summary>
        /// WHERE BETWEEN 子句
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="start">起始值</param>
        /// <param name="end">结束值</param>
        /// <returns>构建器</returns>
        public QueryBuilder WhereBetween(string column, object start, object end)
        {
            var paramStart = GenerateParamName(column, 0);
            var paramEnd = GenerateParamName(column, 1);
            _whereConditions.Add($"{column} BETWEEN @{paramStart} AND @{paramEnd}");
            _parameters[paramStart] = start;
            _parameters[paramEnd] = end;
            return this;
        }

        /// <summary>
        /// WHERE LIKE 子句
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="pattern">匹配模式</param>
        /// <returns>构建器</returns>
        public QueryBuilder WhereLike(string column, string pattern)
        {
            var paramName = GenerateParamName(column);
            _whereConditions.Add($"{column} LIKE @{paramName}");
            _parameters[paramName] = pattern;
            return this;
        }

        /// <summary>
        /// WHERE IS NULL 子句
        /// </summary>
        /// <param name="column">列名</param>
        /// <returns>构建器</returns>
        public QueryBuilder WhereIsNull(string column)
        {
            _whereConditions.Add($"{column} IS NULL");
            return this;
        }

        /// <summary>
        /// WHERE IS NOT NULL 子句
        /// </summary>
        /// <param name="column">列名</param>
        /// <returns>构建器</returns>
        public QueryBuilder WhereIsNotNull(string column)
        {
            _whereConditions.Add($"{column} IS NOT NULL");
            return this;
        }

        /// <summary>
        /// AND WHERE 子句
        /// </summary>
        /// <param name="condition">条件表达式</param>
        /// <returns>构建器</returns>
        public QueryBuilder AndWhere(string condition)
        {
            _whereConditions.Add($"AND {condition}");
            return this;
        }

        /// <summary>
        /// OR WHERE 子句
        /// </summary>
        /// <param name="condition">条件表达式</param>
        /// <returns>构建器</returns>
        public QueryBuilder OrWhere(string condition)
        {
            _whereConditions.Add($"OR {condition}");
            return this;
        }

        /// <summary>
        /// GROUP BY 子句
        /// </summary>
        /// <param name="columns">列名</param>
        /// <returns>构建器</returns>
        public QueryBuilder GroupBy(params string[] columns)
        {
            foreach (var column in columns)
            {
                _groupByColumns.Add(column);
            }
            return this;
        }

        /// <summary>
        /// HAVING 子句
        /// </summary>
        /// <param name="condition">条件表达式</param>
        /// <returns>构建器</returns>
        public QueryBuilder Having(string condition)
        {
            _havingConditions.Add(condition);
            return this;
        }

        /// <summary>
        /// ORDER BY 子句
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="direction">排序方向</param>
        /// <returns>构建器</returns>
        public QueryBuilder OrderBy(string column, SortDirection direction = SortDirection.Asc)
        {
            var dir = direction == SortDirection.Asc ? "ASC" : "DESC";
            _orderByColumns.Add($"{column} {dir}");
            return this;
        }

        /// <summary>
        /// ORDER BY ASC 子句
        /// </summary>
        /// <param name="column">列名</param>
        /// <returns>构建器</returns>
        public QueryBuilder OrderByAsc(string column)
        {
            return OrderBy(column, SortDirection.Asc);
        }

        /// <summary>
        /// ORDER BY DESC 子句
        /// </summary>
        /// <param name="column">列名</param>
        /// <returns>构建器</returns>
        public QueryBuilder OrderByDesc(string column)
        {
            return OrderBy(column, SortDirection.Desc);
        }

        /// <summary>
        /// LIMIT 子句
        /// </summary>
        /// <param name="count">限制数量</param>
        /// <returns>构建器</returns>
        public QueryBuilder Limit(int count)
        {
            _limitClause = $"LIMIT {count}";
            return this;
        }

        /// <summary>
        /// OFFSET 子句
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <returns>构建器</returns>
        public QueryBuilder Offset(int offset)
        {
            _offsetClause = $"OFFSET {offset}";
            return this;
        }

        /// <summary>
        /// 分页设置
        /// </summary>
        /// <param name="page">页码（从1开始）</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>构建器</returns>
        public QueryBuilder Page(int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;
            Limit(pageSize);
            Offset(offset);
            return this;
        }

        /// <summary>
        /// 构建完整 SQL
        /// </summary>
        /// <returns>SQL 字符串</returns>
        public string Build()
        {
            _sql.Clear();

            // SELECT
            if (_selectColumns.Count > 0)
            {
                var distinct = _isDistinct ? "DISTINCT " : "";
                _sql.Append($"SELECT {distinct}{string.Join(", ", _selectColumns)}");
            }
            else
            {
                _sql.Append("SELECT *");
            }

            // FROM
            if (_fromTables.Count > 0)
            {
                _sql.Append($" FROM {string.Join(", ", _fromTables)}");
            }

            // JOIN
            if (_joinClauses.Count > 0)
            {
                _sql.Append($" {string.Join(" ", _joinClauses)}");
            }

            // WHERE
            if (_whereConditions.Count > 0)
            {
                _sql.Append($" WHERE {string.Join(" ", _whereConditions)}");
            }

            // GROUP BY
            if (_groupByColumns.Count > 0)
            {
                _sql.Append($" GROUP BY {string.Join(", ", _groupByColumns)}");
            }

            // HAVING
            if (_havingConditions.Count > 0)
            {
                _sql.Append($" HAVING {string.Join(" ", _havingConditions)}");
            }

            // ORDER BY
            if (_orderByColumns.Count > 0)
            {
                _sql.Append($" ORDER BY {string.Join(", ", _orderByColumns)}");
            }

            // LIMIT
            if (_limitClause != null)
            {
                _sql.Append($" {_limitClause}");
            }

            // OFFSET
            if (_offsetClause != null)
            {
                _sql.Append($" {_offsetClause}");
            }

            return _sql.ToString();
        }

        /// <summary>
        /// 构建计数查询
        /// </summary>
        /// <returns>计数 SQL</returns>
        public string BuildCount()
        {
            _sql.Clear();
            _sql.Append("SELECT COUNT(*)");

            if (_fromTables.Count > 0)
            {
                _sql.Append($" FROM {string.Join(", ", _fromTables)}");
            }

            if (_joinClauses.Count > 0)
            {
                _sql.Append($" {string.Join(" ", _joinClauses)}");
            }

            if (_whereConditions.Count > 0)
            {
                _sql.Append($" WHERE {string.Join(" ", _whereConditions)}");
            }

            return _sql.ToString();
        }

        /// <summary>
        /// 构建存在性查询
        /// </summary>
        /// <returns>存在性 SQL</returns>
        public string BuildExists()
        {
            _sql.Clear();
            _sql.Append("SELECT EXISTS(");

            _sql.Append("SELECT 1");

            if (_fromTables.Count > 0)
            {
                _sql.Append($" FROM {string.Join(", ", _fromTables)}");
            }

            if (_joinClauses.Count > 0)
            {
                _sql.Append($" {string.Join(" ", _joinClauses)}");
            }

            if (_whereConditions.Count > 0)
            {
                _sql.Append($" WHERE {string.Join(" ", _whereConditions)}");
            }

            _sql.Append(")");

            return _sql.ToString();
        }

        /// <summary>
        /// 重置构建器
        /// </summary>
        public void Reset()
        {
            _sql.Clear();
            _parameters.Clear();
            _selectColumns.Clear();
            _fromTables.Clear();
            _joinClauses.Clear();
            _whereConditions.Clear();
            _groupByColumns.Clear();
            _havingConditions.Clear();
            _orderByColumns.Clear();
            _limitClause = null;
            _offsetClause = null;
            _isDistinct = false;
        }

        private string GenerateParamName(string column, int index = 0)
        {
            var baseName = column.Replace(".", "_").Replace(" ", "");
            var paramName = $"p_{baseName}_{index}_{_parameters.Count}";
            return paramName;
        }

        /// <summary>
        /// 创建 INSERT 构建器
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="data">插入数据</param>
        /// <returns>INSERT SQL</returns>
        public static (string Sql, Dictionary<string, object> Parameters) BuildInsert(
            string table,
            Dictionary<string, object> data)
        {
            var columns = new List<string>();
            var paramNames = new List<string>();
            var parameters = new Dictionary<string, object>();
            var index = 0;

            foreach (var kvp in data)
            {
                columns.Add(kvp.Key);
                var paramName = $"p_{kvp.Key}_{index}";
                paramNames.Add($"@{paramName}");
                parameters[paramName] = kvp.Value;
                index++;
            }

            var sql = $"INSERT INTO {table} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", paramNames)})";
            return (sql, parameters);
        }

        /// <summary>
        /// 创建 UPDATE 构建器
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="data">更新数据</param>
        /// <param name="whereClause">WHERE 条件（可选）</param>
        /// <param name="whereParams">WHERE 参数（可选）</param>
        /// <returns>UPDATE SQL</returns>
        public static (string Sql, Dictionary<string, object> Parameters) BuildUpdate(
            string table,
            Dictionary<string, object> data,
            string? whereClause = null,
            Dictionary<string, object>? whereParams = null)
        {
            var setClauses = new List<string>();
            var parameters = new Dictionary<string, object>();
            var index = 0;

            foreach (var kvp in data)
            {
                var paramName = $"p_{kvp.Key}_{index}";
                setClauses.Add($"{kvp.Key} = @{paramName}");
                parameters[paramName] = kvp.Value;
                index++;
            }

            var sql = $"UPDATE {table} SET {string.Join(", ", setClauses)}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += $" WHERE {whereClause}";
                if (whereParams != null)
                {
                    foreach (var kvp in whereParams)
                    {
                        parameters[kvp.Key] = kvp.Value;
                    }
                }
            }

            return (sql, parameters);
        }

        /// <summary>
        /// 创建 DELETE 构建器
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="whereClause">WHERE 条件（可选）</param>
        /// <param name="whereParams">WHERE 参数（可选）</param>
        /// <returns>DELETE SQL</returns>
        public static (string Sql, Dictionary<string, object> Parameters) BuildDelete(
            string table,
            string? whereClause = null,
            Dictionary<string, object>? whereParams = null)
        {
            var parameters = new Dictionary<string, object>();
            var sql = $"DELETE FROM {table}";

            if (!string.IsNullOrEmpty(whereClause))
            {
                sql += $" WHERE {whereClause}";
                if (whereParams != null)
                {
                    foreach (var kvp in whereParams)
                    {
                        parameters[kvp.Key] = kvp.Value;
                    }
                }
            }

            return (sql, parameters);
        }
    }

    /// <summary>
    /// 排序方向
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// 升序
        /// </summary>
        Asc,

        /// <summary>
        /// 降序
        /// </summary>
        Desc
    }
}