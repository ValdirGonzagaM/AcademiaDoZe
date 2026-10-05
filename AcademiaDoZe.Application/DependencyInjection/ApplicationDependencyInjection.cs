using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAlunoService, AlunoService>();
        services.AddScoped<IColaboradorService, ColaboradorService>();
        services.AddScoped<IMatriculaService, MatriculaService>();

        return services;
    }
}