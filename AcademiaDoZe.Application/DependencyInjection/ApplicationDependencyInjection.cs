using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<AcademiaDoZe.Application.Seguranca.SessaoUsuario>();
        services.AddSingleton(TimeProvider.System);
        services.AddTransient<AcessoService>();
        services.AddTransient<RelatorioService>();
        services.AddTransient<AutenticacaoService>();
        services.AddScoped<IAlunoService, AlunoService>();
        services.AddScoped<IColaboradorService, ColaboradorService>();
        services.AddScoped<IMatriculaService, MatriculaService>();

        services.AddScoped<ILogradouroService, LogradouroService>();
        return services;
    }
}