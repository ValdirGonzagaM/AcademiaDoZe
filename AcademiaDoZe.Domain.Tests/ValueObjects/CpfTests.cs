using AcademiaDoZe.Domain.ValueObjects;
using Xunit;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("123.456.789-00")] // Exemplo de formato incorreto se não tiver 11 dígitos limpos
    [InlineData("1111111111")]    // 10 dígitos (inválido)
    [InlineData("invalid-cpf")]
    public void Cpf_QuandoInvalido_DeveRetornarFalha(string cpfInvalido)
    {
        // Act
        var resultado = Cpf.Criar(cpfInvalido);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("CPF deve possuir 11 dígitos.", resultado.Notifications.First().ToString());
    }

    [Fact]
    public void Cpf_QuandoValido_DeveRetornarSucessoEInstanciaCorreta()
    {
        // Arrange
        var cpfValido = "08615141908"; // 11 dígitos

        // Act
        var resultado = Cpf.Criar(cpfValido);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal("08615141908", resultado.Value.Valor);
    }
}