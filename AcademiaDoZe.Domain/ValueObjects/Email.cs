using System;
using System.Collections.Generic;
using System.Text;

// Valdir Gonzaga

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }

    private Email(string valor)
    {
        Valor = valor;
    }

    public static Result<Email> Criar(string valor)
    {
        NormalizadoService.LimparEspacos(valor);
        if (string.IsNullOrWhiteSpace(valor))
            return Result<Email>.Failure("Email", "E-mail é obrigatório.");

        if (!valor.Contains("@"))
            return Result<Email>.Failure("Email", "E-mail inválido.");

        return Result<Email>.Success(new Email(valor));
    }
}