using AcademiaDoZe.Presentation.AppMaui.Configuration;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Presentation.AppMaui.Views;
using Microsoft.Extensions.Logging;

namespace AcademiaDoZe.Presentation.AppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        ConfigurationHelper.ConfigureServices(builder.Services);

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AcessoPage>();
        builder.Services.AddTransient<RelatoriosPage>();
        builder.Services.AddTransient<MinhaContaPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MatriculaListPage>();
        builder.Services.AddTransient<Services.MatriculaConsulta>();
        builder.Services.AddTransient<Services.PessoaCadastro>();
        builder.Services.AddSingleton<Services.FotoService>();
        builder.Services.AddTransient<ColaboradorListPage>();
        builder.Services.AddTransient<AlunoListPage>();

#if WINDOWS
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("Idioma", (handler, view) => handler.PlatformView.Language = Localizador.Atual.Cultura.Name);
#endif

        // ViewModels
        builder.Services.AddTransient<DashboardListViewModel>();
        builder.Services.AddTransient<LogradouroListViewModel>();
        builder.Services.AddTransient<LogradouroViewModel>();
        builder.Services.AddTransient<ConfiguracoesPage>();

        // Views
        builder.Services.AddTransient<DashboardListPage>();
        builder.Services.AddTransient<LogradouroListPage>();
        builder.Services.AddTransient<LogradouroPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}