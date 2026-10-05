using System;
using System.Collections.Generic;
using System.Text;

// Valdir Gonzaga

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Telefone
{
    public string Valor { get; }

    private Telefone(string valor)
    {
        Valor = valor;
    }

    public static Result<Telefone> Criar(string valor)
    {
        valor = NormalizadoService.LimparEDigitos(valor);
        if (valor.Length < 10 || valor.Length > 11)
            return Result<Telefone>.Failure(
                "Telefone",
                "Telefone deve possuir 10 ou 11 dígitos.");

        return Result<Telefone>.Success(new Telefone(valor));
    }
}   