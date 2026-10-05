// Valdir Gonzaga
using System;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using Xunit;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class MatriculaTests
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

    [Fact(DisplayName = "Matricula: criação válida com dados completos")]
    public void Deve_Criar_Matricula_Quando_DadosValidos()
    {
        // Arrange
        var aluno = CriarAlunoValido();

        // Act
        var result = Matricula.Criar(
            1,
            aluno,
            (MatriculaPlano)1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            "Hipertrofia",
            (MatriculaRestricoes)0,
            null,
            ""
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Hipertrofia", result.Value.Objetivo);
    }

    [Fact(DisplayName = "Matricula: aluno obrigatório -> ALUNO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_AlunoNulo()
    {
        // Act
        var result = Matricula.Criar(
            1,
            null!,
            (MatriculaPlano)1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            "Hipertrofia",
            (MatriculaRestricoes)0,
            null,
            ""
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "ALUNO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Matricula: objetivo obrigatório -> OBJETIVO_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_ObjetivoInvalido(string? objetivoInvalido)
    {
        // Arrange
        var aluno = CriarAlunoValido();

        // Act
        var result = Matricula.Criar(
            1,
            aluno,
            (MatriculaPlano)1,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            objetivoInvalido!,
            (MatriculaRestricoes)0,
            null,
            ""
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "OBJETIVO_OBRIGATORIO");
    }
}