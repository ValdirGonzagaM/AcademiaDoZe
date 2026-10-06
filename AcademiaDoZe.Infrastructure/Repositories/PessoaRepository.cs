using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Infrastructure.Repositories;

public abstract class PessoaRepository<T> : BaseRepository where T : Pessoa
{
    private readonly string _table;
    private readonly string _id;
    private readonly bool _colaborador;
    protected PessoaRepository(string connectionString, DatabaseType databaseType, bool colaborador)
        : base(connectionString, databaseType)
    {
        _colaborador = colaborador;
        var kind = colaborador ? "colaborador" : "aluno";
        _table = "tb_" + kind;
        _id = "id_" + kind;
    }

    protected abstract T Create(int id, string nome, string cpf, DateOnly nascimento,
        string telefone, string email, Logradouro logradouro, string numero,
        string complemento, string senha, Arquivo foto, DbDataReader reader);

    protected async Task<List<T>> ReadAsync(string predicate = "1=1", object? value = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT p.*, l.id_logradouro, l.cep, l.nome AS logradouro_nome,
                   l.bairro, l.cidade, l.estado, l.pais
            FROM {_table} p JOIN tb_logradouro l ON l.id_logradouro = p.logradouro_id
            WHERE {predicate} ORDER BY p.nome
            """;
        if (value != null) command.AddParameter("@Value", value, value is int ? DbType.Int32 : DbType.String);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<T>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var foto = reader.GetNullableString("foto");
            result.Add(Create(reader.GetInt32Value(_id), reader.GetStringValue("nome"),
                reader.GetStringValue("cpf"), reader.GetDateOnlyValue("nascimento"),
                reader.GetStringValue("telefone"), reader.GetStringValue("email"),
                LogradouroRepository.Map(reader, "logradouro_nome"), reader.GetStringValue("numero"),
                reader.GetNullableString("complemento") ?? "", reader.GetStringValue("senha"),
                foto == null ? null! : Arquivo.Criar("foto.jpg", foto), reader));
        }
        return result;
    }

    public async Task<T?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        (await ReadAsync($"p.{_id} = @Value", id, cancellationToken)).FirstOrDefault();
    public async Task<IEnumerable<T>> ObterTodosAsync(CancellationToken cancellationToken = default) =>
        await ReadAsync(cancellationToken: cancellationToken);
    public async Task<T?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default) =>
        (await ReadAsync("p.cpf = @Value", new string(cpf.Where(char.IsDigit).ToArray()), cancellationToken)).FirstOrDefault();
    public async Task<T?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default) =>
        (await ReadAsync("p.email = @Value", email.Trim(), cancellationToken)).FirstOrDefault();
    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default) =>
        (await ObterPorCpfAsync(cpf, cancellationToken)) is { } p && p.Id != id;
    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default) =>
        (await ObterPorEmailAsync(email, cancellationToken)) is { } p && p.Id != id;

    public Task<T> AdicionarAsync(T entity, CancellationToken cancellationToken = default) => SaveAsync(entity, false, cancellationToken);
    public Task<T> AtualizarAsync(T entity, CancellationToken cancellationToken = default) => SaveAsync(entity, true, cancellationToken);
    private async Task<T> SaveAsync(T entity, bool update, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        var columns = "cpf,nome,nascimento,telefone,email,logradouro_id,numero,complemento,senha,foto";
        var parameters = "@Cpf,@Nome,@Nascimento,@Telefone,@Email,@LogradouroId,@Numero,@Complemento,@Senha,@Foto";
        if (_colaborador) { columns += ",admissao,tipo,vinculo"; parameters += ",@Admissao,@Tipo,@Vinculo"; }
        command.CommandText = update
            ? $"UPDATE {_table} SET {string.Join(",", columns.Split(',').Zip(parameters.Split(','), (c,p) => c + "=" + p))} WHERE {_id} = @Id"
            : FormatInsertQuery($"INSERT INTO {_table} ({columns}) VALUES ({parameters})");
        command.AddParameter("@Id", entity.Id, DbType.Int32);
        command.AddParameter("@Cpf", entity.Cpf.Valor, DbType.String);
        command.AddParameter("@Nome", entity.Nome, DbType.String);
        command.AddParameter("@Nascimento", entity.DataNascimento, DbType.Date);
        command.AddParameter("@Telefone", entity.Telefone.Valor, DbType.String);
        command.AddParameter("@Email", entity.Email.Valor, DbType.String);
        command.AddParameter("@LogradouroId", entity.Endereco.Logradouro.Id, DbType.Int32);
        command.AddParameter("@Numero", entity.Endereco.Numero, DbType.String);
        command.AddParameter("@Complemento", entity.Endereco.Complemento, DbType.String);
        command.AddParameter("@Senha", entity.Senha.Valor, DbType.String);
        command.AddParameter("@Foto", (object?)entity.Foto?.Url ?? DBNull.Value, DbType.String);
        if (entity is Colaborador colaborador)
        {
            command.AddParameter("@Admissao", colaborador.DataAdmissao, DbType.Date);
            command.AddParameter("@Tipo", (int)colaborador.Tipo, DbType.Int32);
            command.AddParameter("@Vinculo", (int)colaborador.Vinculo, DbType.Int32);
        }
        if (update)
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
                throw new KeyNotFoundException("Cadastro não encontrado.");
        }
        else
        {
            var id = await command.ExecuteScalarIdAsync("CADASTRO", "Não foi possível obter o ID.", cancellationToken);
            typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id);
        }
        return entity;
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {_table} WHERE {_id} = @Id";
        command.AddParameter("@Id", id, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        var result = Senha.Criar(novaSenha);
        if (result.IsFailure) throw new ArgumentException("Senha deve possuir pelo menos 6 caracteres.");
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"UPDATE {_table} SET senha = @Senha WHERE {_id} = @Id";
        command.AddParameter("@Id", id, DbType.Int32);
        command.AddParameter("@Senha", novaSenha, DbType.String);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
