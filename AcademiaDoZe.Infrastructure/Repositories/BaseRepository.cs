using AcademiaDoZe.Domain.Enums;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace AcademiaDoZe.Infrastructure.Repositories;

public abstract class BaseRepository
{
    protected string ConnectionString { get; }
    public DatabaseType DatabaseType { get; }

    protected BaseRepository(string connectionString, DatabaseType databaseType)
    {
        ConnectionString = connectionString;
        DatabaseType = databaseType;
    }

    protected async Task<DbCommand> CreateCommandAsync(string query, CancellationToken cancellationToken = default)
    {
        var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        var command = connection.CreateCommand();
        command.CommandText = query;
        return command;
    }

    protected string FormatInsertQuery(string query)
    {
        return $"{query}; SELECT LAST_INSERT_ID();";
    }
}

public static class DbCommandExtensions
{
    public static void AddParameter(this DbCommand command, string name, object value, DbType type)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        param.DbType = type;
        command.Parameters.Add(param);
    }

    public static async Task<int> ExecuteScalarIdAsync(this DbCommand command, string errorCode, string errorMessage, CancellationToken cancellationToken = default)
    {
        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result == null || result == DBNull.Value)
            throw new Exception($"{errorCode}: {errorMessage}");
        return Convert.ToInt32(result);
    }

    public static int GetInt32Value(this DbDataReader reader, string columnName)
    {
        return reader.GetInt32(reader.GetOrdinal(columnName));
    }

    public static string GetStringValue(this DbDataReader reader, string columnName)
    {
        return reader.GetString(reader.GetOrdinal(columnName));
    }

    public static string? GetNullableString(this DbDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    public static DateOnly GetDateOnlyValue(this DbDataReader reader, string columnName)
    {
        return DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal(columnName)));
    }
}