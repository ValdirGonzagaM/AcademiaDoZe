using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Application.Seguranca;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class AcessoPage : ContentPage
{
    private readonly AcessoService _acessos;
    private readonly SessaoUsuario _sessao;
    private readonly Picker _tipo = new() { Title = "Pessoa", ItemsSource = new[] { "Aluno", "Colaborador" }, SelectedIndex = 0 };
    private readonly Entry _cpf = new() { Placeholder = "CPF completo", Keyboard = Keyboard.Numeric, AutomationId = "CpfAcesso" };
    private readonly Label _pessoa = new() { FontSize = 20, FontAttributes = FontAttributes.Bold, Text = "Busque a pessoa pelo CPF." };
    private readonly Image _foto = new() { HeightRequest = 110, Aspect = Aspect.AspectFit };
    private readonly Label _situacao = new();
    private readonly Label _resultado = new() { FontAttributes = FontAttributes.Bold, AutomationId = "ResultadoAcesso" };
    private readonly Label _tempo = new();
    private readonly VerticalStackLayout _historico = new() { Spacing = 10 };
    private readonly VerticalStackLayout _form = new() { Spacing = 16, Padding = 24, MaximumWidthRequest = 1000 };
    private readonly IDispatcherTimer _timer;
    private PessoaIdentificada? _selecionada;
    private IReadOnlyList<Visita> _visitas = [];
    private bool _ocupado;
    public AcessoPage(AcessoService acessos, SessaoUsuario sessao)
    {
        _acessos = acessos; _sessao = sessao; Title = "Entrada e saída";
        var buscar = new Button { Text = "Buscar CPF", AutomationId = "BuscarPessoaAcesso" };
        buscar.Clicked += async (_, _) => await ExecutarAsync(BuscarAsync);
        _cpf.Completed += async (_, _) => await ExecutarAsync(BuscarAsync);
        var entrada = new Button { Text = "Registrar entrada", AutomationId = "RegistrarEntrada" };
        var saida = new Button { Text = "Registrar saída", AutomationId = "RegistrarSaida", BackgroundColor = Color.FromArgb("#27455B") };
        entrada.Clicked += async (_, _) => await RegistrarAsync(true);
        saida.Clicked += async (_, _) => await RegistrarAsync(false);
        _tipo.SelectedIndexChanged += (_, _) => { _selecionada = null; _visitas = []; _historico.Clear(); AjustarPessoa(); };
        _form.Children.Add(new Label { Text = "CONTROLE DE ACESSO", FontSize = 12, TextColor = Color.FromArgb("#C73A36") });
        _form.Children.Add(new Label { Text = "Cada visita, registrada.", FontSize = 28, FontAttributes = FontAttributes.Bold });
        _form.Children.Add(_tipo); _form.Children.Add(_cpf); _form.Children.Add(buscar);
        _form.Children.Add(new Border { Padding = 20, Content = new VerticalStackLayout { Spacing = 10, Children = { _foto, _pessoa, _situacao, _tempo } } });
        _form.Children.Add(entrada); _form.Children.Add(saida); _form.Children.Add(_resultado);
        _form.Children.Add(new Label { Text = "Histórico de visitas", FontSize = 20, FontAttributes = FontAttributes.Bold });
        _form.Children.Add(_historico); Content = new ScrollView { Content = _form };
        _timer = Dispatcher.CreateTimer(); _timer.Interval = TimeSpan.FromSeconds(1); _timer.Tick += (_, _) => AtualizarTempo();
    }
    private PessoaAcesso Tipo => _tipo.SelectedIndex == 0 ? PessoaAcesso.Aluno : PessoaAcesso.Colaborador;
    private void AjustarPessoa()
    {
        var usuario = _sessao.Usuario;
        if (usuario == null) return;
        var proprio = usuario.Perfil == PerfilUsuario.Aluno || (Tipo == PessoaAcesso.Colaborador && !_sessao.Pode(Permissao.EntradaColaborador));
        _cpf.IsReadOnly = proprio; _cpf.Text = proprio ? usuario.Cpf : "";
        _pessoa.Text = Texto.T("Busque a pessoa pelo CPF."); _situacao.Text = _tempo.Text = "";
        _foto.Source = null; _resultado.Text = "";
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _tipo.IsEnabled = _sessao.Usuario?.Perfil != PerfilUsuario.Aluno;
        _tipo.SelectedIndex = _sessao.Usuario?.Tipo == PessoaAcesso.Colaborador ? 1 : 0;
        AjustarPessoa(); _timer.Start();
        if (_cpf.Text?.Length > 0) await ExecutarAsync(BuscarAsync);
    }
    protected override void OnDisappearing() { _timer.Stop(); base.OnDisappearing(); }
    private async Task BuscarAsync()
    {
        _selecionada = null;
        var pessoa = await _acessos.LocalizarAsync(Tipo, _cpf.Text ?? "");
        _selecionada = pessoa;
        _pessoa.Text = Texto.F("{0}\nCPF: {1} · Nascimento: {2:d}", pessoa.Nome, pessoa.Cpf, pessoa.Nascimento);
        var bytes = string.IsNullOrEmpty(pessoa.Foto) ? null : Convert.FromBase64String(pessoa.Foto);
        _foto.Source = bytes == null ? "logo_aluno.png" : ImageSource.FromStream(() => new MemoryStream(bytes));
        await CarregarHistoricoAsync();
    }
    private async Task CarregarHistoricoAsync()
    {
        if (_selecionada == null) return;
        _visitas = await _acessos.HistoricoAsync(_selecionada.Tipo, _selecionada.Id);
        _historico.Clear();
        foreach (var visita in _visitas.Take(50))
        {
            var texto = Texto.F("Entrada: {0:g}\nSaída: {1}\nPermanência: {2}", visita.Entrada.ToLocalTime(),
                visita.Saida?.ToLocalTime().ToString("g") ?? Texto.T("Em aberto"), Duracao(visita.Permanencia(_acessos.Agora)));
            _historico.Children.Add(new Border { Padding = 16, Content = new Label { Text = texto } });
        }
        if (_visitas.Count == 0) _historico.Children.Add(new Label { Text = Texto.T("Nenhuma visita registrada.") });
        AtualizarTempo();
    }
    private void AtualizarTempo()
    {
        if (_selecionada == null) return;
        var agora = _acessos.Agora; var dia = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(agora, _acessos.Fuso).DateTime);
        var total = Jornada.NoDia(_visitas, dia, agora, _acessos.Fuso);
        var aberta = _visitas.FirstOrDefault(v => v.Saida == null);
        _situacao.Text = Texto.T(aberta == null ? "Fora da academia" : "Na academia");
        _tempo.Text = Texto.F("Tempo acumulado hoje: {0}", Duracao(total));
        if (aberta != null) _tempo.Text += "\n" + Texto.F("Permanência nesta visita: {0}", Duracao(aberta.Permanencia(agora)));
        if (_selecionada.Vinculo is { } vinculo)
        {
            var limite = TimeSpan.FromHours(vinculo == AcademiaDoZe.Domain.Enums.ColaboradorVinculo.CLT ? 8 : 6);
            var saldo = limite - total; if (saldo < TimeSpan.Zero) saldo = TimeSpan.Zero;
            _tempo.Text += "\n" + Texto.F("Limite diário: {0} · Disponível: {1}", Duracao(limite), Duracao(saldo));
            if (total >= limite) _tempo.Text += "\n" + Texto.T("Limite diário atingido. Registre a saída.");
        }
    }
    private Task RegistrarAsync(bool entrada) => ExecutarAsync(async () =>
    {
        if (_selecionada == null || _selecionada.Tipo != Tipo || _selecionada.Cpf != new string((_cpf.Text ?? "").Where(char.IsDigit).ToArray()))
            throw new ArgumentException("Busque a pessoa pelo CPF antes de registrar o acesso.");
        var resultado = await _acessos.RegistrarAsync(Tipo, _selecionada.Id, entrada);
        _resultado.Text = Texto.T(entrada ? "Entrada registrada com sucesso." : "Saída registrada com sucesso.");
        if (resultado.FimPlano is { } fim)
        {
            var hoje = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(_acessos.Agora, _acessos.Fuso).DateTime);
            _resultado.Text += "\n" + Texto.F("Plano válido até {0:d} · {1} dia(s) restante(s)", fim, fim.DayNumber - hoje.DayNumber);
        }
        if (!entrada) _resultado.Text += "\n" + Texto.F("Permanência: {0}\nTotal do dia: {1}", Duracao(resultado.Permanencia), Duracao(resultado.TotalDia));
        await CarregarHistoricoAsync();
        await this.AvisoAsync("Sucesso", _resultado.Text, "OK");
    });
    public static string Duracao(TimeSpan tempo) => $"{(int)tempo.TotalHours:00}:{tempo.Minutes:00}:{tempo.Seconds:00}";
    private async Task ExecutarAsync(Func<Task> acao)
    {
        if (_ocupado) return; _ocupado = true; _form.IsEnabled = false;
        try { await acao(); }
        catch (Exception ex) { _resultado.Text = Texto.T(ex.Message); await this.AvisoAsync("Atenção", ex.Message, "OK"); }
        finally { _ocupado = false; _form.IsEnabled = true; }
    }
}
