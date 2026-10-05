using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Cpf
{
    public string Valor { get; }

    private Cpf(string valor)
    {
        Valor = valor;
    }

    public static Result<Cpf> Criar(string valor)
    {
        string valorLimpo = NormalizadoService.LimparEDigitos(valor ?? string.Empty);

        if (valorLimpo.Length != 11)
            return Result<Cpf>.Failure("Cpf", "CPF deve possuir 11 dígitos.");

        return Result<Cpf>.Success(new Cpf(valorLimpo));
    }
}