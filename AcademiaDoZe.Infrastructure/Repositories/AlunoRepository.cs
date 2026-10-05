using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using System.Data;

namespace AcademiaDoZe.Infrastructure.Repositories;

// Valdir Gonzaga
public class AlunoRepository : BaseRepository, IAlunoRepository
{
    public AlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }

    public async Task<Aluno> AdicionarAsync(Aluno entity, CancellationToken cancellationToken = default)
    {
        string query = FormatInsertQuery(@"INSERT INTO tb_aluno 
            (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto) 
            VALUES (@Cpf, @Nome, @Nascimento, @Telefone, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto)");

        await using var command = await CreateCommandAsync(query, cancellationToken);
        command.AddParameter("@Cpf", entity.Cpf.Valor, DbType.String);
        command.AddParameter("@Nome", entity.Nome, DbType.String);
        command.AddParameter("@Nascimento", entity.DataNascimento, DbType.Date);
        command.AddParameter("@Telefone", entity.Telefone.Valor, DbType.String);
        command.AddParameter("@Email", entity.Email.Valor, DbType.String);
        command.AddParameter("@LogradouroId", entity.Endereco.Logradouro.Id, DbType.Int32);
        command.AddParameter("@Numero", entity.Endereco.Numero, DbType.String);
        command.AddParameter("@Complemento", (object?)entity.Endereco.Complemento ?? DBNull.Value, DbType.String);
        command.AddParameter("@Senha", entity.Senha.Valor, DbType.String);
        command.AddParameter("@Foto", (object?)entity.Foto?.Url ?? DBNull.Value, DbType.String);

        int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ALUNO", "Falha ao obter ID inserido para o aluno.", cancellationToken);

        var idProperty = entity.GetType().GetProperty("Id") ?? entity.GetType().BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, id);

        return entity;
    }

    public async Task<Aluno?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Aluno>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Aluno> AtualizarAsync(Aluno entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Aluno?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Aluno?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Aluno>> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task<Aluno> Adicionar(Aluno entity, CancellationToken cancellationToken = default)
    {
        return await AdicionarAsync(entity, cancellationToken);
    }
}