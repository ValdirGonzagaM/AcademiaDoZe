// Valdir Gonzaga
using System;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using Xunit;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoColaboradorTests
{
    private Colaborador CriarColaboradorValido()
    {
        var cep = Cep.Criar("12345678").Value!;
        var logradouro = Logradouro.Criar(1, cep, "Rua das Flores", "Centro", "Lages", "SC", "Brasil").Value!;
        return Colaborador.Criar(
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
        ).Value!;
    }

    [Fact(DisplayName = "AcessoColaborador: criação válida com dados completos")]
    public void Deve_Criar_AcessoColaborador_Quando_DadosValidos()
    {
        // Arrange
        var colaborador = CriarColaboradorValido();

        // Act
        var result = AcessoColaborador.Criar(
            1,
            colaborador,
            DateTime.Now
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(colaborador, result.Value.Colaborador);
    }

    [Fact(DisplayName = "AcessoColaborador: colaborador obrigatório -> COLABORADOR_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_ColaboradorNulo()
    {
        // Act
        var result = AcessoColaborador.Criar(
            1,
            null!,
            DateTime.Now
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "COLABORADOR_OBRIGATORIO");
    }

    [Fact(DisplayName = "AcessoColaborador: data/hora obrigatória -> DATA_HORA_OBRIGATORIA")]
    public void Deve_Falhar_Criacao_Quando_DataHoraPadrao()
    {
        // Arrange
        var colaborador = CriarColaboradorValido();

        // Act
        var result = AcessoColaborador.Criar(
            1,
            colaborador,
            default
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(
            result.Notifications,
            n => n.Mensagem == "DATA_HORA_OBRIGATORIA");
    }
}