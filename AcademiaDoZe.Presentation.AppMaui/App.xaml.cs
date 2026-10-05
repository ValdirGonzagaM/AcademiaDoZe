using CommunityToolkit.Mvvm.Messaging;
using AcademiaDoZe.Presentation.AppMaui.Messages;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();

        // Escuta a mensagem e altera o tema na hora
        WeakReferenceMessenger.Default.Register<TemaPreferencesUpdatedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UserAppTheme = m.Value switch
                {
                    "Dark" => AppTheme.Dark,
                    "Light" => AppTheme.Light,
                    _ => AppTheme.Unspecified
                };
            });
        });

        MainPage = new AppShell();
    }
}