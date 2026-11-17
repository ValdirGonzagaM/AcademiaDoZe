using AcademiaDoZe.Infrastructure;
using AcademiaDoZe.Infrastructure.Repositories;
using AcademiaDoZe.Domain.Enums;
using Microsoft.Data.Sqlite;
using Xunit;

namespace AcademiaDoZe.Application.Tests;

public sealed class AtualizacaoBancoTests
{
    [Fact]
    public async Task Atualizacao_EmailOpcional_PreservaIdsFotosSenhasMatriculasAcessosEIndices()
    {
        var path = Path.Combine(Path.GetTempPath(), $"migracao-{Guid.NewGuid():N}.db");
        var conexao = new SqliteConnectionStringBuilder { DataSource = path, Pooling = false, ForeignKeys = true }.ToString();
        try
        {
            DatabaseInitializer.InitializeSqlite(conexao);
            using (var db = new SqliteConnection(conexao))
            {
                db.Open(); using var cmd = db.CreateCommand();
                cmd.CommandText = """
                    PRAGMA foreign_keys=OFF;
                    DROP TABLE tb_aluno;
                    CREATE TABLE tb_aluno (id_aluno INTEGER PRIMARY KEY AUTOINCREMENT,cpf TEXT NOT NULL UNIQUE,nome TEXT NOT NULL,
                        nascimento TEXT NOT NULL,telefone TEXT NOT NULL,email TEXT NOT NULL COLLATE NOCASE UNIQUE,
                        logradouro_id INTEGER NOT NULL REFERENCES tb_logradouro(id_logradouro),numero TEXT NOT NULL,
                        complemento TEXT NOT NULL DEFAULT '',senha TEXT NOT NULL,foto TEXT);
                    PRAGMA foreign_keys=ON;
                    INSERT INTO tb_logradouro VALUES (7,'88501000','Rua Central','Centro','Lages','SC','Brasil');
                    INSERT INTO tb_aluno VALUES (42,'08615141908','Valdir Gonzaga','2000-01-01','49999999999','aluno@teste.com',7,'10','Sala 2','hash-existente','AQID');
                    UPDATE sqlite_sequence SET seq=100 WHERE name='tb_aluno';
                    INSERT INTO tb_matricula VALUES (9,42,0,'2026-11-01','2026-12-01','Saúde',0,'observacao-original','AQID');
                    INSERT INTO tb_acesso VALUES (11,42,NULL,638000000000000000,NULL);
                    CREATE INDEX ix_aluno_nome ON tb_aluno(nome);
                    CREATE TABLE auditoria_aluno (id INTEGER);
                    CREATE TRIGGER aluno_atualizado AFTER UPDATE ON tb_aluno BEGIN INSERT INTO auditoria_aluno VALUES (new.id_aluno); END;
                    """;
                cmd.ExecuteNonQuery();
            }
            DatabaseInitializer.InitializeSqlite(conexao);
            DatabaseInitializer.InitializeSqlite(conexao);
            var aluno = await new AlunoRepository(conexao, DatabaseType.Sqlite).ObterPorIdAsync(42);
            Assert.NotNull(aluno); Assert.Equal("hash-existente", aluno.Senha.Valor);
            Assert.Equal("AQID", aluno.Foto.Url); Assert.Equal(7, aluno.Endereco.Logradouro.Id);
            var matricula = await new MatriculaRepository(conexao, DatabaseType.Sqlite).ObterPorId(9);
            Assert.NotNull(matricula); Assert.Equal(42, matricula.AlunoMatricula.Id); Assert.Equal("AQID", matricula.LaudoMedico!.Url);
            Assert.Equal(11, Assert.Single(await new AcessoRepository(conexao).ConsultarAsync()).Id);
            using var connection = new SqliteConnection(conexao); connection.Open(); using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_key_check";
            using (var reader = command.ExecuteReader()) Assert.False(reader.Read());
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='ix_aluno_nome'";
            Assert.Equal(1L, command.ExecuteScalar());
            command.CommandText = "SELECT seq FROM sqlite_sequence WHERE name='tb_aluno'";
            Assert.Equal(100L, command.ExecuteScalar());
            command.CommandText = "UPDATE tb_aluno SET nome='Valdir Gonzaga' WHERE id_aluno=42; SELECT id FROM auditoria_aluno";
            Assert.Equal(42L, command.ExecuteScalar());
        }
        finally { File.Delete(path); }
    }
}
