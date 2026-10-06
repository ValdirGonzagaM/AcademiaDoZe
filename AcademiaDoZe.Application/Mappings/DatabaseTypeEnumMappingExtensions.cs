using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;
namespace AcademiaDoZe.Application.Mappings;
public static class DatabaseTypeEnumMappingExtensions
{
    public static DatabaseType ToInfrastructure(this AppDatabaseType value) => value switch
    {
        AppDatabaseType.MySql => DatabaseType.MySQL,
        AppDatabaseType.SqlServer => DatabaseType.SqlServer,
        AppDatabaseType.Sqlite => DatabaseType.Sqlite,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };
    public static AppDatabaseType ToApplication(this DatabaseType value) => value switch
    {
        DatabaseType.MySQL => AppDatabaseType.MySql,
        DatabaseType.SqlServer => AppDatabaseType.SqlServer,
        DatabaseType.Sqlite => AppDatabaseType.Sqlite,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };
}
