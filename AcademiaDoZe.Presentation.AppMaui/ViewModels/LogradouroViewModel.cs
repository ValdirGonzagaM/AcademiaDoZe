using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroViewModel : ObservableObject
{
    private readonly ILogradouroService _logradouroService;
    private bool _loaded;

    [ObservableProperty]
    private LogradouroDto _logradouro = new()
    {
        Cep = "", Nome = "", Bairro = "", Cidade = "", Estado = "", Pais = "Brasil"
    };

    [ObservableProperty]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;
    public string Titulo => Logradouro.Id == 0 ? "Novo logradouro" : "Editar logradouro";
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand CancelCommand { get; }

    public LogradouroViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        SaveCommand = new AsyncRelayCommand(OnSaveAsync, () => !IsBusy);
        CancelCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync(".."), () => !IsBusy);
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
        SaveCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }

    public async Task CarregarAsync(int id)
    {
        if (_loaded || IsBusy) return;
        IsBusy = true;
        try
        {
            if (id > 0)
                Logradouro = await _logradouroService.ObterPorIdAsync(id)
                    ?? throw new KeyNotFoundException("O logradouro não foi encontrado.");
            _loaded = true;
            OnPropertyChanged(nameof(Titulo));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Atenção", ex.Message, "OK");
            await Shell.Current.GoToAsync("..");
        }
        finally { IsBusy = false; }
    }

    private async Task OnSaveAsync()
    {
        if (IsBusy || !_loaded) return;
        IsBusy = true;
        try
        {
            Logradouro.Estado = Logradouro.Estado.Trim().ToUpperInvariant();
            if (Logradouro.Id == 0)
                await _logradouroService.AdicionarAsync(Logradouro);
            else
                await _logradouroService.AtualizarAsync(Logradouro);
            await Shell.Current.DisplayAlertAsync("Sucesso", "Logradouro salvo com sucesso!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Atenção", ex.Message, "OK"); }
        finally { IsBusy = false; }
    }
}
