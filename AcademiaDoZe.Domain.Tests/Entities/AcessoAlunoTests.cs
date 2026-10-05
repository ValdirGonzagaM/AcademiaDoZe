// Valdir Gonzaga
using System;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using Xunit;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoAlunoTests
{
    private Aluno CriarAlunoValido()
    {
        var cep = Cep.Criar("12345678").Value!;
        var logradouro = Logradouro.Criar(1, cep, "Rua das Flores", "Centro", "Lages", "SC", "Brasil").Value!;
        return Aluno.Criar(
            1,
            "João da Silva",
            "12345678901",
            new DateOnly(2000, 1, 1),
            "49999999999",
            "aluno@email.com",
            logradouro,
            "100",
            "Apto 101",
            "Senha@123",
            null!
        ).Value!;
    }

    [Fact(DisplayName = "AcessoAluno: criação válida com dados completos")]
    public void Deve_Criar_AcessoAluno_Quando_DadosValidos()
    {
        // Arrange
        var aluno = CriarAlunoValido();

        // Act
        var result = AcessoAluno.Criar(
            1,
            aluno,
            DateTime.Now
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(aluno, result.Value.Aluno);
    }

    [Fact(DisplayName = "AcessoAluno: aluno obrigatório -> ALUNO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_AlunoNulo()
    {
        // Act
        var result = AcessoAluno.Criar(
            1,
            null!,
            DateTime.Now
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "ALUNO_OBRIGATORIO");
    }

    [Fact(DisplayName = "AcessoAluno: data/hora obrigatória -> DATA_HORA_OBRIGATORIA")]
    public void Deve_Falhar_Criacao_Quando_DataHoraPadrao()
    {
        // Arrange
        var aluno = CriarAlunoValido();

        // Act
        var result = AcessoAluno.Criar(
            1,
            aluno,
            default
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "DATA_HORA_OBRIGATORIA");
    }
}