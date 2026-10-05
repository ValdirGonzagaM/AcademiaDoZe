using System;
using System.Collections.Generic;
using System.Text;

// Valdir Gonzaga

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Cep
{
    public string Valor { get; }

    private Cep(string valor)
    {
        Valor = valor;
    }

    public static Result<Cep> Criar(string valor)
    {
        valor = NormalizadoService.LimparEDigitos(valor);

        if (valor.Length != 8)
            return Result<Cep>.Failure("Cep", "CEP deve possuir 8 dígitos.");

        return Result<Cep>.Success(new Cep(valor));
    }
}