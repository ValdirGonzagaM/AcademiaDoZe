using System.Data;
using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using Microsoft.Data.Sqlite;

namespace AcademiaDoZe.Infrastructure.Repositories;

public sealed class AcessoRepository(string connectionString) : BaseRepository(connectionString, DatabaseType.Sqlite), IAcessoRepository
{
    private static string Tabela(PessoaAcesso tipo) => tipo switch
    {
        PessoaAcesso.Aluno => "aluno", PessoaAcesso.Colaborador => "colaborador", _ => throw new ArgumentException("Tipo de pessoa inválido.")
    };
    public async Task<PessoaIdentificada?> IdentificarAsync(PessoaAcesso tipo, string cpf, CancellationToken cancellationToken = default)
    {
        var tabela = Tabela(tipo);
        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11) throw new ArgumentException("Informe o CPF completo com 11 dígitos.");
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT id_{tabela},nome,cpf,nascimento,foto,{(tipo == PessoaAcesso.Colaborador ? "vinculo" : "NULL")} AS vinculo FROM tb_{tabela} WHERE cpf=@Cpf";
        command.AddParameter("@Cpf", digitos, DbType.String);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new(reader.GetInt32(0), tipo, reader.GetString(1), reader.GetString(2), DateOnly.FromDateTime(reader.GetDateTime(3)),
            reader.IsDBNull(4) ? null : reader.GetString(4), reader.IsDBNull(5) ? null : (ColaboradorVinculo)reader.GetInt32(5));
    }
    private static string Select => """
        SELECT v.id_acesso,v.aluno_id,v.colaborador_id,COALESCE(a.nome,c.nome),COALESCE(a.cpf,c.cpf),v.entrada_utc,v.saida_utc
        FROM tb_acesso v LEFT JOIN tb_aluno a ON a.id_aluno=v.aluno_id
        LEFT JOIN tb_colaborador c ON c.id_colaborador=v.colaborador_id
        """;
    private static async Task<List<Visita>> LerAsync(DbCommand command, CancellationToken ct)
    {
        var lista = new List<Visita>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var aluno = !reader.IsDBNull(1);
            lista.Add(new(reader.GetInt32(0), aluno ? PessoaAcesso.Aluno : PessoaAcesso.Colaborador,
                reader.GetInt32(aluno ? 1 : 2), reader.GetString(3), reader.GetString(4),
                new DateTimeOffset(reader.GetInt64(5), TimeSpan.Zero), reader.IsDBNull(6) ? null : new DateTimeOffset(reader.GetInt64(6), TimeSpan.Zero)));
        }
        return lista;
    }
    public async Task<IReadOnlyList<Visita>> ConsultarAsync(PessoaAcesso? tipo = null, int? pessoaId = null, CancellationToken cancellationToken = default)
    {
        if (pessoaId.HasValue && !tipo.HasValue) throw new ArgumentException("Informe o tipo da pessoa.");
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        var coluna = tipo.HasValue ? Tabela(tipo.Value) + "_id" : "";
        command.CommandText = Select + " WHERE 1=1" + (tipo.HasValue ? $" AND v.{coluna} IS NOT NULL" : "")
            + (pessoaId.HasValue ? $" AND v.{coluna}=@Pessoa" : "") + " ORDER BY v.entrada_utc DESC,v.id_acesso DESC";
        if (pessoaId.HasValue) command.AddParameter("@Pessoa", pessoaId.Value, DbType.Int32);
        return await LerAsync(command, cancellationToken);
    }
    public async Task<ResultadoVisita> RegistrarAsync(PessoaAcesso tipo, int pessoaId, bool entrada, DateTimeOffset agora, TimeZoneInfo fuso, CancellationToken cancellationToken = default)
    {
        var tabela = Tabela(tipo);
        var hoje = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(agora, fuso).DateTime);
        await using var connection = (SqliteConnection)await OpenConnectionAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(deferred: false);
        await using var pessoa = connection.CreateCommand();
        pessoa.Transaction = transaction;
        pessoa.CommandText = $"SELECT nome,cpf,{(tipo == PessoaAcesso.Colaborador ? "vinculo" : "NULL")} FROM tb_{tabela} WHERE id_{tabela}=@Id";
        pessoa.AddParameter("@Id", pessoaId, DbType.Int32);
        string nome, cpf; TimeSpan? limite = null;
        await using (var reader = await pessoa.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken)) throw new ArgumentException("Cadastro não encontrado.");
            nome = reader.GetString(0); cpf = reader.GetString(1);
            if (tipo == PessoaAcesso.Colaborador) limite = TimeSpan.FromHours(reader.GetInt32(2) == (int)ColaboradorVinculo.CLT ? 8 : 6);
        }
        await using var historico = connection.CreateCommand();
        historico.Transaction = transaction;
        historico.CommandText = Select + $" WHERE v.{tabela}_id=@Id ORDER BY v.entrada_utc DESC";
        historico.AddParameter("@Id", pessoaId, DbType.Int32);
        var visitas = await LerAsync(historico, cancellationToken);
        var aberta = visitas.SingleOrDefault(v => v.Saida == null);
        DateOnly? fimPlano = null;
        if (entrada)
        {
            if (aberta != null) throw new InvalidOperationException("Já existe uma entrada em aberto. Registre a saída antes de entrar novamente.");
            if (visitas.Any(v => (v.Saida ?? v.Entrada) > agora)) throw new InvalidOperationException("O relógio do dispositivo é anterior ao último registro.");
            if (tipo == PessoaAcesso.Aluno)
            {
                await using var matricula = connection.CreateCommand();
                matricula.Transaction = transaction;
                matricula.CommandText = "SELECT data_fim FROM tb_matricula WHERE aluno_id=@Id AND data_inicio<=@Hoje AND data_fim>=@Hoje ORDER BY data_fim DESC LIMIT 1";
                matricula.AddParameter("@Id", pessoaId, DbType.Int32); matricula.AddParameter("@Hoje", hoje, DbType.Date);
                var valor = await matricula.ExecuteScalarAsync(cancellationToken);
                if (valor == null) throw new InvalidOperationException("Aluno sem matrícula ativa. Não é possível registrar a entrada.");
                fimPlano = DateOnly.FromDateTime(Convert.ToDateTime(valor, System.Globalization.CultureInfo.InvariantCulture));
            }
            if (limite.HasValue && Jornada.NoDia(visitas, hoje, agora, fuso) >= limite.Value)
                throw new InvalidOperationException("O limite diário de trabalho foi atingido. Não é possível registrar nova entrada hoje.");
        }
        else
        {
            if (aberta == null) throw new InvalidOperationException("Não existe entrada em aberto para registrar a saída.");
            if (agora < aberta.Entrada) throw new InvalidOperationException("A saída não pode ser anterior à entrada.");
        }
        await using var gravar = connection.CreateCommand();
        gravar.Transaction = transaction;
        gravar.CommandText = entrada ? $"INSERT INTO tb_acesso ({tabela}_id,entrada_utc) VALUES (@Pessoa,@Agora); SELECT last_insert_rowid();"
            : "UPDATE tb_acesso SET saida_utc=@Agora WHERE id_acesso=@Id AND saida_utc IS NULL";
        gravar.AddParameter("@Pessoa", pessoaId, DbType.Int32); gravar.AddParameter("@Agora", agora.UtcTicks, DbType.Int64);
        gravar.AddParameter("@Id", aberta?.Id ?? 0, DbType.Int32);
        Visita registrada;
        if (entrada)
        {
            var id = Convert.ToInt32(await gravar.ExecuteScalarAsync(cancellationToken));
            registrada = new(id, tipo, pessoaId, nome, cpf, agora, null); visitas.Add(registrada);
        }
        else
        {
            if (await gravar.ExecuteNonQueryAsync(cancellationToken) != 1) throw new InvalidOperationException("A saída já foi registrada.");
            registrada = aberta! with { Saida = agora };
            visitas[visitas.FindIndex(v => v.Id == registrada.Id)] = registrada;
        }
        await transaction.CommitAsync(cancellationToken);
        return new(registrada, registrada.Permanencia(agora), Jornada.NoDia(visitas, hoje, agora, fuso), limite, fimPlano);
    }
}
