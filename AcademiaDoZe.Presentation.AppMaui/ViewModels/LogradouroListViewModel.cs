using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : ObservableObject
{
    private readonly ILogradouroService _logradouroService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedFilterType = "Nome";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    public string QuantidadeTexto => Texto.F("{0} registro(s) encontrado(s)", Logradouros.Count);
    public string[] Filtros { get; } = ["Nome", "Cep", "Cidade", "Id"];
    public void AtualizarIdioma() => OnPropertyChanged(nameof(QuantidadeTexto));

    public ObservableCollection<LogradouroDto> Logradouros { get; } = new();

    public LogradouroListViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Logradouros.CollectionChanged += (_, _) => AtualizarIdioma();
    }

    [RelayCommand]
    public async Task SearchLogradourosAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            IEnumerable<LogradouroDto> resultados = Enumerable.Empty<LogradouroDto>();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                resultados = await _logradouroService.ObterTodosAsync(cts.Token);
            }
            else if (SelectedFilterType == "Nome")
            {
                resultados = await _logradouroService.ObterPorNomeAsync(SearchText, cts.Token);
            }
            else if (SelectedFilterType == "Cep")
            {
                var cepLimpo = new string(SearchText.Where(char.IsDigit).ToArray());
                if (cepLimpo.Length != 8)
                {
                    await Shell.Current.AvisoAsync("Validação", "Para buscar por CEP, informe um CEP válido com 8 dígitos.", "OK");
                    return;
                }
                var logradouro = await _logradouroService.ObterPorCepAsync(cepLimpo, cts.Token);
                if (logradouro != null)
                    resultados = new[] { logradouro };
            }
            else if (SelectedFilterType == "Cidade")
            {
                resultados = await _logradouroService.ObterPorCidadeAsync(SearchText, cts.Token);
            }
            else if (SelectedFilterType == "Id" && int.TryParse(SearchText, out int id) && id > 0)
            {
                var logradouro = await _logradouroService.ObterPorIdAsync(id, cts.Token);
                if (logradouro != null)
                    resultados = new[] { logradouro };
            }
            else
            {
                await Shell.Current.AvisoAsync("Validação", "Informe um ID numérico maior que zero.", "OK");
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Logradouros.Clear();
                foreach (var logradouro in resultados)
                {
                    Logradouros.Add(logradouro);
                }
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.AvisoAsync("Tempo Esgotado", "A conexão expirou.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.AvisoAsync("Erro", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task LoadLogradourosAsync()
    {
        SearchText = string.Empty;
        await SearchLogradourosAsync();
    }

    [RelayCommand]
    private async Task AddLogradouroAsync()
    {
        if (IsBusy) return;
        await Shell.Current.GoToAsync("logradouro");
    }

    [RelayCommand]
    private async Task EditLogradouroAsync(LogradouroDto? logradouro)
    {
        if (IsBusy || logradouro == null) return;
        await Shell.Current.GoToAsync($"logradouro?id={logradouro.Id}");
    }

    [RelayCommand]
    private async Task DeleteLogradouroAsync(LogradouroDto? logradouro)
    {
        if (IsBusy || logradouro == null) return;
        IsBusy = true;
        try
        {
            bool confirm = await Shell.Current.AvisoAsync("Confirmação", Texto.F("Deseja excluir '{0}'?", logradouro.Nome), "Excluir", "Cancelar");
            if (!confirm) return;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (!await _logradouroService.RemoverAsync(logradouro.Id, cts.Token))
                throw new KeyNotFoundException("O logradouro não foi encontrado. Atualize a busca.");
            Logradouros.Remove(logradouro);
            await Shell.Current.AvisoAsync("Sucesso", "Logradouro excluído!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.AvisoAsync("Atenção", ex.Message, "OK");
        }
        finally { IsBusy = false; }
    }
}
