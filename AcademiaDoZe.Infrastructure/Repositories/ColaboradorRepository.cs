using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Infrastructure.Repositories;
public class ColaboradorRepository : PessoaRepository<Colaborador>, IColaboradorRepository
{
    public ColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType, true) { }
    public async Task<bool> ExisteAdministradorAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand(); command.CommandText = "SELECT COUNT(*) FROM tb_colaborador WHERE tipo=0";
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }
    public Task<Colaborador> CriarPrimeiroAdministradorAsync(Colaborador colaborador, CancellationToken cancellationToken = default)
    {
        if (colaborador.Tipo != ColaboradorTipo.Administrador || colaborador.Vinculo != ColaboradorVinculo.CLT)
            throw new ArgumentException("O administrador deve ter vínculo CLT.");
        return SaveAsync(colaborador, false, cancellationToken, true);
    }
    public async Task<bool> PromoverPrimeiroAdministradorAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = connection is Microsoft.Data.Sqlite.SqliteConnection sqlite ? sqlite.BeginTransaction(deferred: false)
            : await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        await using var command = connection.CreateCommand(); command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(*) FROM tb_colaborador WHERE tipo=0";
        if (Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0) return false;
        command.CommandText = "UPDATE tb_colaborador SET tipo=0 WHERE id_colaborador=@Id AND vinculo=0";
        command.AddParameter("@Id", id, System.Data.DbType.Int32);
        var sucesso = await command.ExecuteNonQueryAsync(cancellationToken) == 1;
        await transaction.CommitAsync(cancellationToken); return sucesso;
    }
    protected override Colaborador Create(int id, string nome, string cpf, DateOnly nascimento,
        string telefone, string email, Logradouro logradouro, string numero,
        string complemento, string senha, Arquivo foto, DbDataReader reader)
    {
        var result = Colaborador.Criar(id, nome, cpf, nascimento, telefone, email, logradouro, numero, complemento, senha, foto, reader.GetDateOnlyValue("admissao"), (ColaboradorTipo)reader.GetInt32Value("tipo"), (ColaboradorVinculo)reader.GetInt32Value("vinculo"));
        if (result.IsFailure) throw new InvalidOperationException(string.Join("; ", result.Notifications.Select(n => n.Mensagem)));
        return result.Value!;
    }
    public async Task<IEnumerable<Colaborador>> ObterPorTipoAsync(ColaboradorTipo tipo, CancellationToken cancellationToken = default) =>
        await ReadAsync("p.tipo = @Value", (int)tipo, cancellationToken);
    public async Task<IEnumerable<Colaborador>> ObterPorVinculoAsync(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default) =>
        await ReadAsync("p.vinculo = @Value", (int)vinculo, cancellationToken);
}
