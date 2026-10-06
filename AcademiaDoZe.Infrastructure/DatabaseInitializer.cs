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
            command.CommandText = $"""
                CREATE TABLE IF NOT EXISTS tb_{kind} (
                    id_{kind} INTEGER PRIMARY KEY AUTOINCREMENT,
                    cpf TEXT NOT NULL UNIQUE, nome TEXT NOT NULL, nascimento TEXT NOT NULL,
                    telefone TEXT NOT NULL, email TEXT NOT NULL COLLATE NOCASE UNIQUE,
                    logradouro_id INTEGER NOT NULL REFERENCES tb_logradouro(id_logradouro),
                    numero TEXT NOT NULL, complemento TEXT NOT NULL DEFAULT '',
                    senha TEXT NOT NULL, foto TEXT {extra});
                """;
            command.ExecuteNonQuery();
        }
    }
}
