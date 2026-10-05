// Valdir Gonzaga
using System;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using Xunit;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AlunoTests
{
    [Fact(DisplayName = "Aluno: criação válida com dados completos")]
    public void Deve_Criar_Aluno_Quando_DadosValidos()
    {
        // Arrange
        var cep = Cep.Criar("12345678").Value!;
        var logradouro = Logradouro.Criar(1, cep, "Rua das Flores", "Centro", "Lages", "SC", "Brasil").Value!;

        // Act
        var result = Aluno.Criar(
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
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("João da Silva", result.Value.Nome);
    }

    [Theory(DisplayName = "Aluno: nome obrigatório -> NOME_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_NomeInvalido(string? nomeInvalido)
    {
        // Arrange
        var cep = Cep.Criar("12345678").Value!;
        var logradouro = Logradouro.Criar(1, cep, "Rua das Flores", "Centro", "Lages", "SC", "Brasil").Value!;

        // Act
        var result = Aluno.Criar(
            1,
            nomeInvalido!,
            "12345678901",
            new DateOnly(2000, 1, 1),
            "49999999999",
            "aluno@email.com",
            logradouro,
            "100",
            "Apto 101",
            "Senha@123",
            null!
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "NOME_OBRIGATORIO");
    }
}