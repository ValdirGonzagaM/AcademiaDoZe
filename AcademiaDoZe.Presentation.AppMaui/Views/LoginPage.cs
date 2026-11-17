using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Presentation.AppMaui.Services;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class LoginPage : ContentPage
{
    private readonly AutenticacaoService _auth;
    private readonly Entry _cpf = new() { Placeholder = "CPF", Keyboard = Keyboard.Numeric, AutomationId = "CpfLogin" };
    private readonly Entry _senha = new() { Placeholder = "Senha", IsPassword = true, AutomationId = "SenhaLogin" };
    private readonly Picker _tipo = new() { Title = "Perfil de acesso", ItemsSource = new[] { "Colaborador", "Aluno" }, SelectedIndex = 0 };
    private readonly VerticalStackLayout _primeiro = new() { Spacing = 12, IsVisible = false };
    private readonly VerticalStackLayout _form = new() { Spacing = 16, Padding = 28, MaximumWidthRequest = 560 };
    private bool _ocupado;
    public LoginPage(AutenticacaoService auth, PessoaCadastro cadastro, FotoService fotos,
        AcademiaDoZe.Application.Interfaces.ILogradouroService logradouros)
    {
        _auth = auth; Title = "Entrar";
        _form.Children.Add(new Image { Source = "logo_aluno.png", HeightRequest = 180, Aspect = Aspect.AspectFit });
        _form.Children.Add(new Label { Text = "Bem-vindo à Academia do Zé", FontSize = 26, FontAttributes = FontAttributes.Bold });
        _form.Children.Add(new Label { Text = "Entre com seu CPF e sua senha para continuar." });
        _form.Children.Add(_tipo); _form.Children.Add(_cpf); _form.Children.Add(_senha);
        var entrar = new Button { Text = "Entrar", AutomationId = "Entrar" };
        entrar.Clicked += async (_, _) => await ExecutarAsync(async () =>
        {
            await _auth.EntrarAsync(_tipo.SelectedIndex == 1 ? PessoaAcesso.Aluno : PessoaAcesso.Colaborador, _cpf.Text ?? "", _senha.Text ?? "");
            _senha.Text = "";
            await Shell.Current.GoToAsync("//MainPage");
        });

        _form.Children.Add(entrar);
        _primeiro.Children.Add(new Label { Text = "Configuração inicial", FontSize = 20, FontAttributes = FontAttributes.Bold });
        _primeiro.Children.Add(new Label { Text = "Cadastre um endereço e o primeiro administrador. Se já possui cadastro CLT, informe seu CPF e senha e use a opção abaixo." });
        var endereco = new Button { Text = "Cadastrar endereço inicial" };
        endereco.Clicked += async (_, _) => await Navigation.PushAsync(new LogradouroListPage(new LogradouroListViewModel(logradouros)));
        var criar = new Button { Text = "Cadastrar primeiro administrador" };
        criar.Clicked += async (_, _) => await Navigation.PushAsync(new PessoaEditPage(cadastro, fotos, true, configuracaoInicial: true));
        var existente = new Button { Text = "Configurar meu cadastro como administrador" };
        existente.Clicked += async (_, _) => await ExecutarAsync(async () =>
        {
            if (!await this.AvisoAsync("Configuração inicial", "Configurar este cadastro CLT como primeiro administrador?", "Confirmar", "Cancelar")) return;
            await _auth.ConfigurarAdministradorExistenteAsync(_cpf.Text ?? "", _senha.Text ?? "");
            _senha.Text = ""; await Shell.Current.GoToAsync("//MainPage");
        });
        _primeiro.Children.Add(endereco); _primeiro.Children.Add(criar); _primeiro.Children.Add(existente);
        _form.Children.Add(_primeiro);
        Content = new ScrollView { Content = _form };
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { _primeiro.IsVisible = await _auth.PrecisaAdministradorAsync(); }
        catch (Exception ex) { await this.AvisoAsync("Atenção", ex.Message, "OK"); }
        Localizador.Atual.Aplicar(this);
    }
    private async Task ExecutarAsync(Func<Task> acao)
    {
        if (_ocupado) return; _ocupado = true; _form.IsEnabled = false;
        try { await acao(); }
        catch (Exception ex) { await this.AvisoAsync("Atenção", ex.Message, "OK"); }
        finally { _ocupado = false; _form.IsEnabled = true; }
    }
}
