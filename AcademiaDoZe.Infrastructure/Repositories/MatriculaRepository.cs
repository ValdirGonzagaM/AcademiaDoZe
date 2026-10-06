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
        string email = reader.GetStringValue("email");
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
            foto: foto
        ).Value!;
    }

    public async Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.id_matricula = @Id";
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter matrícula por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY m.data_inicio DESC";
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todas as matrículas: {ex.Message}", ex);
        }
    }

    public async Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = FormatInsertQuery(@"INSERT INTO tb_matricula 
                (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, obs_restricao, laudo_medico) 
                VALUES (@AlunoId, @Plano, @DataInicio, @DataFim, @Objetivo, @RestricaoMedica, @ObsRestricao, @LaudoMedico)");

            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.AddParameter("@AlunoId", entity.AlunoMatricula.Id, DbType.Int32);
            command.AddParameter("@Plano", (int)entity.Plano, DbType.Int32);
            command.AddParameter("@DataInicio", entity.DataInicio, DbType.Date);
            command.AddParameter("@DataFim", entity.DataFim, DbType.Date);
            command.AddParameter("@Objetivo", entity.Objetivo, DbType.String);
            command.AddParameter("@RestricaoMedica", (int)entity.RestricoesMedicas, DbType.Int32);
            command.AddParameter("@ObsRestricao", (object?)entity.ObservacoesRestricoes ?? DBNull.Value, DbType.String);
            command.AddParameter("@LaudoMedico", (object?)entity.LaudoMedico?.Url ?? DBNull.Value, DbType.String);

            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_MATRICULA", "Falha ao obter ID inserido para a matricula.", cancellationToken);

            var type = entity.GetType();
            var idProperty = type.GetProperty("Id") ?? type.BaseType?.GetProperty("Id");
            if (idProperty != null && idProperty.CanWrite)
            {
                idProperty.SetValue(entity, id);
            }
            else
            {
                var field = type.GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                           ?? type.BaseType?.GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                field?.SetValue(entity, id);
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_MATRICULA", $"Erro ao adicionar matrícula para o aluno ID {entity.AlunoMatricula.Id}: {ex.Message}", ex);
        }
    }

    public async Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = @"UPDATE tb_matricula SET 
                aluno_id = @AlunoId, 
                plano = @Plano, 
                data_inicio = @DataInicio, 
                data_fim = @DataFim, 
                objetivo = @Objetivo, 
                restricao_medica = @RestricaoMedica, 
                obs_restricao = @ObsRestricao, 
                laudo_medico = @LaudoMedico 
                WHERE id_matricula = @Id";

            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@AlunoId", entity.AlunoMatricula.Id, DbType.Int32);
            command.AddParameter("@Plano", (int)entity.Plano, DbType.Int32);
            command.AddParameter("@DataInicio", entity.DataInicio, DbType.Date);
            command.AddParameter("@DataFim", entity.DataFim, DbType.Date);
            command.AddParameter("@Objetivo", entity.Objetivo, DbType.String);
            command.AddParameter("@RestricaoMedica", (int)entity.RestricoesMedicas, DbType.Int32);
            command.AddParameter("@ObsRestricao", (object?)entity.ObservacoesRestricoes ?? DBNull.Value, DbType.String);
            command.AddParameter("@LaudoMedico", (object?)entity.LaudoMedico?.Url ?? DBNull.Value, DbType.String);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhuma matricula encontrada com o ID {entity.Id} para atualização.");
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_MATRICULA", $"Erro ao atualizar matrícula ID {entity.Id} do aluno ID {entity.AlunoMatricula.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_matricula WHERE id_matricula = @Id";
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.AddParameter("@Id", id, DbType.Int32);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_MATRICULA", $"Erro ao remover matricula ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId ORDER BY m.data_inicio DESC";
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ALUNO", $"Erro ao obter matriculas do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.aluno_id = @AlunoId AND m.data_fim >= CURDATE() ORDER BY m.data_fim DESC";
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_MATRICULA_ATIVA", $"Erro ao obter matricula ativa do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default)
    {
        var matricula = await ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        return matricula != null;
    }

    public async Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.data_fim >= CURDATE() {(alunoId > 0 ? "AND m.aluno_id = @id" : "")} ORDER BY m.data_fim ASC";
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            if (alunoId > 0)
            {
                command.AddParameter("@id", alunoId, DbType.Int32);
            }
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_MATRICULAS_ATIVAS", $"Erro ao obter matriculas ativas: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.data_fim >= CURDATE() AND m.data_fim <= DATE_ADD(CURDATE(), INTERVAL @Dias DAY) ORDER BY m.data_fim ASC";
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.AddParameter("@Dias", dias, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_MATRICULAS_VENCENDO", $"Erro ao obter matrículas vencendo em {dias} dias: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} WHERE m.plano = @Plano ORDER BY a.nome";
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.AddParameter("@Plano", (int)plano, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var matriculas = new List<Matricula>();
            while (await reader.ReadAsync(cancellationToken))
            {
                matriculas.Add(Map(reader));
            }
            return matriculas;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_PLANO", $"Erro ao obter matriculas pelo plano {plano}: {ex.Message}", ex);
        }
    }
}