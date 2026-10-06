using AcademiaDoZe.Domain.ValueObjects;
using Xunit;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("123456789012")] // 12 dígitos (inválido)
    [InlineData("1111111111")]    // 10 dígitos (inválido)
    [InlineData("invalid-cpf")]
    public void Cpf_QuandoInvalido_DeveRetornarFalha(string cpfInvalido)
    {
        // Act
        var resultado = Cpf.Criar(cpfInvalido);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("CPF deve possuir 11 dígitos.", Assert.Single(resultado.Notifications).Mensagem);
    }

    [Theory]
    [InlineData("08615141908")]
    [InlineData("086.151.419-08")]
    public void Cpf_ComOnzeDigitos_DeveRetornarSucessoENormalizar(string cpfInformado)
    {
        // Act
        var resultado = Cpf.Criar(cpfInformado);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal("08615141908", resultado.Value.Valor);
    }
}