using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.AppMaui;
public partial class MainPage : ContentPage
{
    private readonly ILogradouroService _logradouros;
    private readonly IColaboradorService _colaboradores;
    private readonly IAlunoService _alunos;
    public MainPage(ILogradouroService logradouros, IColaboradorService colaboradores, IAlunoService alunos)
    {
        _logradouros = logradouros; _colaboradores = colaboradores; _alunos = alunos;
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var enderecos = _logradouros.ObterTodosAsync();
            var colaboradores = _colaboradores.ContarAsync();
            var alunos = _alunos.ContarAsync();
            await Task.WhenAll(enderecos, colaboradores, alunos);
            TotalLogradourosLabel.Text = $"{enderecos.Result.Count()} logradouro(s) cadastrado(s)";
            TotalColaboradoresLabel.Text = $"{colaboradores.Result} colaborador(es) cadastrado(s)";
            TotalAlunosLabel.Text = $"{alunos.Result} aluno(s) cadastrado(s)";
        }
        catch (Exception ex)
        {
            TotalLogradourosLabel.Text = "Não foi possível consultar os totais.";
            TotalColaboradoresLabel.Text = TotalAlunosLabel.Text = "Total indisponível";
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
    private async void OnColaboradoresClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//ColaboradorListPage");
    private async void OnAlunosClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//AlunoListPage");
    private async void OnLogradourosClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//LogradouroListPage");
}
