using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.Services;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public class PessoaListPage : ContentPage
{
    private readonly PessoaCadastro _cadastro;
    private readonly FotoService _fotos;
    private readonly bool _colaborador;
    private readonly ObservableCollection<PessoaDto> _pessoas = [];
    private readonly SearchBar _busca = new() { Placeholder = "Digite o CPF completo", AutomationId = "BuscaPessoa" };
    private readonly Picker _filtro = new() { Title = "Filtrar por", ItemsSource = new[] { "CPF", "Nome" }, SelectedIndex = 0 };
    private readonly Label _status = new() { Text = "Carregando cadastros…" };
    private readonly ActivityIndicator _atividade = new();
    private bool _ocupado;

    protected PessoaListPage(PessoaCadastro cadastro, FotoService fotos, bool colaborador)
    {
        _cadastro = cadastro; _fotos = fotos; _colaborador = colaborador;
        Title = colaborador ? "Colaboradores" : "Alunos";
        var novo = new Button { Text = colaborador ? "+ Novo colaborador" : "+ Novo aluno", AutomationId = "NovoCadastro" };
        novo.Clicked += async (_, _) => await ExecutarAsync(() => Navigation.PushAsync(new PessoaEditPage(_cadastro, _fotos, _colaborador)));
        var buscar = new Button { Text = "Buscar", AutomationId = "FiltrarPessoa" };
        buscar.Clicked += async (_, _) => await CarregarAsync();
        _busca.SearchButtonPressed += async (_, _) => await CarregarAsync();
        _filtro.SelectedIndexChanged += (_, _) => _busca.Placeholder = _filtro.SelectedIndex == 0 ? "Digite o CPF completo" : "Digite parte do nome";
        var lista = new CollectionView { ItemsSource = _pessoas, SelectionMode = SelectionMode.None, AutomationId = "ListaPessoas" };
        lista.ItemTemplate = new DataTemplate(() =>
        {
            var nome = new Label { FontSize = 19, FontAttributes = FontAttributes.Bold };
            nome.SetBinding(Label.TextProperty, nameof(PessoaDto.Nome));
            var cpf = new Label { FontSize = 14 };
            cpf.SetBinding(Label.TextProperty, nameof(PessoaDto.Cpf), stringFormat: "CPF: {0}");
            var foto = new Image { WidthRequest = 72, HeightRequest = 72, Aspect = Aspect.AspectFill };
            var editar = new Button { Text = "Editar", AutomationId = "EditarPessoa" };
            var excluir = new Button { Text = "Excluir", BackgroundColor = Color.FromArgb("#A33232"), AutomationId = "ExcluirPessoa" };
            editar.Clicked += async (_, _) =>
            {
                if (editar.BindingContext is PessoaDto pessoa)
                    await ExecutarAsync(() => Navigation.PushAsync(new PessoaEditPage(_cadastro, _fotos, _colaborador, pessoa)));
            };
            excluir.Clicked += async (_, _) =>
            {
                if (_ocupado || excluir.BindingContext is not PessoaDto pessoa) return;
                if (!await DisplayAlertAsync("Excluir cadastro", $"Excluir {pessoa.Nome} (CPF {pessoa.Cpf})?", "Excluir", "Cancelar")) return;
                await ExecutarAsync(async () =>
                {
                    if (!await _cadastro.ExcluirAsync(pessoa)) throw new InvalidOperationException("Cadastro não encontrado.");
                    _pessoas.Remove(pessoa); AtualizarStatus();
                    await DisplayAlertAsync("Cadastro excluído", "A exclusão foi concluída.", "OK");
                });
            };
            var detalhes = new VerticalStackLayout { Spacing = 5, Children = { nome, cpf } };
            var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 12 };
            grid.Add(foto, 0); grid.Add(detalhes, 1);
            var card = new Border
            {
                Padding = 14, Margin = new Thickness(0, 5), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Content = new VerticalStackLayout { Spacing = 10, Children = { grid, new HorizontalStackLayout { Spacing = 12, Children = { editar, excluir } } } }
            };
            card.BindingContextChanged += (_, _) =>
            {
                var bytes = (card.BindingContext as PessoaDto)?.Foto?.Conteudo;
                foto.Source = bytes is { Length: > 0 } ? ImageSource.FromStream(() => new MemoryStream(bytes)) : "logo_aluno.png";
            };
            return card;
        });
        var header = new VerticalStackLayout { Spacing = 10, Children = { new Label { Text = "PESSOAS · ACADEMIA DO ZÉ", FontSize = 12, TextColor = Color.FromArgb("#147D73") }, novo, _filtro, _busca, buscar, _atividade, _status } };
        var layout = new Grid { Padding = 20, RowSpacing = 10, RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        layout.Add(header, 0, 0); layout.Add(lista, 0, 1); Content = layout;
    }
    protected override async void OnAppearing() { base.OnAppearing(); await CarregarAsync(); }
    private async Task CarregarAsync() => await ExecutarAsync(async () =>
    {
        var pessoas = await _cadastro.BuscarAsync(_colaborador, _filtro.SelectedIndex == 0 ? "CPF" : "Nome", _busca.Text ?? "");
        _pessoas.Clear(); foreach (var pessoa in pessoas) _pessoas.Add(pessoa);
        AtualizarStatus();
    });
    private void AtualizarStatus() => _status.Text = _pessoas.Count == 0 ? "Nenhum cadastro encontrado." : $"{_pessoas.Count} cadastro(s) encontrado(s)";
    private async Task ExecutarAsync(Func<Task> action)
    {
        if (_ocupado) return;
        _ocupado = true; _atividade.IsRunning = true;
        try { await action(); }
        catch (Exception ex) { _status.Text = "Não foi possível concluir a operação."; await DisplayAlertAsync("Atenção", ex.Message, "OK"); }
        finally { _ocupado = false; _atividade.IsRunning = false; }
    }
}
public sealed class ColaboradorListPage(PessoaCadastro cadastro, FotoService fotos) : PessoaListPage(cadastro, fotos, true);
public sealed class AlunoListPage(PessoaCadastro cadastro, FotoService fotos) : PessoaListPage(cadastro, fotos, false);
