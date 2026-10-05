using AcademiaDoZe.Presentation.AppMaui.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ConfiguracoesViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string TemaSelecionado { get; set; } = "System";

    [ObservableProperty]
    public partial string ProvedorBanco { get; set; } = "SQLite";

    [RelayCommand]
    private void SalvarTema(string novoTema)
    {
        TemaSelecionado = novoTema;
        Preferences.Default.Set("TemaApp", novoTema);
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage(novoTema));
    }

    [RelayCommand]
    private void SalvarBanco()
    {
        Preferences.Default.Set("ProvedorBanco", ProvedorBanco);
        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage(ProvedorBanco));
    }
}