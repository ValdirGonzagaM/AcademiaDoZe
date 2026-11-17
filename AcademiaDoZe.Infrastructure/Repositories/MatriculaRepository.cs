using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class MatriculaRepository : BaseRepository, IMatriculaRepository
{
    public MatriculaRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery => @"
        SELECT 
            m.id_matricula, m.aluno_id, m.plano, m.data_inicio, m.data_fim, m.objetivo, m.restricao_medica, m.obs_restricao, m.laudo_medico,
            a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
            l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_matricula m
        INNER JOIN tb_aluno a ON m.aluno_id = a.id_aluno
        INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro";

    public static Matricula Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_matricula");
            MatriculaPlano plano = (MatriculaPlano)reader.GetInt32Value("plano");
            DateOnly dataInicio = reader.GetDateOnlyValue("data_inicio");
            DateOnly dataFim = reader.GetDateOnlyValue("data_fim");
            string objetivo = reader.GetStringValue("objetivo");
            MatriculaRestricoes restricoesMedicas = (MatriculaRestricoes)reader.GetInt32Value("restricao_medica");
            string? obsRestricao = reader.GetNullableString("obs_restricao");

            string? laudoPath = reader.GetNullableString("laudo_medico");
            Arquivo? laudoMedico = !string.IsNullOrEmpty(laudoPath) ? Arquivo.Criar("laudo.pdf", laudoPath) : null;

            var aluno = MapearAluno(reader);

            var result = Matricula.Criar(
                id: id,
                alunoMatricula: aluno,
                plano: plano,
                dataInicio: dataInicio,
                dataFim: dataFim,
                objetivo: objetivo,
                restricoesMedicas: restricoesMedicas,
                laudoMedico: laudoMedico,
                observacoesRestricoes: obsRestricao ?? string.Empty
            );

            if (result.IsFailure)
            {
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear matricula ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            }

            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_MATRICULA", $"Erro ao mapear dados da matrícula: {ex.Message}", ex);
        }
    }

    private static Aluno MapearAluno(DbDataReader reader)
    {
        int logradouroId = reader.GetInt32Value("id_logradouro");
        string cepStr = reader.GetStringValue("cep");
        string logradouroNome = reader.GetStringValue("logradouro_nome");
        string bairro = reader.GetStringValue("bairro");
        string cidade = reader.GetStringValue("cidade");
        string estado = reader.GetStringValue("estado");
        string pais = reader.GetStringValue("pais");

        var cepObj = Cep.Criar(cepStr).Value!;
        var logradouro = Logradouro.Criar(logradouroId, cepObj, logradouroNome, bairro, cidade, estado, pais).Value!;

        int alunoId = reader.GetInt32Value("id_aluno");
        string cpf = reader.GetStringValue("cpf");
        string nome = reader.GetStringValue("aluno_nome");
        DateOnly nascimento = reader.GetDateOnlyValue("nascimento");
        string telefone = reader.GetStringValue("telefone");
        string email = reader.GetNullableString("email") ?? "";
        string numero = reader.GetStringValue("numero");
        string? complemento = reader.GetNullableString("complemento");
        string senha = reader.GetStringValue("senha");
        string? fotoPath = reader.GetNullableString("foto");
        Arquivo? foto = !string.IsNullOrEmpty(fotoPath) ? Arquivo.Criar("foto.jpg", fotoPath) : null;

        return Aluno.Criar(
            id: alunoId,
            nome: nome,
            cpf: cpf,
            dataNascimento: nascimento,
            telefone: telefone,
            email: email,
            endereco: logradouro,
            numero: numero,
            complemento: complemento ?? string.Empty,
            senha: senha,
            foto: foto!
        ).Value!;
    }

    private async Task<List<Matricula>> ConsultarAsync(string filtro, Action<DbCommand>? parametros, string ordem,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"{BaseSelectQuery} WHERE {filtro} ORDER BY {ordem}";
        parametros?.Invoke(command);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var lista = new List<Matricula>();
        while (await reader.ReadAsync(cancellationToken)) lista.Add(Map(reader));
        return lista;
    }
    private static void Hoje(DbCommand command) => command.AddParameter("@Hoje", DateOnly.FromDateTime(DateTime.Today), DbType.Date);

    public async Task<int> Contar(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM tb_matricula";
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }
    public async Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default) =>
        (await ConsultarAsync("m.id_matricula = @Id", c => c.AddParameter("@Id", id, DbType.Int32), "m.id_matricula", cancellationToken)).FirstOrDefault();
    public async Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default) =>
        await ConsultarAsync("1=1", null, "m.data_inicio DESC, m.id_matricula DESC", cancellationToken);
    public async Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default) =>
        await ConsultarAsync("m.aluno_id = @Aluno", c => c.AddParameter("@Aluno", alunoId, DbType.Int32), "m.data_inicio DESC", cancellationToken);
    public async Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default) =>
        (await ObterAtivas(alunoId, cancellationToken)).OrderByDescending(m => m.DataFim).FirstOrDefault();
    public async Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default) =>
        await ObterMatriculaAtivaPorAluno(alunoId, cancellationToken) != null;
    public async Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default) =>
        await ConsultarAsync("m.data_fim >= @Hoje" + (alunoId > 0 ? " AND m.aluno_id = @Aluno" : ""), c =>
        {
            Hoje(c);
            if (alunoId > 0) c.AddParameter("@Aluno", alunoId, DbType.Int32);
        }, "m.data_fim ASC", cancellationToken);
    public async Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        if (dias < 0 || dias > DateOnly.MaxValue.DayNumber - hoje.DayNumber)
            throw new ArgumentException("Informe um número de dias válido, maior ou igual a zero.");
        return await ConsultarAsync("m.data_fim >= @Hoje AND m.data_fim <= @Limite", c =>
        {
            Hoje(c); c.AddParameter("@Limite", hoje.AddDays(dias), DbType.Date);
        }, "m.data_fim ASC", cancellationToken);
    }
    public async Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default) =>
        await ConsultarAsync("m.plano = @Plano", c => c.AddParameter("@Plano", (int)plano, DbType.Int32), "a.nome", cancellationToken);

    public Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default) => SalvarAsync(entity, false, cancellationToken);
    public Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default) => SalvarAsync(entity, true, cancellationToken);
    private async Task<Matricula> SalvarAsync(Matricula entity, bool atualizar, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = connection is Microsoft.Data.Sqlite.SqliteConnection sqlite
            ? sqlite.BeginTransaction(deferred: false)
            : await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (atualizar)
        {
            await using var existe = connection.CreateCommand();
            existe.Transaction = transaction;
            existe.CommandText = "SELECT COUNT(*) FROM tb_matricula WHERE id_matricula = @Id";
            existe.AddParameter("@Id", entity.Id, DbType.Int32);
            if (Convert.ToInt32(await existe.ExecuteScalarAsync(cancellationToken)) == 0)
                throw new KeyNotFoundException("Matrícula não encontrada.");
        }
        if (!atualizar || entity.DataFim >= DateOnly.FromDateTime(DateTime.Today))
        {
            await using var verificar = connection.CreateCommand();
            verificar.Transaction = transaction;
            verificar.CommandText = "SELECT COUNT(*) FROM tb_matricula WHERE aluno_id = @Aluno AND data_fim >= @Hoje AND id_matricula <> @Id";
            verificar.AddParameter("@Aluno", entity.AlunoMatricula.Id, DbType.Int32);
            verificar.AddParameter("@Id", atualizar ? entity.Id : 0, DbType.Int32);
            Hoje(verificar);
            if (Convert.ToInt32(await verificar.ExecuteScalarAsync(cancellationToken)) > 0)
                throw new InvalidOperationException("O aluno já possui uma matrícula vigente ou agendada. Aguarde o encerramento ou exclua a matrícula existente.");
        }
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = atualizar ? """
            UPDATE tb_matricula SET aluno_id=@Aluno, plano=@Plano, data_inicio=@Inicio, data_fim=@Fim,
                objetivo=@Objetivo, restricao_medica=@Restricoes, obs_restricao=@Observacoes, laudo_medico=@Laudo
                WHERE id_matricula=@Id
            """ : FormatInsertQuery("""
            INSERT INTO tb_matricula (aluno_id,plano,data_inicio,data_fim,objetivo,restricao_medica,obs_restricao,laudo_medico)
                VALUES (@Aluno,@Plano,@Inicio,@Fim,@Objetivo,@Restricoes,@Observacoes,@Laudo)
            """);
        command.AddParameter("@Id", entity.Id, DbType.Int32);
        command.AddParameter("@Aluno", entity.AlunoMatricula.Id, DbType.Int32);
        command.AddParameter("@Plano", (int)entity.Plano, DbType.Int32);
        command.AddParameter("@Inicio", entity.DataInicio, DbType.Date);
        command.AddParameter("@Fim", entity.DataFim, DbType.Date);
        command.AddParameter("@Objetivo", entity.Objetivo, DbType.String);
        command.AddParameter("@Restricoes", (int)entity.RestricoesMedicas, DbType.Int32);
        command.AddParameter("@Observacoes", entity.ObservacoesRestricoes, DbType.String);
        command.AddParameter("@Laudo", (object?)entity.LaudoMedico?.Url ?? DBNull.Value, DbType.String);
        var id = atualizar ? entity.Id : await command.ExecuteScalarIdAsync("MATRICULA", "Não foi possível obter o ID da matrícula.", cancellationToken);
        if (atualizar) await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        if (!atualizar) typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id);
        return entity;
    }
    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM tb_matricula WHERE id_matricula = @Id";
        command.AddParameter("@Id", id, DbType.Int32);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
