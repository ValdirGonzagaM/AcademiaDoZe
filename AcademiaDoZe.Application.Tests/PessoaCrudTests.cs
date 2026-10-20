using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Infrastructure;
using AcademiaDoZe.Infrastructure.Repositories;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Application.Mappings;
using Microsoft.Data.Sqlite;
using Xunit;

namespace AcademiaDoZe.Application.Tests;

public sealed class PessoaCrudTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"academia-{Guid.NewGuid():N}.db");
    private string Connection => new SqliteConnectionStringBuilder { DataSource = _path, ForeignKeys = true, Pooling = false }.ToString();
    public PessoaCrudTests() { DatabaseInitializer.InitializeSqlite(Connection); DatabaseInitializer.InitializeSqlite(Connection); }
    public void Dispose() { File.Delete(_path); }

    private async Task<LogradouroDto> EnderecoAsync()
    {
        var service = new LogradouroService(() => new LogradouroRepository(Connection, DatabaseType.Sqlite));
        return await service.AdicionarAsync(new LogradouroDto { Cep = "88501000", Nome = "Rua de teste", Bairro = "Centro", Cidade = "Lages", Estado = "SC", Pais = "Brasil" });
    }
    private PessoaDto Pessoa(bool colaborador, LogradouroDto endereco)
    {
        PessoaDto dto = colaborador
            ? new ColaboradorDto { Nome = "Pessoa teste", Cpf = "086.151.419-08", DataNascimento = new(1995, 1, 1), Telefone = "49999999999", Numero = "10", DataAdmissao = DateOnly.FromDateTime(DateTime.Today), Tipo = AppColaboradorTipo.Atendente, Vinculo = AppColaboradorVinculo.CLT }
            : new AlunoDto { Nome = "Pessoa teste", Cpf = "086.151.419-08", DataNascimento = new(1995, 1, 1), Telefone = "49999999999", Numero = "10" };
        dto.Email = "pessoa@teste.com"; dto.Endereco = endereco; dto.Complemento = "Sala 2"; dto.Senha = "Teste123!";
        dto.Foto = new ArquivoDto { Conteudo = [255, 216, 255, 1] }; return dto;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ciclo_Cadastro_Cpf_Edicao_Foto_Persistencia_Exclusao(bool colaborador)
    {
        var dto = Pessoa(colaborador, await EnderecoAsync());
        var alunos = new AlunoService(new AlunoRepository(Connection, DatabaseType.Sqlite));
        var colaboradores = new ColaboradorService(new ColaboradorRepository(Connection, DatabaseType.Sqlite));
        PessoaDto salvo = colaborador ? await colaboradores.AdicionarAsync((ColaboradorDto)dto) : await alunos.AdicionarAsync((AlunoDto)dto);
        Assert.True(salvo.Id > 0); Assert.Null(salvo.Senha); Assert.Equal("Teste123!", dto.Senha);
        PessoaDto? encontrado = colaborador ? await colaboradores.ObterPorCpfAsync("086.151.419-08") : await alunos.ObterPorCpfAsync("086.151.419-08");
        Assert.NotNull(encontrado); Assert.Equal(dto.Foto!.Conteudo, encontrado.Foto!.Conteudo);
        encontrado.Nome = "zé dos testes"; encontrado.Foto = new ArquivoDto { Conteudo = [255, 216, 255, 2] };
        if (colaborador) await colaboradores.AtualizarAsync((ColaboradorDto)encontrado);
        else await alunos.AtualizarAsync((AlunoDto)encontrado);
        // Uma nova instância abre o mesmo arquivo, sem depender de cache da tela.
        PessoaDto? reaberto = colaborador
            ? await new ColaboradorService(new ColaboradorRepository(Connection, DatabaseType.Sqlite)).ObterPorIdAsync(salvo.Id)
            : await new AlunoService(new AlunoRepository(Connection, DatabaseType.Sqlite)).ObterPorIdAsync(salvo.Id);
        Assert.NotNull(reaberto); Assert.Equal("zé dos testes", reaberto.Nome);
        Assert.Equal(new byte[] { 255, 216, 255, 2 }, reaberto.Foto!.Conteudo);
        Assert.Equal("Sala 2", reaberto.Complemento); Assert.Equal(dto.Endereco!.Id, reaberto.Endereco!.Id);
        var senha = colaborador
            ? (await new ColaboradorRepository(Connection, DatabaseType.Sqlite).ObterPorIdAsync(salvo.Id))!.Senha.Valor
            : (await new AlunoRepository(Connection, DatabaseType.Sqlite).ObterPorIdAsync(salvo.Id))!.Senha.Valor;
        Assert.NotEqual("Teste123!", senha); Assert.True(PasswordHasher.Verify("Teste123!", senha));
        var lista = colaborador ? (IEnumerable<PessoaDto>)await colaboradores.ObterTodosAsync() : await alunos.ObterTodosAsync();
        Assert.Single(lista);
        if (colaborador)
        {
            Assert.Single(await colaboradores.ObterPorTipoAsync(AppColaboradorTipo.Atendente));
            Assert.Empty(await colaboradores.ObterPorTipoAsync(AppColaboradorTipo.Instrutor));
            Assert.Single(await colaboradores.ObterPorVinculoAsync(AppColaboradorVinculo.CLT));
            Assert.True(await colaboradores.RemoverAsync(salvo.Id));
            Assert.Null(await colaboradores.ObterPorCpfAsync(dto.Cpf));
        }
        else
        {
            Assert.Single(await alunos.ObterPorNomeAsync("ZÉ DOS"));
            Assert.True(await alunos.RemoverAsync(salvo.Id));
            Assert.Null(await alunos.ObterPorCpfAsync(dto.Cpf));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rejeita_Cpf_Duplicado_E_Dados_Invalidos(bool colaborador)
    {
        var dto = Pessoa(colaborador, await EnderecoAsync());
        var alunos = new AlunoService(new AlunoRepository(Connection, DatabaseType.Sqlite));
        var colaboradores = new ColaboradorService(new ColaboradorRepository(Connection, DatabaseType.Sqlite));
        async Task Save() { if (colaborador) await colaboradores.AdicionarAsync((ColaboradorDto)dto); else await alunos.AdicionarAsync((AlunoDto)dto); }
        dto.Senha = "123"; await Assert.ThrowsAsync<ArgumentException>(Save);
        dto.Senha = "Teste123!"; await Save();
        await Assert.ThrowsAsync<ArgumentException>(Save);
        dto.Cpf = "12345678901"; dto.Email = "outra@teste.com"; dto.Endereco = null;
        await Assert.ThrowsAsync<ArgumentException>(Save);
        var lista = colaborador ? (IEnumerable<PessoaDto>)await colaboradores.ObterTodosAsync() : await alunos.ObterTodosAsync();
        Assert.Single(lista);
    }

    [Fact]
    public void Provedores_Nao_Dependendem_Da_Ordem_Dos_Enums()
    {
        Assert.Equal(DatabaseType.Sqlite, AppDatabaseType.Sqlite.ToInfrastructure());
        Assert.Equal(DatabaseType.SqlServer, AppDatabaseType.SqlServer.ToInfrastructure());
        Assert.Equal(DatabaseType.MySQL, AppDatabaseType.MySql.ToInfrastructure());
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Fluxo_Compartilhado_Das_Telas_Filtra_Salva_E_Exclui(bool colaborador)
    {
        var cadastro = new AcademiaDoZe.Presentation.AppMaui.Services.PessoaCadastro(
            new AlunoService(new AlunoRepository(Connection, DatabaseType.Sqlite)),
            new ColaboradorService(new ColaboradorRepository(Connection, DatabaseType.Sqlite)),
            new LogradouroService(() => new LogradouroRepository(Connection, DatabaseType.Sqlite)));
        var dto = Pessoa(colaborador, await EnderecoAsync());
        await cadastro.SalvarAsync(dto);
        var encontrado = Assert.Single(await cadastro.BuscarAsync(colaborador, "CPF", dto.Cpf));
        encontrado.Nome = "zé dos testes";
        encontrado.Foto = new ArquivoDto { Conteudo = [255, 216, 255, 3] };
        await cadastro.SalvarAsync(encontrado);
        var editado = Assert.Single(await cadastro.BuscarAsync(colaborador, "Nome", "ZÉ DOS"));
        Assert.Equal("zé dos testes", editado.Nome);
        Assert.Equal(new byte[] { 255, 216, 255, 3 }, editado.Foto!.Conteudo);
        Assert.Empty(await cadastro.BuscarAsync(!colaborador, "CPF", dto.Cpf));
        await Assert.ThrowsAsync<ArgumentException>(() => cadastro.BuscarAsync(colaborador, "CPF", "123"));
        Assert.True(await cadastro.ExcluirAsync(editado));
        Assert.Empty(await cadastro.BuscarAsync(colaborador, "CPF", dto.Cpf));
    }

    [Fact]
    public async Task Colaborador_FiltrosDoSlide_Contagem_E_BuscasDoFormulario()
    {
        var endereco = await EnderecoAsync();
        var alunos = new AlunoService(new AlunoRepository(Connection, DatabaseType.Sqlite));
        var colaboradores = new ColaboradorService(new ColaboradorRepository(Connection, DatabaseType.Sqlite));
        var cadastro = new AcademiaDoZe.Presentation.AppMaui.Services.PessoaCadastro(alunos, colaboradores,
            new LogradouroService(() => new LogradouroRepository(Connection, DatabaseType.Sqlite)));
        Assert.Equal(0, await colaboradores.ContarAsync());
        var primeiro = (ColaboradorDto)Pessoa(true, endereco);
        primeiro.Nome = "José da Equipe";
        var salvo = await cadastro.SalvarAsync(primeiro);
        var segundo = (ColaboradorDto)Pessoa(true, endereco);
        segundo.Cpf = "52998224725"; segundo.Email = "instrutor@teste.com";
        segundo.Tipo = AppColaboradorTipo.Instrutor; segundo.Vinculo = AppColaboradorVinculo.Estagio;
        var instrutor = await cadastro.SalvarAsync(segundo);
        Assert.Equal(2, await colaboradores.ContarAsync());
        Assert.Equal(0, await alunos.ContarAsync());
        foreach (var busca in new[] { ("CPF", "086.151.419-08"), ("ID", salvo.Id.ToString()),
                     ("E-mail", " PESSOA@TESTE.COM "), ("Nome", "jose da equipe"), ("Tipo", "atendente"), ("Vínculo", "clt") })
            Assert.Equal(salvo.Id, Assert.Single(await cadastro.BuscarAsync(true, busca.Item1, busca.Item2)).Id);
        foreach (var termo in new[] { "Estagiário", "estagiario", "Estágio", "estagio" })
            Assert.Equal(instrutor.Id, Assert.Single(await cadastro.BuscarAsync(true, "Vínculo", termo)).Id);
        Assert.Equal(instrutor.Id, Assert.Single(await cadastro.BuscarAsync(true, "Tipo", "instrutor")).Id);
        Assert.Empty(await cadastro.BuscarAsync(true, "Tipo", "administrador"));
        Assert.Equal("Estagiário", AppColaboradorVinculo.Estagio.GetDisplayName());
        var porCpf = Assert.IsType<ColaboradorDto>(await cadastro.PorCpfAsync(true, primeiro.Cpf));
        Assert.Null(porCpf.Senha);
        Assert.Equal(primeiro.Foto!.Conteudo, porCpf.Foto!.Conteudo);
        Assert.Equal(endereco.Id, porCpf.Endereco!.Id);
        var porCep = await cadastro.PorCepAsync("88501-000");
        Assert.NotNull(porCep);
        Assert.Equal(endereco.Id, porCep.Id); Assert.Equal(endereco.Bairro, porCep.Bairro);
        Assert.Null(await cadastro.PorCepAsync("99999000"));
        Assert.Null(await cadastro.PorCpfAsync(true, "12345678901"));
        await Assert.ThrowsAsync<ArgumentException>(() => cadastro.PorCepAsync("123"));
        foreach (var busca in new[] { ("ID", "-1"), ("Tipo", "inexistente"), ("Vínculo", "inexistente"), ("Outro", "valor") })
            await Assert.ThrowsAsync<ArgumentException>(() => cadastro.BuscarAsync(true, busca.Item1, busca.Item2));
        var edicao = Assert.IsType<ColaboradorDto>(await cadastro.PorIdAsync(true, instrutor.Id));
        edicao.Tipo = AppColaboradorTipo.Atendente;
        edicao.Vinculo = AppColaboradorVinculo.CLT;
        edicao.DataAdmissao = new DateOnly(2024, 3, 1);
        await cadastro.SalvarAsync(edicao);
        var reaberto = Assert.IsType<ColaboradorDto>(await cadastro.PorIdAsync(true, instrutor.Id));
        Assert.Equal(AppColaboradorTipo.Atendente, reaberto.Tipo);
        Assert.Equal(AppColaboradorVinculo.CLT, reaberto.Vinculo);
        Assert.Equal(new DateOnly(2024, 3, 1), reaberto.DataAdmissao);
        Assert.Empty(await cadastro.BuscarAsync(true, "Tipo", "instrutor"));
        Assert.Empty(await cadastro.BuscarAsync(true, "Vínculo", "estagiário"));
        Assert.Equal(2, (await cadastro.BuscarAsync(true, "Tipo", "atendente")).Count);
        Assert.True(await cadastro.ExcluirAsync(instrutor));
        Assert.Equal(1, await colaboradores.ContarAsync());
        Assert.True(await cadastro.ExcluirAsync(salvo));
        Assert.Equal(0, await colaboradores.ContarAsync());
    }

    [Theory]
    [InlineData("Teste123!", "Teste123!", false, true)]
    [InlineData("Teste123!", "Diferente123!", false, false)]
    [InlineData("", "", false, false)]
    [InlineData("123", "123", false, false)]
    [InlineData("", "", true, true)]
    [InlineData(null, null, true, true)]
    [InlineData("", "Teste123!", true, false)]
    [InlineData("Teste123!", "", true, false)]
    [InlineData("Teste123!", "Teste123!", true, true)]
    public void ConfirmacaoSenha_ProtegeCadastro_E_PermiteManterSenhaNaEdicao(string? senha, string? confirmacao, bool editando, bool valido)
    {
        void Validar() => AcademiaDoZe.Presentation.AppMaui.Services.PessoaCadastro.ValidarConfirmacaoSenha(senha, confirmacao, editando);
        if (valido) Validar();
        else Assert.Throws<ArgumentException>(Validar);
    }

}
