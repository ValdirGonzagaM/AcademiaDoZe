using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Infrastructure;
using AcademiaDoZe.Infrastructure.Repositories;
using AcademiaDoZe.Presentation.AppMaui.Services;
using Microsoft.Data.Sqlite;
using Xunit;

namespace AcademiaDoZe.Application.Tests;

public sealed class MatriculaCrudTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"matricula-{Guid.NewGuid():N}.db");
    private string Connection => new SqliteConnectionStringBuilder { DataSource = _path, ForeignKeys = true, Pooling = false, DefaultTimeout = 10 }.ToString();
    private MatriculaRepository Repository() => new(Connection, DatabaseType.Sqlite);
    private AlunoService Alunos() => new(new AlunoRepository(Connection, DatabaseType.Sqlite));
    private MatriculaService Service() => new(Repository(), new AlunoRepository(Connection, DatabaseType.Sqlite));
    private MatriculaConsulta Consulta() => new(Service(), Alunos());
    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.Today);
    public MatriculaCrudTests() { DatabaseInitializer.InitializeSqlite(Connection); DatabaseInitializer.InitializeSqlite(Connection); }
    public void Dispose() => File.Delete(_path);
    private async Task<AlunoDto> CriarAluno(int idade = 20)
    {
        var logradouros = new LogradouroService(() => new LogradouroRepository(Connection, DatabaseType.Sqlite));
        var endereco = await logradouros.AdicionarAsync(new LogradouroDto { Nome = "Rua Central", Cep = "88501000", Bairro = "Centro", Cidade = "Lages", Estado = "SC", Pais = "Brasil" });
        return await Alunos().AdicionarAsync(new AlunoDto
        {
            Nome = "Valdir Gonzaga", Cpf = "08615141908", DataNascimento = Hoje.AddYears(-idade),
            Telefone = "49999999999", Email = "aluno@teste.com", Endereco = endereco, Numero = "10", Senha = "Teste123!",
            Foto = new ArquivoDto { Conteudo = [255, 216, 255, 1] }
        });
    }
    private static MatriculaDto Nova(AlunoDto aluno) => new()
    {
        AlunoMatricula = aluno, Plano = AppMatriculaPlano.Mensal, DataInicio = Hoje, Objetivo = "Condicionamento",
        RestricoesMedicas = AppMatriculaRestricoes.None
    };

    [Fact]
    public async Task Cadastro_Filtros_EdicaoRestricoes_Laudo_Persistencia_E_Exclusao()
    {
        var aluno = await CriarAluno();
        Assert.Null(aluno.Senha);
        var dto = Nova(aluno); dto.DataFim = Hoje.AddYears(5);
        var salvo = await Service().AdicionarAsync(dto);
        Assert.True(salvo.Id > 0); Assert.Equal(Hoje.AddMonths(1), salvo.DataFim);
        Assert.Equal(1, await Service().ContarAsync());
        Assert.Equal(aluno.Nome, salvo.AlunoMatricula.Nome);
        Assert.Equal(aluno.Foto!.Conteudo, salvo.AlunoMatricula.Foto!.Conteudo);
        foreach (var filtro in new[] { ("Todas", ""), ("ID", salvo.Id.ToString()), ("ID do aluno", aluno.Id.ToString()),
                     ("CPF do aluno", "086.151.419-08"), ("Nome do aluno", "Valdir"), ("Ativas", ""), ("Ativas", aluno.Id.ToString()), ("Plano", "Mensal"), ("Vencendo em dias", "31") })
            Assert.Equal(salvo.Id, Assert.Single(await Consulta().BuscarAsync(filtro.Item1, filtro.Item2)).Id);
        Assert.Empty(await Consulta().BuscarAsync("Vencendo em dias", "0"));
        var edicao = await Service().ObterPorIdAsync(salvo.Id);
        Assert.NotNull(edicao);
        edicao.RestricoesMedicas = AppMatriculaRestricoes.Diabetes | AppMatriculaRestricoes.Labirintite | AppMatriculaRestricoes.Alergias;
        edicao.ObservacoesRestricoes = "Acompanhamento médico";
        await Assert.ThrowsAsync<ArgumentException>(() => Service().AtualizarAsync(edicao));
        edicao.LaudoMedico = new ArquivoDto { Conteudo = [255, 216, 255, 2] };
        await Service().AtualizarAsync(edicao);
        var reaberto = await Service().ObterPorIdAsync(salvo.Id);
        Assert.NotNull(reaberto);
        Assert.Equal(edicao.RestricoesMedicas, reaberto.RestricoesMedicas);
        Assert.Equal(edicao.ObservacoesRestricoes, reaberto.ObservacoesRestricoes);
        Assert.Equal(edicao.LaudoMedico.Conteudo, reaberto.LaudoMedico!.Conteudo);
        Assert.Null(reaberto.AlunoMatricula.Senha);
        var raw = await Repository().ObterPorId(salvo.Id);
        Assert.True(raw!.RestricoesMedicas.HasFlag(MatriculaRestricoes.Alergia));
        Assert.True(raw.RestricoesMedicas.HasFlag(MatriculaRestricoes.Labirintite));
        Assert.Equal(1, await Service().ContarAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => Alunos().RemoverAsync(aluno.Id));
        Assert.True(await Service().RemoverAsync(salvo.Id));
        Assert.Null(await Service().ObterPorIdAsync(salvo.Id));
        Assert.Empty(await Consulta().BuscarAsync("CPF do aluno", aluno.Cpf));
        Assert.Equal(0, await Service().ContarAsync());
        Assert.True(await Alunos().RemoverAsync(aluno.Id));
    }

    [Theory]
    [InlineData(12, false)]
    [InlineData(15, false)]
    [InlineData(16, true)]
    [InlineData(30, true)]
    public async Task Laudo_E_Exigido_ConformeIdadeRealDoAluno(int idade, bool semLaudo)
    {
        var aluno = await CriarAluno(idade);
        var dto = Nova(aluno);
        dto.AlunoMatricula.DataNascimento = Hoje.AddYears(-30); // O serviço consulta o aluno persistido.
        if (semLaudo) Assert.True((await Service().AdicionarAsync(dto)).Id > 0);
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => Service().AdicionarAsync(dto));
            Assert.Equal(0, await Service().ContarAsync());
            dto.LaudoMedico = new ArquivoDto { Conteudo = [1, 2, 3] };
            Assert.True((await Service().AdicionarAsync(dto)).Id > 0);
        }
    }

    [Fact]
    public async Task MatriculaVigenteOuAgendada_ImpedeNova_E_EncerradaPermiteRenovacao()
    {
        var aluno = await CriarAluno();
        var anterior = Nova(aluno); anterior.DataInicio = Hoje.AddMonths(-2);
        await Service().AdicionarAsync(anterior);
        var atual = await Service().AdicionarAsync(Nova(aluno));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().AdicionarAsync(Nova(aluno)));
        await Service().AtualizarAsync(atual);
        anterior.Id = (await Service().ObterTodasAsync()).Single(m => m.Id != atual.Id).Id;
        anterior.DataInicio = Hoje;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().AtualizarAsync(anterior));
        Assert.True(await Service().RemoverAsync(atual.Id));
        var futura = Nova(aluno); futura.DataInicio = Hoje.AddMonths(1);
        await Service().AdicionarAsync(futura);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().AdicionarAsync(Nova(aluno)));
        Assert.Single(await Service().ObterAtivasAsync(aluno.Id));
    }

    [Fact]
    public async Task CadastrosSimultaneos_NaoDuplicamMatriculaVigente()
    {
        var aluno = await CriarAluno();
        async Task<bool> Tentar()
        {
            try { await Service().AdicionarAsync(Nova(aluno)); return true; }
            catch (InvalidOperationException) { return false; }
        }
        var resultados = await Task.WhenAll(Task.Run(Tentar), Task.Run(Tentar));
        Assert.Single(resultados, ok => ok);
        Assert.Equal(1, await Service().ContarAsync());
    }

    [Theory]
    [InlineData(AppMatriculaPlano.Mensal, 2024, 1, 31, 2024, 2, 29)]
    [InlineData(AppMatriculaPlano.Trimestral, 2025, 11, 30, 2026, 2, 28)]
    [InlineData(AppMatriculaPlano.Semestral, 2026, 8, 31, 2027, 2, 28)]
    [InlineData(AppMatriculaPlano.Anual, 2024, 2, 29, 2025, 2, 28)]
    public void Periodo_CalculaTerminoDoPlano(AppMatriculaPlano plano, int ano, int mes, int dia, int anoFim, int mesFim, int diaFim) =>
        Assert.Equal(new DateOnly(anoFim, mesFim, diaFim), MatriculaPeriodo.CalcularFim(plano, new DateOnly(ano, mes, dia)));

    [Fact]
    public void Restricoes_MantemTodasCombinacoesEValoresPersistidos()
    {
        for (var bits = 0; bits <= 63; bits++)
        {
            var restricoes = (AppMatriculaRestricoes)bits;
            Assert.Equal(restricoes, restricoes.ToDomain().ToApplication());
        }
        Assert.Equal(4, (int)MatriculaRestricoes.Alergia);
        Assert.Equal(MatriculaRestricoes.Alergia, AppMatriculaRestricoes.Alergias.ToDomain());
        Assert.Equal(MatriculaRestricoes.Labirintite, AppMatriculaRestricoes.Labirintite.ToDomain());
        Assert.Throws<ArgumentException>(() => ((AppMatriculaRestricoes)128).ToDomain());
    }

    [Fact]
    public async Task ValoresInvalidos_NaoSaoGravados()
    {
        var aluno = await CriarAluno();
        foreach (var filtro in new[] { ("ID", "-1"), ("Vencendo em dias", "-2"), ("CPF do aluno", "123"), ("Plano", "Outro") })
            await Assert.ThrowsAsync<ArgumentException>(() => Consulta().BuscarAsync(filtro.Item1, filtro.Item2));
        var dto = Nova(aluno); dto.Objetivo = "";
        await Assert.ThrowsAsync<ArgumentException>(() => Service().AdicionarAsync(dto));
        dto.Objetivo = "Saúde"; dto.Plano = (AppMatriculaPlano)99;
        await Assert.ThrowsAsync<ArgumentException>(() => Service().AdicionarAsync(dto));
        dto.Plano = AppMatriculaPlano.Mensal; dto.RestricoesMedicas = (AppMatriculaRestricoes)128;
        dto.LaudoMedico = new ArquivoDto { Conteudo = [1, 2] };
        await Assert.ThrowsAsync<ArgumentException>(() => Service().AdicionarAsync(dto));
        Assert.Equal(0, await Service().ContarAsync());
    }
}
