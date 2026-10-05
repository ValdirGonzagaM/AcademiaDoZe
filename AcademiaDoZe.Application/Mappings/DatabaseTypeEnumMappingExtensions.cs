using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Infrastructure; // <- mude para Infrastructure se não existir .Data

namespace AcademiaDoZe.Application.Mappings;

public static class DatabaseTypeEnumMappingExtensions
{
    public static DatabaseType ToInfrastructure(this AppDatabaseType appDatabaseType)
    {
        return (DatabaseType)appDatabaseType;
    }

    public static AppDatabaseType ToApplication(this DatabaseType databaseType)
    {
        return (AppDatabaseType)databaseType;
    }
}