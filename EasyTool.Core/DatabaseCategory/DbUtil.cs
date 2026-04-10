using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.DatabaseCategory
{
    /// <summary>
    /// 数据库工具类
    /// 提供通用的数据库操作方法
    /// </summary>
    public static class DbUtil
    {
        #region 连接管理

        /// <summary>
        /// 创建并打开连接
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="providerFactory">数据库提供者工厂</param>
        /// <returns>数据库连接</returns>
        public static async Task<DbConnection> CreateConnectionAsync(string connectionString, DbProviderFactory providerFactory)
        {
            var connection = providerFactory.CreateConnection()
                ?? throw new InvalidOperationException("无法创建数据库连接");

            connection.ConnectionString = connectionString;
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        /// <summary>
        /// 创建并打开连接（同步）
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="providerFactory">数据库提供者工厂</param>
        /// <returns>数据库连接</returns>
        public static DbConnection CreateConnection(string connectionString, DbProviderFactory providerFactory)
        {
            return CreateConnectionAsync(connectionString, providerFactory).GetAwaiter().GetResult();
        }

        #endregion

        #region 执行查询

        /// <summary>
        /// 执行非查询命令
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>受影响的行数</returns>
        public static async Task<int> ExecuteNonQueryAsync(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null)
        {
            using var command = CreateCommand(connection, sql, parameters, transaction, commandTimeout);
            return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 执行非查询命令（同步）
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null)
        {
            return ExecuteNonQueryAsync(connection, sql, parameters, transaction, commandTimeout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 执行标量查询
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>标量值</returns>
        public static async Task<T?> ExecuteScalarAsync<T>(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null)
        {
            using var command = CreateCommand(connection, sql, parameters, transaction, commandTimeout);
            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            if (result == null || result == DBNull.Value)
                return default;

            return (T)Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// 执行标量查询（同步）
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>标量值</returns>
        public static T? ExecuteScalar<T>(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null)
        {
            return ExecuteScalarAsync<T>(connection, sql, parameters, transaction, commandTimeout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 执行查询并返回数据读取器
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <param name="commandBehavior">命令行为</param>
        /// <returns>数据读取器</returns>
        public static async Task<DbDataReader> ExecuteReaderAsync(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            var command = CreateCommand(connection, sql, parameters, transaction, commandTimeout);
            return await command.ExecuteReaderAsync(commandBehavior).ConfigureAwait(false);
        }

        /// <summary>
        /// 执行查询并返回数据读取器（同步）
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <param name="commandBehavior">命令行为</param>
        /// <returns>数据读取器</returns>
        public static DbDataReader ExecuteReader(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            return ExecuteReaderAsync(connection, sql, parameters, transaction, commandTimeout, commandBehavior).GetAwaiter().GetResult();
        }

        #endregion

        #region 查询映射

        /// <summary>
        /// 执行查询并映射到实体列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>实体列表</returns>
        public static async Task<List<T>> QueryAsync<T>(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null) where T : new()
        {
            var result = new List<T>();

            using var reader = await ExecuteReaderAsync(connection, sql, parameters, transaction, commandTimeout).ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                result.Add(MapToObject<T>(reader));
            }

            return result;
        }

        /// <summary>
        /// 执行查询并映射到实体列表（同步）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>实体列表</returns>
        public static List<T> Query<T>(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null) where T : new()
        {
            return QueryAsync<T>(connection, sql, parameters, transaction, commandTimeout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 执行查询并返回第一个实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>实体</returns>
        public static async Task<T?> QueryFirstOrDefaultAsync<T>(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null) where T : new()
        {
            using var reader = await ExecuteReaderAsync(
                connection, sql, parameters, transaction, commandTimeout,
                CommandBehavior.SingleRow);

            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                return MapToObject<T>(reader);
            }

            return default;
        }

        /// <summary>
        /// 执行查询并返回第一个实体（同步）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>实体</returns>
        public static T? QueryFirstOrDefault<T>(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null) where T : new()
        {
            return QueryFirstOrDefaultAsync<T>(connection, sql, parameters, transaction, commandTimeout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 执行查询并返回单列值列表
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>值列表</returns>
        public static async Task<List<T>> QueryColumnAsync<T>(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null)
        {
            var result = new List<T>();

            using var reader = await ExecuteReaderAsync(connection, sql, parameters, transaction, commandTimeout).ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var value = reader.GetValue(0);
                if (value != null && value != DBNull.Value)
                {
                    result.Add((T)Convert.ChangeType(value, typeof(T)));
                }
            }

            return result;
        }

        #endregion

        #region 批量操作

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="table">表名</param>
        /// <param name="entities">实体列表</param>
        /// <param name="transaction">事务</param>
        /// <param name="batchSize">批次大小</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>插入行数</returns>
        public static async Task<int> BulkInsertAsync<T>(
            DbConnection connection,
            string table,
            IEnumerable<T> entities,
            DbTransaction? transaction = null,
            int batchSize = 1000,
            int? commandTimeout = null) where T : class
        {
            var totalRows = 0;
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var entityList = entities.ToList();

            for (int i = 0; i < entityList.Count; i += batchSize)
            {
                var batch = entityList.Skip(i).Take(batchSize);

                var columns = string.Join(", ", properties.Select(p => p.Name));
                var paramNames = string.Join(", ", properties.Select((p, idx) => $"@p{idx}"));

                var sql = $"INSERT INTO {table} ({columns}) VALUES ({paramNames})";

                foreach (var entity in batch)
                {
                    var parameters = new Dictionary<string, object?>();
                    for (int j = 0; j < properties.Length; j++)
                    {
                        parameters[$"@p{j}"] = properties[j].GetValue(entity);
                    }

                    totalRows += await ExecuteNonQueryAsync(connection, sql, parameters, transaction, commandTimeout).ConfigureAwait(false);
                }
            }

            return totalRows;
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="table">表名</param>
        /// <param name="entities">实体列表</param>
        /// <param name="keyColumn">主键列名</param>
        /// <param name="updateColumns">要更新的列（null 表示更新所有非主键列）</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">命令超时时间</param>
        /// <returns>更新行数</returns>
        public static async Task<int> BulkUpdateAsync<T>(
            DbConnection connection,
            string table,
            IEnumerable<T> entities,
            string keyColumn,
            string[]? updateColumns = null,
            DbTransaction? transaction = null,
            int? commandTimeout = null) where T : class
        {
            var totalRows = 0;
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var columnsToUpdate = updateColumns ?? properties
                .Select(p => p.Name)
                .Where(n => n != keyColumn)
                .ToArray();

            foreach (var entity in entities)
            {
                var setClauses = columnsToUpdate.Select((c, i) => $"{c} = @p{i}");
                var sql = $"UPDATE {table} SET {string.Join(", ", setClauses)} WHERE {keyColumn} = @key";

                var parameters = new Dictionary<string, object?>();
                for (int i = 0; i < columnsToUpdate.Length; i++)
                {
                    var prop = properties.First(p => p.Name == columnsToUpdate[i]);
                    parameters[$"@p{i}"] = prop.GetValue(entity);
                }

                var keyProp = properties.First(p => p.Name == keyColumn);
                parameters["@key"] = keyProp.GetValue(entity);

                totalRows += await ExecuteNonQueryAsync(connection, sql, parameters, transaction, commandTimeout).ConfigureAwait(false);
            }

            return totalRows;
        }

        #endregion

        #region 事务

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="action">事务操作</param>
        /// <param name="isolationLevel">隔离级别</param>
        public static async Task ExecuteTransactionAsync(
            DbConnection connection,
            Func<DbTransaction, Task> action,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using var transaction = await connection.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);

            try
            {
                await action(transaction).ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// 执行事务并返回结果
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="func">事务操作</param>
        /// <param name="isolationLevel">隔离级别</param>
        /// <returns>结果</returns>
        public static async Task<T> ExecuteTransactionAsync<T>(
            DbConnection connection,
            Func<DbTransaction, Task<T>> func,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using var transaction = await connection.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);

            try
            {
                var result = await func(transaction).ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        #endregion

        #region 辅助方法

        private static DbCommand CreateCommand(
            DbConnection connection,
            string sql,
            Dictionary<string, object?>? parameters,
            DbTransaction? transaction,
            int? commandTimeout)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;

            if (commandTimeout.HasValue)
            {
                command.CommandTimeout = commandTimeout.Value;
            }

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = kvp.Key;
                    parameter.Value = kvp.Value ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        private static T MapToObject<T>(DbDataReader reader) where T : new()
        {
            var obj = new T();
            var type = typeof(T);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var property = type.GetProperty(name,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property != null && property.CanWrite)
                {
                    var value = reader.GetValue(i);

                    if (value != null && value != DBNull.Value)
                    {
                        if (property.PropertyType != value.GetType())
                        {
                            value = Convert.ChangeType(value, property.PropertyType);
                        }
                        property.SetValue(obj, value);
                    }
                }
            }

            return obj;
        }

        #endregion
    }
}
