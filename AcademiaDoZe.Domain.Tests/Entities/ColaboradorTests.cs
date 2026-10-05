// Valdir Gonzaga
using System;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using Xunit;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class ColaboradorTests
{
    [Fact(DisplayName = "Colaborador: criação válida com dados completos")]
    public void Deve_Criar_Colaborador_Quando_DadosValidos()
    {
        // Arrange
        var cep = Cep.Criar("12345678").Value!;
        var logradouro = Logradouro.Criar(1, cep, "Rua das Flores", "Centro", "Lages", "SC", "Brasil").Value!;

        // Act
        var result = Colaborador.Criar(
            1,
            "Carlos Silva",
            "12345678901",
            new DateOnly(1990, 5, 15),
            "49999999999",
            "colaborador@email.com",
            logradouro,
            "200",
            "Sala 01",
            "Senha@123",
            null!,
            new DateOnly(2020, 1, 1),
            ColaboradorTipo.Instrutor,
            ColaboradorVinculo.CLT
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Carlos Silva", result.Value.Nome);
    }

    [Theory(DisplayName = "Colaborador: nome obrigatório -> NOME_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_Falhar_Criacao_Quando_NomeInvalido(string? nomeInvalido)
    {
        // Arrange
        var cep = Cep.Criar("12345678").Value!;
        var logradouro = Logradouro.Criar(1, cep, "Rua das Flores", "Centro", "Lages", "SC", "Brasil").Value!;

        // Act
        var result = Colaborador.Criar(
            1,
            nomeInvalido!,
            "12345678901",
            new DateOnly(1990, 5, 15),
            "49999999999",
            "colaborador@email.com",
            logradouro,
            "200",
            "Sala 01",
            "Senha@123",
            null!,
            new DateOnly(2020, 1, 1),
            ColaboradorTipo.Instrutor,
            ColaboradorVinculo.CLT
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "NOME_OBRIGATORIO");
    }
}