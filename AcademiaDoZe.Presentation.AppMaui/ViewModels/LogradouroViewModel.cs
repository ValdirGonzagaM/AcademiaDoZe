using System.Windows.Input;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class LogradouroViewModel
{
    private readonly ILogradouroService _logradouroService;

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
        // Caso o método no seu ILogradouroService tenha outro nome (ex: SalvarAsync ou IncluirAsync),
        // ajuste apenas o nome da chamada abaixo:
        await _logradouroService.AdicionarAsync(Logradouro);
        await Shell.Current.GoToAsync("..");
    }

    private async Task OnCancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}