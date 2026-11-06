using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Services;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class MatriculaListPage : ContentPage
{
    private readonly IMatriculaService _matriculas;
    private readonly IAlunoService _alunos;
    private readonly FotoService _fotos;
    private readonly MatriculaConsulta _consulta;
    private readonly ObservableCollection<MatriculaDto> _itens = [];
    private readonly Picker _filtro = new() { Title = "Filtro", ItemsSource = new[] { "Todas", "ID", "ID do aluno", "CPF do aluno", "Nome do aluno", "Ativas", "Vencendo em dias", "Plano" }, SelectedIndex = 0 };
    private readonly SearchBar _busca = new() { Placeholder = "Selecione um filtro", AutomationId = "BuscaMatricula" };
    private readonly Label _status = new();
    private readonly ActivityIndicator _atividade = new();
    private bool _ocupado;

    public MatriculaListPage(IMatriculaService matriculas, IAlunoService alunos, FotoService fotos, MatriculaConsulta consulta)
    {
        _matriculas = matriculas; _alunos = alunos; _fotos = fotos; _consulta = consulta;
        Title = "Matrículas";
        var novo = new Button { Text = "+ Nova matrícula", AutomationId = "NovaMatricula" };
        novo.Clicked += async (_, _) => await ExecutarAsync(() => Navigation.PushAsync(new MatriculaPage(_matriculas, _alunos, _fotos)));
        var buscar = new Button { Text = "Buscar", AutomationId = "FiltrarMatricula" };
        buscar.Clicked += async (_, _) => await CarregarAsync();
        _busca.SearchButtonPressed += async (_, _) => await CarregarAsync();
        _filtro.SelectedIndexChanged += (_, _) => _busca.Placeholder = (_filtro.SelectedItem as string) switch
        {
            "Ativas" => "ID do aluno (opcional)", "Vencendo em dias" => "Quantidade de dias (0 = hoje)",
            "CPF do aluno" => "CPF completo", "Plano" => "Mensal, Trimestral, Semestral ou Anual",
            "Nome do aluno" => "Nome do aluno", "Todas" => "Selecione um filtro", _ => "ID numérico"
        };
        var lista = new CollectionView { ItemsSource = _itens, SelectionMode = SelectionMode.None };
        lista.ItemTemplate = new DataTemplate(() =>
        {
            var foto = new Image { WidthRequest = 80, HeightRequest = 80, Aspect = Aspect.AspectFit };
            var aluno = new Label { FontSize = 19, FontAttributes = FontAttributes.Bold };
            var identificacao = new Label(); var periodo = new Label(); var restricoes = new Label(); var observacoes = new Label(); var laudo = new Label();
            var editar = new Button { Text = "Editar", AutomationId = "EditarMatricula" };
            editar.Clicked += async (_, _) =>
            {
                if (editar.BindingContext is MatriculaDto item)
                    await ExecutarAsync(async () =>
                    {
                        var atual = await _matriculas.ObterPorIdAsync(item.Id) ?? throw new KeyNotFoundException("Matrícula não encontrada.");
                        await Navigation.PushAsync(new MatriculaPage(_matriculas, _alunos, _fotos, atual));
                    });
            };
            var excluir = new Button { Text = "Excluir", BackgroundColor = Color.FromArgb("#A33232"), AutomationId = "ExcluirMatricula" };
            excluir.Clicked += async (_, _) =>
            {
                if (excluir.BindingContext is not MatriculaDto item) return;
                await ExecutarAsync(async () =>
                {
                    if (!await DisplayAlertAsync("Excluir matrícula", $"Excluir a matrícula {item.Id} de {item.AlunoMatricula.Nome}?", "Excluir", "Cancelar")) return;
                    if (!await _matriculas.RemoverAsync(item.Id)) throw new KeyNotFoundException("Matrícula não encontrada.");
                    _itens.Remove(item); AtualizarStatus();
                    await DisplayAlertAsync("Sucesso", "Matrícula excluída.", "OK");
                });
            };
            var card = new Border { Padding = 16, Margin = new Thickness(0, 6), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Content = new VerticalStackLayout { Spacing = 8, Children = { foto, aluno, identificacao, periodo, restricoes, observacoes, laudo,
                    new HorizontalStackLayout { Spacing = 12, Children = { editar, excluir } } } } };
            card.BindingContextChanged += (_, _) =>
            {
                if (card.BindingContext is not MatriculaDto item) return;
                aluno.Text = item.AlunoMatricula.Nome;
                identificacao.Text = $"Matrícula: {item.Id} · Aluno: {item.AlunoMatricula.Id}\nCPF: {item.AlunoMatricula.Cpf} · Nascimento: {item.AlunoMatricula.DataNascimento:dd/MM/yyyy}";
                var hoje = DateOnly.FromDateTime(DateTime.Today);
                var situacao = item.DataFim < hoje ? "Encerrada" : item.DataInicio > hoje ? "Agendada" : "Ativa";
                periodo.Text = $"{item.Plano.GetDisplayName()} · {item.DataInicio:dd/MM/yyyy} a {item.DataFim:dd/MM/yyyy} · {situacao}\nObjetivo: {item.Objetivo}";
                restricoes.Text = "Restrições: " + item.RestricoesMedicas.GetDisplayName();
                observacoes.Text = "Observações: " + (string.IsNullOrWhiteSpace(item.ObservacoesRestricoes) ? "Não informadas" : item.ObservacoesRestricoes);
                laudo.Text = item.LaudoMedico?.Conteudo is { Length: > 0 } ? "Laudo médico anexado" : "Sem laudo médico";
                var bytes = item.AlunoMatricula.Foto?.Conteudo;
                foto.Source = bytes is { Length: > 0 } ? ImageSource.FromStream(() => new MemoryStream(bytes)) : "logo_aluno.png";
            };
            return card;
        });
        var header = new VerticalStackLayout { Spacing = 10, Children = { novo, _filtro, _busca, buscar, _atividade, _status } };
        var layout = new Grid { Padding = DeviceInfo.Idiom == DeviceIdiom.Phone ? 16 : 28, MaximumWidthRequest = 1000,
            RowSpacing = 12, RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        layout.Add(header, 0, 0); layout.Add(lista, 0, 1); Content = layout;
    }
    protected override async void OnAppearing() { base.OnAppearing(); await CarregarAsync(); }
    private Task CarregarAsync() => ExecutarAsync(async () =>
    {
        var resultado = await _consulta.BuscarAsync(_filtro.SelectedItem as string ?? "Todas", _busca.Text ?? "");
        _itens.Clear(); foreach (var item in resultado) _itens.Add(item); AtualizarStatus();
    });
    private void AtualizarStatus() => _status.Text = _itens.Count == 0 ? "Nenhuma matrícula encontrada." : $"{_itens.Count} matrícula(s) encontrada(s)";
    private async Task ExecutarAsync(Func<Task> acao)
    {
        if (_ocupado) return;
        _ocupado = true; _atividade.IsRunning = true;
        try { await acao(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); await DisplayAlertAsync("Atenção", ex.Message, "OK"); }
        finally { _ocupado = false; _atividade.IsRunning = false; }
    }
}
