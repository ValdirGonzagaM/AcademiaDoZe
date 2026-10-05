using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Obtém a string de conexão e o tipo de banco das configurações
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não foi encontrada.");

        // Registo dos repositórios injetando a ConnectionString e o DatabaseType
        services.AddScoped<IAlunoRepository>(provider =>
            new AlunoRepository(connectionString, DatabaseType.MySQL));

        services.AddScoped<IColaboradorRepository>(provider =>
            new ColaboradorRepository(connectionString, DatabaseType.MySQL));

        services.AddScoped<IMatriculaRepository>(provider =>
            new MatriculaRepository(connectionString, DatabaseType.MySQL));

        return services;
    }
}