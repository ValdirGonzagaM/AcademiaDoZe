using AcademiaDoZe.Application.Seguranca;
using AcademiaDoZe.Presentation.AppMaui.Views;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
    private readonly SessaoUsuario _sessao;
    public AppShell(SessaoUsuario sessao)
    {
        _sessao = sessao;
        InitializeComponent(); Routing.RegisterRoute("logradouro", typeof(LogradouroPage));
        _sessao.Alterada += (_, _) => { if (Dispatcher.IsDispatchRequired) Dispatcher.Dispatch(AtualizarMenus); else AtualizarMenus(); };
        Localizador.Atual.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(Localizador.Cultura)) Localizar(); };
        Navigated += (_, _) => Localizar();
        DescendantAdded += (_, e) => { if (e.Element is IVisualTreeElement elemento) Localizador.Atual.Aplicar(elemento); };
        AtualizarMenus(); Localizar();
    }
    private void AtualizarMenus()
    {
        var logado = _sessao.Autenticado;
        LoginItem.IsVisible = !logado;
        InicioItem.IsVisible = AcessosItem.IsVisible = ContaItem.IsVisible = ConfiguracoesItem.IsVisible = logado;
        ColaboradoresItem.IsVisible = _sessao.Pode(Permissao.Colaboradores);
        AlunosItem.IsVisible = _sessao.Pode(Permissao.Alunos);
        LogradourosItem.IsVisible = _sessao.Pode(Permissao.Logradouros);
        MatriculasItem.IsVisible = _sessao.Pode(Permissao.Matriculas);
        RelatoriosItem.IsVisible = _sessao.Pode(Permissao.Relatorios);
        SairItem.IsEnabled = logado;
        FlyoutBehavior = logado ? FlyoutBehavior.Flyout : FlyoutBehavior.Disabled;
    }
    private void Localizar()
    {
        foreach (var item in Items)
        {
            Localizador.Atual.Vincular(item, BaseShellItem.TitleProperty);
            foreach (var section in item.Items)
                foreach (var content in section.Items) Localizador.Atual.Vincular(content, BaseShellItem.TitleProperty);
        }
        Localizador.Atual.Vincular(SairItem, MenuItem.TextProperty);
        if (CurrentPage != null) Localizador.Atual.Aplicar(CurrentPage);
    }
    private async void OnSairClicked(object? sender, EventArgs e)
    {
        if (!await this.AvisoAsync("Sair da conta", "Deseja encerrar a sessão?", "Sair", "Cancelar")) return;
        _sessao.Encerrar(); await GoToAsync("//LoginPage");
    }
}
