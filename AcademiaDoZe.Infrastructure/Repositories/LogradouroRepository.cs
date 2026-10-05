using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class LogradouroRepository : BaseRepository, ILogradouroRepository
{
    public LogradouroRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }

    public async Task<Logradouro?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "SELECT id_logradouro, cep, nome, bairro, cidade, estado, pais FROM tb_logradouro WHERE id_logradouro = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_LOGRADOURO", $"Erro ao obter logradouro {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Logradouro>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "SELECT id_logradouro, cep, nome, bairro, cidade, estado, pais FROM tb_logradouro ORDER BY nome";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<Logradouro>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_LOGRADOUROS", $"Erro ao obter logradouros: {ex.Message}", ex);
        }
    }

    public async Task<Logradouro> Adicionar(Logradouro entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery("INSERT INTO tb_logradouro (cep, nome, bairro, cidade, estado, pais) VALUES (@Cep, @Nome, @Bairro, @Cidade, @Estado, @Pais)");
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Cep", entity.Cep.Valor, DbType.String);
            command.AddParameter("@Nome", entity.Nome, DbType.String);
            command.AddParameter("@Bairro", entity.Bairro, DbType.String);
            command.AddParameter("@Cidade", entity.Cidade, DbType.String);
            command.AddParameter("@Estado", entity.Estado, DbType.String);
            command.AddParameter("@Pais", entity.Pais, DbType.String);

            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_LOGRADOURO", "Falha ao obter ID do logradouro inserido.", cancellationToken);
            var idProp = typeof(Entity).GetProperty("Id");
            idProp?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_LOGRADOURO", $"Erro ao adicionar logradouro: {ex.Message}", ex);
        }
    }

    public async Task<Logradouro> Atualizar(Logradouro entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_logradouro SET cep = @Cep, nome = @Nome, bairro = @Bairro, cidade = @Cidade, estado = @Estado, pais = @Pais WHERE id_logradouro = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@Cep", entity.Cep.Valor, DbType.String);
            command.AddParameter("@Nome", entity.Nome, DbType.String);
            command.AddParameter("@Bairro", entity.Bairro, DbType.String);
            command.AddParameter("@Cidade", entity.Cidade, DbType.String);
            command.AddParameter("@Estado", entity.Estado, DbType.String);
            command.AddParameter("@Pais", entity.Pais, DbType.String);

            int rows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Logradouro {entity.Id} não encontrado.");
            }
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_LOGRADOURO", $"Erro ao atualizar logradouro {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_logradouro WHERE id_logradouro = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_LOGRADOURO", $"Erro ao remover logradouro {id}: {ex.Message}", ex);
        }
    }

    public static Logradouro Map(DbDataReader reader, string nomeColumn = "nome")
    {
        int id = reader.GetInt32Value("id_logradouro");
        string cepTexto = reader.GetStringValue("cep");
        string nome = reader.GetStringValue(nomeColumn);
        string bairro = reader.GetStringValue("bairro");
        string cidade = reader.GetStringValue("cidade");
        string estadoStr = reader.GetStringValue("estado");
        string paisStr = reader.GetStringValue("pais");

        var cepResult = Cep.Criar(cepTexto);
        var cep = cepResult.IsSuccess ? cepResult.Value! : Cep.Criar("00000-000").Value!;

        var res = Logradouro.Criar(id, cep, nome, bairro, cidade, estadoStr, paisStr);
        if (res.IsFailure)
        {
            throw new InfrastructureException("ERRO_DOMINIO_LOGRADOURO", "Falha ao mapear Logradouro de domínio.");
        }
        return res.Value!;
    }
}