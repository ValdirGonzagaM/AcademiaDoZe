using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Application.Seguranca;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class MainPage : ContentPage
{
    private readonly ILogradouroService _logradouros;
    private readonly IMatriculaService _matriculas;
    private readonly IColaboradorService _colaboradores;
    private readonly IAlunoService _alunos;
    private readonly SessaoUsuario _sessao;
    private readonly AcessoService _acessos;
    public MainPage(ILogradouroService logradouros, IColaboradorService colaboradores, IAlunoService alunos,
        IMatriculaService matriculas, SessaoUsuario sessao, AcessoService acessos)
    {
        _logradouros = logradouros; _colaboradores = colaboradores; _alunos = alunos; _matriculas = matriculas; _sessao = sessao; _acessos = acessos;
        InitializeComponent(); SizeChanged += (_, _) => AjustarLayout();
    }
    private void AjustarLayout()
    {
        var compacto = Width < 680;
        HeroGrid.ColumnDefinitions = compacto ? new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star))
            : new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star));
        HeroGrid.RowDefinitions = compacto ? new RowDefinitionCollection(new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto))
            : new RowDefinitionCollection(new RowDefinition(GridLength.Auto));
        Grid.SetColumn(HeroTexto, compacto ? 0 : 1); Grid.SetRow(HeroTexto, compacto ? 1 : 0);
        var cards = new[] { AlunosCard, ColaboradoresCard, MatriculasCard, LogradourosCard }.Where(c => c.IsVisible).ToList();
        MetricasGrid.ColumnDefinitions = compacto ? new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star))
            : new ColumnDefinitionCollection(new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star));
        MetricasGrid.RowDefinitions.Clear();
        for (var i = 0; i < (cards.Count + (compacto ? 0 : 1)) / (compacto ? 1 : 2); i++) MetricasGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        for (var i = 0; i < cards.Count; i++) { Grid.SetColumn(cards[i], compacto ? 0 : i % 2); Grid.SetRow(cards[i], compacto ? i : i / 2); }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AlunosCard.IsVisible = _sessao.Pode(Permissao.Alunos); ColaboradoresCard.IsVisible = _sessao.Pode(Permissao.Colaboradores);
        MatriculasCard.IsVisible = _sessao.Pode(Permissao.Matriculas); LogradourosCard.IsVisible = _sessao.Pode(Permissao.Logradouros);
        RelatoriosButton.IsVisible = _sessao.Pode(Permissao.Relatorios);
        BoasVindasLabel.Text = Texto.F("Bem-vindo, {0}", _sessao.Usuario?.Nome ?? ""); AjustarLayout();
        try
        {
            if (AlunosCard.IsVisible) TotalAlunosLabel.Text = (await _alunos.ContarAsync()).ToString("N0");
            if (ColaboradoresCard.IsVisible) TotalColaboradoresLabel.Text = (await _colaboradores.ContarAsync()).ToString("N0");
            if (MatriculasCard.IsVisible) TotalMatriculasLabel.Text = (await _matriculas.ContarAsync()).ToString("N0");
            if (LogradourosCard.IsVisible) TotalLogradourosLabel.Text = (await _logradouros.ObterTodosAsync()).Count().ToString("N0");
            if (_sessao.Pode(Permissao.Relatorios)) OcupacaoLabel.Text = Texto.F("{0} pessoa(s) presentes", (await _acessos.RelatorioAsync()).Count(v => v.Saida == null));
            else OcupacaoLabel.Text = Texto.T("Sua rotina começa por aqui.");
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); OcupacaoLabel.Text = Texto.T("Não foi possível consultar os totais."); }
    }
    private async void OnAcessosClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//AcessoPage");
    private async void OnContaClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//MinhaContaPage");
    private async void OnRelatoriosClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//RelatoriosPage");
    private async void OnMatriculasClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//MatriculaListPage");
    private async void OnColaboradoresClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//ColaboradorListPage");
    private async void OnAlunosClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//AlunoListPage");
    private async void OnLogradourosClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//LogradouroListPage");
}
