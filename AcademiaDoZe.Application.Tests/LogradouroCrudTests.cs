using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Infrastructure;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Xunit;

namespace AcademiaDoZe.Application.Tests;

public sealed class LogradouroCrudTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"logradouro-{Guid.NewGuid():N}.db");
    private string Connection => new SqliteConnectionStringBuilder { DataSource = _path, ForeignKeys = true, Pooling = false }.ToString();
    private LogradouroService Service() => new(() => new LogradouroRepository(Connection, DatabaseType.Sqlite));
    public LogradouroCrudTests() => DatabaseInitializer.InitializeSqlite(Connection);
    public void Dispose() => File.Delete(_path);
    private static LogradouroDto Novo(string cep = "88501000") => new()
    {
        Cep = cep, Nome = "Valdir Gonzaga", Bairro = "Centro", Cidade = "Lages", Estado = "SC", Pais = "Brasil"
    };

    [Fact]
    public async Task CicloDoVideo_Cadastra_Filtra_EditaBairro_Reabre_E_Exclui()
    {
        var salvo = await Service().AdicionarAsync(Novo());
        Assert.True(salvo.Id > 0);
        Assert.Single(await Service().ObterTodosAsync());
        var filtrado = Assert.Single(await Service().ObterPorNomeAsync(" valdir gonzaga "));
        Assert.Equal(salvo.Id, filtrado.Id);
        Assert.Equal(salvo.Id, (await Service().ObterPorCepAsync("88501-000"))!.Id);
        Assert.Empty(await Service().ObterPorNomeAsync("Nome ausente"));
        filtrado.Bairro = "Abc Bolinhas";
        await Service().AtualizarAsync(filtrado);
        var reaberto = await Service().ObterPorIdAsync(salvo.Id);
        Assert.NotNull(reaberto);
        Assert.Equal("Abc Bolinhas", reaberto.Bairro);
        Assert.Equal("Valdir Gonzaga", reaberto.Nome);
        Assert.Equal(salvo.Id, reaberto.Id);
        Assert.Single(await Service().ObterTodosAsync()); // Editar não criou um segundo registro.
        Assert.Single(await Service().ObterPorBairroAsync("Lages", "Abc Bolinhas"));
        Assert.True(await Service().RemoverAsync(salvo.Id));
        Assert.Null(await Service().ObterPorIdAsync(salvo.Id));
        Assert.Empty(await Service().ObterPorNomeAsync("Valdir Gonzaga"));
        Assert.Empty(await Service().ObterTodosAsync());
        Assert.False(await Service().RemoverAsync(salvo.Id));
    }

    [Fact]
    public async Task Edicao_NaoPermiteCepDeOutroCadastro_NemAlteraRegistroPersistido()
    {
        var original = await Service().AdicionarAsync(Novo());
        await Service().AdicionarAsync(Novo("88502000"));
        original.Cep = "88502000";
        original.Bairro = "Não deve gravar";
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service().AtualizarAsync(original));
        var reaberto = await Service().ObterPorIdAsync(original.Id);
        Assert.Equal("88501000", reaberto!.Cep);
        Assert.Equal("Centro", reaberto.Bairro);
        Assert.Equal(2, (await Service().ObterTodosAsync()).Count());
    }

    [Fact]
    public async Task Exclusao_ProtegeEnderecoVinculado_AAluno()
    {
        var endereco = await Service().AdicionarAsync(Novo());
        var alunos = new AlunoService(new AlunoRepository(Connection, DatabaseType.Sqlite));
        await alunos.AdicionarAsync(new AlunoDto
        {
            Nome = "Aluno de teste", Cpf = "08615141908", DataNascimento = new(2000, 1, 1),
            Telefone = "49999999999", Email = "aluno@teste.com", Numero = "10", Endereco = endereco, Senha = "Teste123!"
        });
        var error = await Assert.ThrowsAsync<InfrastructureException>(() => Service().RemoverAsync(endereco.Id));
        Assert.Equal("LOGRADOURO_EM_USO", error.ErrorCode);
        Assert.NotNull(await Service().ObterPorIdAsync(endereco.Id));
        Assert.Single(await alunos.ObterTodosAsync());
    }
}
