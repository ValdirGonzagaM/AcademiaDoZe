namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfiguracoesPage : ContentPage
{
    public ConfiguracoesPage()
    {
        InitializeComponent();
        IdiomaPicker.SelectedIndex = Array.IndexOf(new[] { "pt-BR", "en-US", "es-ES" }, Localizador.Atual.Cultura.Name);
    }
    private void IdiomaChanged(object? sender, EventArgs e)
    {
        if (IdiomaPicker.SelectedIndex >= 0) Localizador.Atual.Alterar(new[] { "pt-BR", "en-US", "es-ES" }[IdiomaPicker.SelectedIndex]);
    }
    private static void Tema(string tema)
    {
        Preferences.Default.Set("TemaApp", tema);
        if (Microsoft.Maui.Controls.Application.Current != null) Microsoft.Maui.Controls.Application.Current.UserAppTheme = tema switch { "Light" => AppTheme.Light, "Dark" => AppTheme.Dark, _ => AppTheme.Unspecified };
    }
    private void TemaClaro(object? sender, EventArgs e) => Tema("Light");
    private void TemaEscuro(object? sender, EventArgs e) => Tema("Dark");
    private void TemaSistema(object? sender, EventArgs e) => Tema("System");
}
