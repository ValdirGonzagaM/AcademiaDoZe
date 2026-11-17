namespace AcademiaDoZe.Presentation.AppMaui;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly IServiceProvider _services;
    public App(IServiceProvider services)
    {
        _services = services;
        Localizador.Atual.Alterar(Preferences.Default.Get("IdiomaApp", "pt-BR"));
        InitializeComponent();
        UserAppTheme = Preferences.Default.Get("TemaApp", "System") switch { "Dark" => AppTheme.Dark, "Light" => AppTheme.Light, _ => AppTheme.Unspecified };
    }
    protected override Window CreateWindow(IActivationState? activationState) => new(_services.GetRequiredService<AppShell>());
}
