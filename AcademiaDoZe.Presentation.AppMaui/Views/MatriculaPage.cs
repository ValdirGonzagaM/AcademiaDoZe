using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Presentation.AppMaui.Converters;
using AcademiaDoZe.Presentation.AppMaui.Services;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class MatriculaPage : ContentPage
{
    private readonly IMatriculaService _matriculas;
    private readonly IAlunoService _alunos;
    private readonly FotoService _fotos;
    private readonly int _id;
    private AlunoDto? _aluno;
    private byte[]? _laudo;
    private bool _ocupado;
    private readonly Entry _cpf = new() { Placeholder = "CPF do aluno", Keyboard = Keyboard.Numeric, AutomationId = "CpfAlunoMatricula" };
    private readonly Label _alunoDados = new() { Text = "Busque um aluno cadastrado pelo CPF.", AutomationId = "AlunoMatricula" };
    private readonly Image _fotoAluno = new() { HeightRequest = 100, Aspect = Aspect.AspectFit, Source = "logo_aluno.png" };
    private readonly Picker _plano = new() { Title = "Plano", ItemsSource = Enum.GetValues<AppMatriculaPlano>(), ItemDisplayBinding = new Binding(".", converter: new EnumDisplayConverter()), SelectedIndex = 0 };
    private readonly DatePicker _inicio = new() { Date = DateTime.Today, Format = "dd/MM/yyyy", AutomationId = "InicioMatricula" };
    private readonly Label _fim = new() { AutomationId = "FimMatricula" };
    private readonly Entry _objetivo = new() { Placeholder = "Objetivo da matrícula", MaxLength = 200, AutomationId = "ObjetivoMatricula" };
    private readonly Editor _observacoes = new() { Placeholder = "Cuidados e observações sobre a saúde", HeightRequest = 100, MaxLength = 500 };
    private readonly Dictionary<AppMatriculaRestricoes, CheckBox> _restricoes = [];
    private readonly Label _exigencia = new() { FontAttributes = FontAttributes.Bold };
    private readonly Label _statusLaudo = new();
    private readonly Image _imagemLaudo = new() { HeightRequest = 160, Aspect = Aspect.AspectFit, IsVisible = false };
    private readonly Label _erro = new() { TextColor = Color.FromArgb("#A33232") };
    private readonly VerticalStackLayout _form = new() { Padding = DeviceInfo.Idiom == DeviceIdiom.Phone ? 16 : 28, Spacing = 12, MaximumWidthRequest = 960 };

    public MatriculaPage(IMatriculaService matriculas, IAlunoService alunos, FotoService fotos, MatriculaDto? item = null)
    {
        _matriculas = matriculas; _alunos = alunos; _fotos = fotos; _id = item?.Id ?? 0;
        Title = _id == 0 ? "Nova matrícula" : "Editar matrícula";
        var buscar = new Button { Text = "Buscar aluno por CPF", AutomationId = "BuscarAlunoMatricula" };
        buscar.Clicked += async (_, _) => await BuscarAlunoAsync();
        _cpf.Completed += async (_, _) => await BuscarAlunoAsync();
        Campo("Aluno", _cpf); _form.Children.Add(buscar); _form.Children.Add(_fotoAluno); _form.Children.Add(_alunoDados);
        Campo("Plano", _plano); Campo("Data de início", _inicio); Campo("Data final calculada", _fim);
        Campo("Objetivo", _objetivo);
        _form.Children.Add(new Label { Text = "Restrições médicas — marque todas as aplicáveis", FontAttributes = FontAttributes.Bold });
        foreach (var valor in Enum.GetValues<AppMatriculaRestricoes>())
        {
            if (valor == AppMatriculaRestricoes.None) continue;
            var check = new CheckBox { AutomationId = "Restricao" + valor };
            _restricoes.Add(valor, check);
            check.CheckedChanged += (_, _) => AtualizarExigencia();
            _form.Children.Add(new HorizontalStackLayout { Spacing = 8, Children = { check, new Label { Text = valor.GetDisplayName(), VerticalOptions = LayoutOptions.Center } } });
        }
        Campo("Observações de saúde", _observacoes);
        _form.Children.Add(_exigencia); _form.Children.Add(_statusLaudo); _form.Children.Add(_imagemLaudo);
        var camera = new Button { Text = "Fotografar laudo", AutomationId = "CameraLaudo" };
        var galeria = new Button { Text = "Selecionar imagem do laudo", AutomationId = "GaleriaLaudo" };
        var remover = new Button { Text = "Remover laudo" };
        camera.Clicked += async (_, _) => await SelecionarLaudoAsync(true);
        galeria.Clicked += async (_, _) => await SelecionarLaudoAsync(false);
        remover.Clicked += (_, _) => { if (!_ocupado) { _laudo = null; AtualizarLaudo(); } };
        _form.Children.Add(camera); _form.Children.Add(galeria); _form.Children.Add(remover);
        var salvar = new Button { Text = "Salvar matrícula", AutomationId = "SalvarMatricula" };
        salvar.Clicked += async (_, _) => await SalvarAsync();
        var cancelar = new Button { Text = "Cancelar", BackgroundColor = Color.FromArgb("#526469") };
        cancelar.Clicked += async (_, _) => { if (!_ocupado) await Navigation.PopAsync(); };
        _form.Children.Add(_erro); _form.Children.Add(salvar); _form.Children.Add(cancelar);
        _plano.SelectedIndexChanged += (_, _) => AtualizarPeriodo();
        _inicio.DateSelected += (_, _) => AtualizarPeriodo();
        if (item != null)
        {
            _aluno = item.AlunoMatricula; _cpf.Text = _aluno.Cpf; _plano.SelectedItem = item.Plano;
            _inicio.Date = item.DataInicio.ToDateTime(TimeOnly.MinValue); _objetivo.Text = item.Objetivo;
            _observacoes.Text = item.ObservacoesRestricoes; _laudo = item.LaudoMedico?.Conteudo;
            foreach (var par in _restricoes) par.Value.IsChecked = item.RestricoesMedicas.HasFlag(par.Key);
        }
        AtualizarAluno(); AtualizarPeriodo(); AtualizarLaudo();
        Content = new ScrollView { Content = _form };
    }
    private void Campo(string titulo, View campo)
    {
        _form.Children.Add(new Label { Text = titulo, FontAttributes = FontAttributes.Bold }); _form.Children.Add(campo);
    }
    private AppMatriculaRestricoes Selecionadas() => _restricoes.Where(p => p.Value.IsChecked).Aggregate(AppMatriculaRestricoes.None, (flags, p) => flags | p.Key);
    private void AtualizarPeriodo()
    {
        try
        {
            _fim.Text = _inicio.Date is DateTime data && _plano.SelectedItem is AppMatriculaPlano plano
                ? MatriculaPeriodo.CalcularFim(plano, DateOnly.FromDateTime(data)).ToString("dd/MM/yyyy") : "Informe o plano e a data de início.";
        }
        catch (ArgumentException ex) { _fim.Text = ex.Message; }
    }
    private void AtualizarAluno()
    {
        _alunoDados.Text = _aluno == null ? "Busque um aluno cadastrado pelo CPF."
            : $"{_aluno.Nome}\nAluno ID: {_aluno.Id} · CPF: {_aluno.Cpf}\nNascimento: {_aluno.DataNascimento:dd/MM/yyyy}";
        var bytes = _aluno?.Foto?.Conteudo;
        _fotoAluno.Source = bytes is { Length: > 0 } ? ImageSource.FromStream(() => new MemoryStream(bytes)) : "logo_aluno.png";
        AtualizarExigencia();
    }
    private void AtualizarExigencia()
    {
        if (_aluno == null)
        {
            _exigencia.Text = "Selecione o aluno para verificar a exigência de laudo médico.";
            return;
        }
        var menor = _aluno != null && _aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
        _exigencia.Text = menor || Selecionadas() != AppMatriculaRestricoes.None
            ? "Laudo médico obrigatório para este aluno e as restrições selecionadas."
            : "Laudo médico opcional para aluno a partir de 16 anos sem restrições.";
    }
    private void AtualizarLaudo()
    {
        var bytes = _laudo;
        _statusLaudo.Text = bytes is { Length: > 0 } ? "Laudo médico anexado" : "Nenhum laudo anexado";
        _imagemLaudo.IsVisible = bytes is { Length: > 0 };
        _imagemLaudo.Source = bytes is { Length: > 0 } ? ImageSource.FromStream(() => new MemoryStream(bytes)) : null;
    }
    private Task BuscarAlunoAsync() => ExecutarAsync(async () =>
    {
        var cpf = new string((_cpf.Text ?? "").Where(char.IsDigit).ToArray());
        if (cpf.Length != 11) throw new ArgumentException("Informe os 11 dígitos do CPF do aluno.");
        var encontrado = await _alunos.ObterPorCpfAsync(cpf);
        if (_aluno?.Id != encontrado?.Id) { _laudo = null; AtualizarLaudo(); }
        _aluno = encontrado;
        AtualizarAluno();
        if (_aluno == null) throw new ArgumentException("Aluno não encontrado. Cadastre o aluno antes de realizar a matrícula.");
    });
    private Task SelecionarLaudoAsync(bool camera) => ExecutarAsync(async () =>
    {
        var bytes = await _fotos.ObterAsync(camera);
        if (bytes == null) return;
        _laudo = bytes; AtualizarLaudo();
    });
    private Task SalvarAsync() => ExecutarAsync(async () =>
    {
        if (_aluno == null || new string((_cpf.Text ?? "").Where(char.IsDigit).ToArray()) != _aluno.Cpf)
            throw new ArgumentException("Busque o aluno pelo CPF antes de salvar.");
        if (_inicio.Date is not DateTime inicio || _plano.SelectedItem is not AppMatriculaPlano plano)
            throw new ArgumentException("Informe o plano e a data de início.");
        var dto = new MatriculaDto
        {
            Id = _id, AlunoMatricula = _aluno, Plano = plano, DataInicio = DateOnly.FromDateTime(inicio),
            Objetivo = _objetivo.Text ?? "", RestricoesMedicas = Selecionadas(), ObservacoesRestricoes = _observacoes.Text,
            LaudoMedico = _laudo is { Length: > 0 } ? new ArquivoDto { Conteudo = _laudo } : null
        };
        if (_id == 0) await _matriculas.AdicionarAsync(dto); else await _matriculas.AtualizarAsync(dto);
        await DisplayAlertAsync("Sucesso", "Matrícula salva.", "OK");
        await Navigation.PopAsync();
    });
    private async Task ExecutarAsync(Func<Task> acao)
    {
        if (_ocupado) return;
        _ocupado = true; _form.IsEnabled = false; _erro.Text = "";
        try { await acao(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); _erro.Text = ex.Message; await DisplayAlertAsync("Atenção", ex.Message, "OK"); }
        finally { _ocupado = false; _form.IsEnabled = true; }
    }
}
