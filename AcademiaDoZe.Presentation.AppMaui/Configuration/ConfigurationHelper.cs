using AcademiaDoZe.Application;
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;
public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");
        var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = path, ForeignKeys = true, DefaultTimeout = 5
        }.ToString();
        DatabaseInitializer.InitializeSqlite(connectionString);
        services.AddSingleton(new RepositoryConfig { ConnectionString = connectionString, DatabaseType = DatabaseType.Sqlite });
        services.AddTransient<IAlunoRepository>(_ => new AlunoRepository(connectionString, DatabaseType.Sqlite));
        services.AddTransient<IColaboradorRepository>(_ => new ColaboradorRepository(connectionString, DatabaseType.Sqlite));
        services.AddTransient<ILogradouroRepository>(_ => new LogradouroRepository(connectionString, DatabaseType.Sqlite));
        services.AddTransient<Func<ILogradouroRepository>>(provider => () => provider.GetRequiredService<ILogradouroRepository>());
        services.AddTransient<IMatriculaRepository>(_ => new MatriculaRepository(connectionString, DatabaseType.Sqlite));
        services.AddApplicationServices();
    }
}
