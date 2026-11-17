using Microsoft.Data.Sqlite;
namespace AcademiaDoZe.Infrastructure;

public static class DatabaseInitializer
{
    public static void InitializeSqlite(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA foreign_keys = ON;
            CREATE TABLE IF NOT EXISTS tb_logradouro (
                id_logradouro INTEGER PRIMARY KEY AUTOINCREMENT,
                cep TEXT NOT NULL, nome TEXT NOT NULL, bairro TEXT NOT NULL,
                cidade TEXT NOT NULL, estado TEXT NOT NULL, pais TEXT NOT NULL);
            """;
        command.ExecuteNonQuery();
        foreach (var kind in new[] { "aluno", "colaborador" })
        {
            var extra = kind == "colaborador"
                ? ", admissao TEXT NOT NULL, tipo INTEGER NOT NULL, vinculo INTEGER NOT NULL" : "";
            var obrigatorioEmail = kind == "colaborador" ? " NOT NULL" : "";
            command.CommandText = $"""
                CREATE TABLE IF NOT EXISTS tb_{kind} (
                    id_{kind} INTEGER PRIMARY KEY AUTOINCREMENT,
                    cpf TEXT NOT NULL UNIQUE, nome TEXT NOT NULL, nascimento TEXT NOT NULL,
                    telefone TEXT NOT NULL, email TEXT{obrigatorioEmail} COLLATE NOCASE UNIQUE,
                    logradouro_id INTEGER NOT NULL REFERENCES tb_logradouro(id_logradouro),
                    numero TEXT NOT NULL, complemento TEXT NOT NULL DEFAULT '',
                    senha TEXT NOT NULL, foto TEXT {extra});
                """;
            command.ExecuteNonQuery();
        }
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS tb_matricula (
                id_matricula INTEGER PRIMARY KEY AUTOINCREMENT,
                aluno_id INTEGER NOT NULL REFERENCES tb_aluno(id_aluno),
                plano INTEGER NOT NULL, data_inicio TEXT NOT NULL, data_fim TEXT NOT NULL,
                objetivo TEXT NOT NULL, restricao_medica INTEGER NOT NULL DEFAULT 0,
                obs_restricao TEXT NOT NULL DEFAULT '', laudo_medico TEXT);
            CREATE INDEX IF NOT EXISTS ix_matricula_aluno_fim ON tb_matricula(aluno_id, data_fim);
            CREATE TABLE IF NOT EXISTS tb_acesso (
                id_acesso INTEGER PRIMARY KEY AUTOINCREMENT,
                aluno_id INTEGER REFERENCES tb_aluno(id_aluno),
                colaborador_id INTEGER REFERENCES tb_colaborador(id_colaborador),
                entrada_utc INTEGER NOT NULL, saida_utc INTEGER,
                CHECK ((aluno_id IS NOT NULL AND colaborador_id IS NULL) OR (aluno_id IS NULL AND colaborador_id IS NOT NULL)),
                CHECK (saida_utc IS NULL OR saida_utc >= entrada_utc));
            CREATE UNIQUE INDEX IF NOT EXISTS ix_acesso_aluno_aberto ON tb_acesso(aluno_id) WHERE saida_utc IS NULL;
            CREATE UNIQUE INDEX IF NOT EXISTS ix_acesso_colaborador_aberto ON tb_acesso(colaborador_id) WHERE saida_utc IS NULL;
            CREATE INDEX IF NOT EXISTS ix_acesso_entrada ON tb_acesso(entrada_utc);
            """;
        command.ExecuteNonQuery();
        AtualizarEmailAluno(connection);

    }
    private static void AtualizarEmailAluno(SqliteConnection connection)
    {
        using var inspect = connection.CreateCommand(); inspect.CommandText = "PRAGMA table_info(tb_aluno)";
        bool obrigatorio = false;
        using (var reader = inspect.ExecuteReader())
            while (reader.Read()) if (reader.GetString(1) == "email") obrigatorio = reader.GetInt32(3) == 1;
        if (!obrigatorio) return;
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys=OFF"; command.ExecuteNonQuery();
        try
        {
            using var transaction = connection.BeginTransaction(); command.Transaction = transaction;
            command.CommandText = "SELECT COALESCE(MAX(seq),0) FROM sqlite_sequence WHERE name='tb_aluno'";
            var ultimoId = Convert.ToInt64(command.ExecuteScalar());
            var indices = new List<string>();
            command.CommandText = "SELECT sql FROM sqlite_master WHERE type IN ('index','trigger') AND tbl_name='tb_aluno' AND sql IS NOT NULL";
            using (var reader = command.ExecuteReader()) while (reader.Read()) indices.Add(reader.GetString(0));
            command.CommandText = """
                CREATE TABLE tb_aluno_email_opcional (
                    id_aluno INTEGER PRIMARY KEY AUTOINCREMENT, cpf TEXT NOT NULL UNIQUE,
                    nome TEXT NOT NULL, nascimento TEXT NOT NULL, telefone TEXT NOT NULL,
                    email TEXT COLLATE NOCASE UNIQUE,
                    logradouro_id INTEGER NOT NULL REFERENCES tb_logradouro(id_logradouro),
                    numero TEXT NOT NULL, complemento TEXT NOT NULL DEFAULT '', senha TEXT NOT NULL, foto TEXT);
                INSERT INTO tb_aluno_email_opcional (id_aluno,cpf,nome,nascimento,telefone,email,logradouro_id,numero,complemento,senha,foto)
                    SELECT id_aluno,cpf,nome,nascimento,telefone,NULLIF(email,''),logradouro_id,numero,complemento,senha,foto FROM tb_aluno;
                DROP TABLE tb_aluno;
                ALTER TABLE tb_aluno_email_opcional RENAME TO tb_aluno;
                """;
            command.ExecuteNonQuery();
            command.CommandText = "UPDATE sqlite_sequence SET seq=MAX(seq,@UltimoId) WHERE name='tb_aluno'";
            command.Parameters.AddWithValue("@UltimoId", ultimoId);
            command.ExecuteNonQuery(); command.Parameters.Clear();
            foreach (var sql in indices) { command.CommandText = sql; command.ExecuteNonQuery(); }
            command.CommandText = "PRAGMA foreign_key_check";
            using (var reader = command.ExecuteReader()) if (reader.Read()) throw new InvalidOperationException("Falha na atualização dos vínculos do banco de dados.");
            transaction.Commit(); command.Transaction = null;
        }
        finally { command.Transaction = null; command.CommandText = "PRAGMA foreign_keys=ON"; command.ExecuteNonQuery(); }
    }

}
