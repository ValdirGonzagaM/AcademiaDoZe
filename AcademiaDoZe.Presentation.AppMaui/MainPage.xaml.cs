using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.AppMaui;
public partial class MainPage : ContentPage
{
    private readonly ILogradouroService _logradouros;
    public MainPage(ILogradouroService logradouros)
    {
        _logradouros = logradouros;
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var itens = await _logradouros.ObterTodosAsync();
            TotalLogradourosLabel.Text = $"{itens.Count()} logradouro(s) cadastrado(s)";
        }
        catch (Exception ex)
        {
            TotalLogradourosLabel.Text = "Não foi possível consultar o total de logradouros.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
    private async void OnColaboradoresClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//ColaboradorListPage");
    private async void OnAlunosClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//AlunoListPage");
    private async void OnLogradourosClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//LogradouroListPage");
}
