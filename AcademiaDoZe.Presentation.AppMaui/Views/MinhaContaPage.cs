using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Application.Seguranca;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public sealed class MinhaContaPage : ContentPage
{
    private readonly SessaoUsuario _sessao;
    private readonly IAlunoService _alunos;
    private readonly IColaboradorService _colaboradores;
    private readonly IMatriculaService _matriculas;
    private readonly AcessoService _acessos;
    private readonly VerticalStackLayout _dados = new() { Spacing = 12 };
    public MinhaContaPage(SessaoUsuario sessao, IAlunoService alunos, IColaboradorService colaboradores, IMatriculaService matriculas, AutenticacaoService auth, AcessoService acessos)
    {
        _acessos = acessos; _sessao = sessao; _alunos = alunos; _colaboradores = colaboradores; _matriculas = matriculas; Title = "Minha conta";
        var atual = new Entry { Placeholder = "Senha atual", IsPassword = true };
        var nova = new Entry { Placeholder = "Nova senha", IsPassword = true };
        var confirmar = new Entry { Placeholder = "Confirmar senha", IsPassword = true };
        var salvar = new Button { Text = "Alterar minha senha" };
        salvar.Clicked += async (_, _) =>
        {
            salvar.IsEnabled = false;
            try
            {
                await auth.TrocarMinhaSenhaAsync(atual.Text ?? "", nova.Text ?? "", confirmar.Text ?? "");
                atual.Text = nova.Text = confirmar.Text = "";
                await this.AvisoAsync("Sucesso", "Senha alterada com sucesso.", "OK");
            }
            catch (Exception ex) { await this.AvisoAsync("Atenção", ex.Message, "OK"); }
            finally { salvar.IsEnabled = true; }
        };
        Content = new ScrollView { Content = new VerticalStackLayout { Padding = 24, Spacing = 16, MaximumWidthRequest = 800,
            Children = { _dados, new Label { Text = "Segurança da conta", FontSize = 22, FontAttributes = FontAttributes.Bold }, atual, nova, confirmar, salvar } } };
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing(); _dados.Clear();
        try
        {
            var usuario = _sessao.Usuario ?? throw new UnauthorizedAccessException("Faça login para continuar.");
            AcademiaDoZe.Application.DTOs.PessoaDto? pessoa = usuario.Tipo == PessoaAcesso.Aluno
                ? await _alunos.ObterPorIdAsync(usuario.Id) : await _colaboradores.ObterPorIdAsync(usuario.Id);
            if (pessoa == null) throw new ArgumentException("Cadastro não encontrado.");
            _dados.Add(new Label { Text = pessoa.Nome, FontSize = 28, FontAttributes = FontAttributes.Bold });
            _dados.Add(new Label { Text = Texto.F("CPF: {0}\nNascimento: {1:d}\nTelefone: {2}\nE-mail: {3}", pessoa.Cpf, pessoa.DataNascimento, pessoa.Telefone, pessoa.Email ?? "") });
            if (pessoa.Foto?.Conteudo is { Length: > 0 } bytes) _dados.Add(new Image { HeightRequest = 130, Aspect = Aspect.AspectFit, Source = ImageSource.FromStream(() => new MemoryStream(bytes)) });
            var visitas = await _acessos.HistoricoAsync(usuario.Tipo, usuario.Id);
            var concluidas = visitas.Where(v => v.Saida != null).ToList();
            var total = TimeSpan.FromTicks(visitas.Sum(v => v.Permanencia(_acessos.Agora).Ticks));
            var media = concluidas.Count == 0 ? TimeSpan.Zero : TimeSpan.FromTicks((long)concluidas.Average(v => v.Permanencia(_acessos.Agora).Ticks));
            _dados.Add(new Border { Padding = 16, Content = new Label { Text = Texto.F("Visitas registradas: {0}\nPermanência total: {1}\nMédia das visitas concluídas: {2}", visitas.Count, AcessoPage.Duracao(total), AcessoPage.Duracao(media)) } });
            if (usuario.Tipo == PessoaAcesso.Aluno)
                foreach (var matricula in await _matriculas.ObterPorAlunoIdAsync(usuario.Id))
                    _dados.Add(new Border { Padding = 16, Content = new Label { Text = Texto.F("Contrato: {0:d} a {1:d}\nObjetivo: {2}", matricula.DataInicio, matricula.DataFim, matricula.Objetivo) } });
        }
        catch (Exception ex) { await this.AvisoAsync("Atenção", ex.Message, "OK"); }
        Localizador.Atual.Aplicar(this);
    }
}
