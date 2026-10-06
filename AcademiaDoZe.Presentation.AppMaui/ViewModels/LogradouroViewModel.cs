using System.Windows.Input;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class LogradouroViewModel
{
    private readonly ILogradouroService _logradouroService;
    private bool _saving;

    public LogradouroDto Logradouro { get; set; } = new()
    {
        Cep = string.Empty,
        Nome = string.Empty,
        Bairro = string.Empty,
        Cidade = string.Empty,
        Estado = string.Empty,
        Pais = string.Empty
    };

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public LogradouroViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;

        SaveCommand = new Command(async () => await OnSaveAsync());
        CancelCommand = new Command(async () => await OnCancelAsync());
    }

    private async Task OnSaveAsync()
    {
        if (_saving) return;
        _saving = true;
        try
        {
            Logradouro.Estado = Logradouro.Estado.Trim().ToUpperInvariant();
            await _logradouroService.AdicionarAsync(Logradouro);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { await Shell.Current.DisplayAlertAsync("Atenção", ex.Message, "OK"); }
        finally { _saving = false; }
    }

    private async Task OnCancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}