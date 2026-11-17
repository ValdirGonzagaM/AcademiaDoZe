using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Application.Seguranca;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Infrastructure;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Xunit;

namespace AcademiaDoZe.Application.Tests;

public sealed class AcessoSegurancaTests : IDisposable
{
    private readonly string _arquivo = Path.Combine(Path.GetTempPath(), $"acessos-{Guid.NewGuid():N}.db");
    private string Conexao => new SqliteConnectionStringBuilder { DataSource = _arquivo, Pooling = false, ForeignKeys = true, DefaultTimeout = 10 }.ToString();
    private readonly SessaoUsuario _sessao = new();
    private readonly Relogio _clock = new();
    private AlunoRepository AlunoRepo() => new(Conexao, AcademiaDoZe.Domain.Enums.DatabaseType.Sqlite);
    private ColaboradorRepository ColabRepo() => new(Conexao, AcademiaDoZe.Domain.Enums.DatabaseType.Sqlite);
    private LogradouroRepository LogRepo() => new(Conexao, AcademiaDoZe.Domain.Enums.DatabaseType.Sqlite);
    private MatriculaRepository MatRepo() => new(Conexao, AcademiaDoZe.Domain.Enums.DatabaseType.Sqlite);
    private AcessoRepository AccessRepo() => new(Conexao);
    private AcessoService Service() => new(AccessRepo(), _sessao, _clock);
    private AutenticacaoService Auth() => new(AlunoRepo(), ColabRepo(), LogRepo(), _sessao);
    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.Today);
    public AcessoSegurancaTests()
    {
        DatabaseInitializer.InitializeSqlite(Conexao);
        _sessao.Iniciar(new(999, PessoaAcesso.Colaborador, "Gestor", "12345678901", PerfilUsuario.Administrador));
    }
    public void Dispose() => File.Delete(_arquivo);
    private async Task<LogradouroDto> Endereco()
    {
        var service = new LogradouroService(LogRepo);
        var atual = (await service.ObterTodosAsync()).FirstOrDefault();
        return atual ?? await service.AdicionarAsync(new() { Cep = "88501000", Nome = "Rua Central", Bairro = "Centro", Cidade = "Lages", Estado = "SC", Pais = "Brasil" });
    }
    private async Task<AlunoDto> Aluno(string cpf = "08615141908", string? email = "aluno@teste.com") =>
        await new AlunoService(AlunoRepo()).AdicionarAsync(new() { Nome = "Valdir Gonzaga", Cpf = cpf, DataNascimento = Hoje.AddYears(-25),
            Telefone = "49999999999", Email = email, Numero = "10", Endereco = await Endereco(), Senha = "Teste123!" });
    private async Task<ColaboradorDto> Colaborador(AppColaboradorVinculo vinculo = AppColaboradorVinculo.CLT, AppColaboradorTipo tipo = AppColaboradorTipo.Atendente, string cpf = "52998224725") =>
        await new ColaboradorService(ColabRepo()).AdicionarAsync(new() { Nome = "Valdir Gonzaga", Cpf = cpf, DataNascimento = Hoje.AddYears(-30),
            Telefone = "49999999999", Email = cpf + "@teste.com", Numero = "10", Endereco = await Endereco(), Senha = "Teste123!",
            DataAdmissao = Hoje, Tipo = tipo, Vinculo = vinculo });
    private Task<MatriculaDto> Matricular(AlunoDto aluno, DateOnly? inicio = null) => new MatriculaService(MatRepo(), AlunoRepo()).AdicionarAsync(new()
    {
        AlunoMatricula = aluno, Plano = AppMatriculaPlano.Mensal, DataInicio = inicio ?? Hoje,
        Objetivo = "Condicionamento", RestricoesMedicas = AppMatriculaRestricoes.None
    });
    [Fact]
    public async Task Aluno_SemMatricula_E_FuturaNaoEntra_ComAtivaEntraESai()
    {
        var aluno = await Aluno();
        var pessoa = await Service().LocalizarAsync(PessoaAcesso.Aluno, "086.151.419-08");
        Assert.Equal(aluno.Id, pessoa.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().RegistrarAsync(PessoaAcesso.Aluno, aluno.Id, true));
        Assert.Empty(await Service().HistoricoAsync(PessoaAcesso.Aluno, aluno.Id));
        var futura = await Matricular(aluno, Hoje.AddDays(1));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().RegistrarAsync(PessoaAcesso.Aluno, aluno.Id, true));
        await new MatriculaService(MatRepo(), AlunoRepo()).RemoverAsync(futura.Id);
        var matricula = await Matricular(aluno);
        var entrada = await Service().RegistrarAsync(PessoaAcesso.Aluno, aluno.Id, true);
        Assert.Null(entrada.Visita.Saida); Assert.Equal(matricula.DataFim, entrada.FimPlano);
        _clock.Avancar(TimeSpan.FromMinutes(95));
        var saida = await Service().RegistrarAsync(PessoaAcesso.Aluno, aluno.Id, false);
        Assert.Equal(entrada.Visita.Id, saida.Visita.Id);
        Assert.Equal(TimeSpan.FromMinutes(95), saida.Permanencia);
        Assert.Equal(saida.Permanencia, saida.TotalDia);
        Assert.Single(await AccessRepo().ConsultarAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().RegistrarAsync(PessoaAcesso.Aluno, aluno.Id, false));
    }
    [Theory]
    [InlineData(AppColaboradorVinculo.CLT, 8)]
    [InlineData(AppColaboradorVinculo.Estagio, 6)]
    public async Task Colaborador_SomaIntervalosDoDia_E_BloqueiaNovaEntradaNoLimite(AppColaboradorVinculo vinculo, int horas)
    {
        var colaborador = await Colaborador(vinculo);
        await Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, true);
        _clock.Avancar(TimeSpan.FromHours(2));
        await Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, false);
        _clock.Avancar(TimeSpan.FromHours(1));
        await Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, true);
        _clock.Avancar(TimeSpan.FromHours(horas - 2));
        var saida = await Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, false);
        Assert.Equal(TimeSpan.FromHours(horas), saida.TotalDia); Assert.Equal(saida.TotalDia, saida.LimiteDia);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, true));
        _clock.Avancar(TimeSpan.FromDays(1));
        Assert.Null((await Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, true)).Visita.Saida);
    }
    [Fact]
    public async Task Saida_E_Permitida_AposVencimentoDoPlanoOuLimiteDeJornada()
    {
        var aluno = await Aluno(); await Matricular(aluno);
        await Service().RegistrarAsync(PessoaAcesso.Aluno, aluno.Id, true);
        _clock.Avancar(TimeSpan.FromDays(45));
        Assert.NotNull((await Service().RegistrarAsync(PessoaAcesso.Aluno, aluno.Id, false)).Visita.Saida);
        var colaborador = await Colaborador();
        await Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, true);
        _clock.Avancar(TimeSpan.FromHours(9));
        var saida = await Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, false);
        Assert.Equal(TimeSpan.FromHours(9), saida.Permanencia);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, true));
    }
    [Fact]
    public async Task EntradasSimultaneas_NaoAbremDuasVisitas()
    {
        var colaborador = await Colaborador();
        async Task<bool> Tentar()
        {
            try { await Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, true); return true; }
            catch (InvalidOperationException) { return false; }
        }
        var respostas = await Task.WhenAll(Task.Run(Tentar), Task.Run(Tentar));
        Assert.Single(respostas, ok => ok);
        Assert.Single(await AccessRepo().ConsultarAsync());
    }
    [Fact]
    public void Jornada_RecortaVisitaNaViradaDoDia_E_ConsideraFuso()
    {
        var fuso = TimeZoneInfo.CreateCustomTimeZone("Teste-Brasil", TimeSpan.FromHours(-3), "Brasil", "Brasil");
        var entrada = new DateTimeOffset(2026, 11, 17, 1, 0, 0, TimeSpan.Zero); // 22h do dia anterior
        var saida = entrada.AddHours(4);
        var visita = new Visita(1, PessoaAcesso.Colaborador, 1, "Pessoa", "08615141908", entrada, saida);
        Assert.Equal(TimeSpan.FromHours(2), Jornada.NoDia([visita], new(2026, 11, 16), saida, fuso));
        Assert.Equal(TimeSpan.FromHours(2), Jornada.NoDia([visita], new(2026, 11, 17), saida, fuso));
    }
    [Fact]
    public async Task Credenciais_E_Permissoes_ImpedemAcessoAOutrosCadastros()
    {
        var aluno = await Aluno(); var outro = await Aluno("52998224725", "outro@teste.com");
        var colaborador = await Colaborador(cpf: "12345678901");
        _sessao.Encerrar();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Auth().EntrarAsync(PessoaAcesso.Aluno, aluno.Cpf, "senha errada"));
        Assert.False(_sessao.Autenticado);
        await Auth().EntrarAsync(PessoaAcesso.Aluno, aluno.Cpf, "Teste123!");
        var alunos = new AlunoService(AlunoRepo(), _sessao);
        Assert.NotNull(await alunos.ObterPorIdAsync(aluno.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => alunos.ObterPorIdAsync(outro.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => alunos.ObterTodosAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().RegistrarAsync(PessoaAcesso.Aluno, outro.Id, true));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new ColaboradorService(ColabRepo(), _sessao).ObterTodosAsync());
        await Auth().TrocarMinhaSenhaAsync("Teste123!", "Nova123!", "Nova123!");
        _sessao.Encerrar();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Auth().EntrarAsync(PessoaAcesso.Aluno, aluno.Cpf, "Teste123!"));
        await Auth().EntrarAsync(PessoaAcesso.Aluno, aluno.Cpf, "Nova123!");
        _sessao.Encerrar(); await Auth().EntrarAsync(PessoaAcesso.Colaborador, colaborador.Cpf, "Teste123!");
        Assert.Equal(PerfilUsuario.Atendente, _sessao.Usuario!.Perfil);
        Assert.NotEmpty(await new AlunoService(AlunoRepo(), _sessao).ObterTodosAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new ColaboradorService(ColabRepo(), _sessao).RemoverAsync(colaborador.Id));
        Assert.Null((await Service().RegistrarAsync(PessoaAcesso.Colaborador, colaborador.Id, true)).Visita.Saida);
    }
    [Theory]
    [InlineData(PerfilUsuario.Administrador, true, true, true)]
    [InlineData(PerfilUsuario.Atendente, true, false, true)]
    [InlineData(PerfilUsuario.Instrutor, false, false, false)]
    [InlineData(PerfilUsuario.Aluno, false, false, false)]
    public void MatrizDePermissoes_SeparaPerfis(PerfilUsuario perfil, bool alunos, bool equipe, bool matriculas)
    {
        _sessao.Iniciar(new(1, perfil == PerfilUsuario.Aluno ? PessoaAcesso.Aluno : PessoaAcesso.Colaborador, "Pessoa", "08615141908", perfil));
        Assert.Equal(alunos, _sessao.Pode(Permissao.Alunos)); Assert.Equal(equipe, _sessao.Pode(Permissao.Colaboradores));
        Assert.Equal(matriculas, _sessao.Pode(Permissao.Matriculas));
        Assert.Equal(perfil != PerfilUsuario.Aluno, _sessao.Pode(Permissao.EntradaAluno));
    }
    [Fact]
    public async Task EnderecoInicial_ExigeAdministradorParaEditarExcluirOuConsultarAposConfiguracao()
    {
        _sessao.Encerrar();
        var enderecos = new LogradouroService(LogRepo, _sessao, ColabRepo());
        var endereco = await enderecos.AdicionarAsync(new() { Cep = "88501000", Nome = "Rua Central", Bairro = "Centro", Cidade = "Lages", Estado = "SC", Pais = "Brasil" });
        Assert.NotEmpty(await enderecos.ObterTodosAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => enderecos.AtualizarAsync(endereco));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => enderecos.RemoverAsync(endereco.Id));
        await Colaborador(tipo: AppColaboradorTipo.Administrador);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => enderecos.ObterTodosAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => enderecos.AdicionarAsync(endereco));
    }
    [Fact]
    public async Task Instrutor_RegistraAlunos_EProprioAcesso_MasNaoAcessaOutrosColaboradoresOuRelatorios()
    {
        var aluno = await Aluno(); await Matricular(aluno);
        var instrutor = await Colaborador(tipo: AppColaboradorTipo.Instrutor);
        var outro = await Colaborador(cpf: "12345678901");
        await Auth().EntrarAsync(PessoaAcesso.Colaborador, instrutor.Cpf, "Teste123!");
        Assert.Equal(aluno.Id, (await Service().LocalizarAsync(PessoaAcesso.Aluno, aluno.Cpf)).Id);
        Assert.Null((await Service().RegistrarAsync(PessoaAcesso.Aluno, aluno.Id, true)).Visita.Saida);
        Assert.Null((await Service().RegistrarAsync(PessoaAcesso.Colaborador, instrutor.Id, true)).Visita.Saida);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().HistoricoAsync(PessoaAcesso.Colaborador, outro.Id));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service().RegistrarAsync(PessoaAcesso.Colaborador, outro.Id, true));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new RelatorioService(AccessRepo(), MatRepo(), _sessao, _clock).GerarAsync(Hoje, Hoje));
    }
    [Fact]
    public async Task ConfiguracaoInicial_NaoPodeSerRepetida()
    {
        var atendente = await Colaborador();
        _sessao.Encerrar(); Assert.True(await Auth().PrecisaAdministradorAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Auth().ConfigurarAdministradorExistenteAsync(atendente.Cpf, "incorreta"));
        await Auth().ConfigurarAdministradorExistenteAsync(atendente.Cpf, "Teste123!");
        Assert.Equal(PerfilUsuario.Administrador, _sessao.Usuario!.Perfil);
        Assert.False(await Auth().PrecisaAdministradorAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => Auth().ConfigurarAdministradorExistenteAsync(atendente.Cpf, "Teste123!"));
    }
    [Fact]
    public async Task ConfiguracaoInicial_NaoPromoveEstagiario()
    {
        var estagiario = await Colaborador(AppColaboradorVinculo.Estagio);
        _sessao.Encerrar();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Auth().ConfigurarAdministradorExistenteAsync(estagiario.Cpf, "Teste123!"));
        Assert.False(await ColabRepo().ExisteAdministradorAsync());
        Assert.Equal(AcademiaDoZe.Domain.Enums.ColaboradorVinculo.Estagiario, (await ColabRepo().ObterPorIdAsync(estagiario.Id))!.Vinculo);
    }
    [Fact]
    public async Task PrimeiroAdministrador_NaoTemSenhaPadrao_NemPermiteDoisBootstraps()
    {
        var endereco = await Endereco();
        _sessao.Encerrar();
        ColaboradorDto Novo(string cpf) => new() { Nome = "Administrador", Cpf = cpf, DataNascimento = Hoje.AddYears(-30),
            Telefone = "49999999999", Email = cpf + "@teste.com", Numero = "1", Endereco = endereco, Senha = "Admin123!",
            DataAdmissao = Hoje, Tipo = AppColaboradorTipo.Administrador, Vinculo = AppColaboradorVinculo.CLT };
        async Task<ColaboradorDto?> Tentar(string cpf)
        {
            try { return await Auth().CriarPrimeiroAdministradorAsync(Novo(cpf)); }
            catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException) { return null; }
        }
        var resultado = await Task.WhenAll(Task.Run(() => Tentar("08615141908")), Task.Run(() => Tentar("52998224725")));
        var admin = Assert.Single(resultado, item => item != null)!;
        Assert.Null(admin.Senha); Assert.True(await ColabRepo().ExisteAdministradorAsync());
        await Auth().EntrarAsync(PessoaAcesso.Colaborador, admin.Cpf, "Admin123!");
        Assert.Equal(PerfilUsuario.Administrador, _sessao.Usuario!.Perfil);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Auth().CriarPrimeiroAdministradorAsync(Novo("12345678901")));
    }
    [Fact]
    public async Task EmailAluno_E_Opcional_SemImpedirMultiplosCadastros()
    {
        var primeiro = await Aluno(email: null); var segundo = await Aluno("52998224725", "");
        Assert.Equal("", (await new AlunoService(AlunoRepo()).ObterPorIdAsync(primeiro.Id))!.Email);
        Assert.Equal(2, await new AlunoService(AlunoRepo()).ContarAsync());
        Assert.NotEqual(primeiro.Id, segundo.Id);
    }
    [Fact]
    public async Task Relatorio_ResumeVisitas_Jornadas_E_Contratos()
    {
        var aluno = await Aluno(); await Matricular(aluno); var funcionario = await Colaborador();
        await Service().RegistrarAsync(PessoaAcesso.Aluno, aluno.Id, true);
        await Service().RegistrarAsync(PessoaAcesso.Colaborador, funcionario.Id, true);
        _clock.Avancar(TimeSpan.FromHours(2));
        await Service().RegistrarAsync(PessoaAcesso.Aluno, aluno.Id, false);
        await Service().RegistrarAsync(PessoaAcesso.Colaborador, funcionario.Id, false);
        var relatorio = await new RelatorioService(AccessRepo(), MatRepo(), _sessao, _clock).GerarAsync(Hoje, Hoje);
        Assert.Equal(1, relatorio.EntradasAlunos); Assert.Equal(1, relatorio.EntradasColaboradores); Assert.Equal(0, relatorio.Presentes);
        Assert.Equal(TimeSpan.FromHours(2), relatorio.MediaPermanencia);
        Assert.Equal(TimeSpan.FromHours(2), Assert.Single(relatorio.Jornadas).Tempo);
        Assert.Equal(1, Assert.Single(relatorio.Contratos).Vigentes);
        Assert.Equal(2, Assert.Single(relatorio.Horarios).Entradas);
    }
    private sealed class Relogio : TimeProvider
    {
        private DateTimeOffset _agora = new(Hoje.ToDateTime(new TimeOnly(8, 0)), TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _agora;
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
        public void Avancar(TimeSpan tempo) => _agora += tempo;
    }
}
