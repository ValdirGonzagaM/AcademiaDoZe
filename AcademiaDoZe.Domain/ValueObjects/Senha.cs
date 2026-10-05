using System;
using System.Collections.Generic;
using System.Text;

// Valdir Gonzaga

using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Senha
{
    public string Valor { get; }

    private Senha(string valor)
    {
        Valor = valor;
    }

    public static Result<Senha> Criar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return Result<Senha>.Failure("Senha", "Senha é obrigatória.");

        if (valor.Length < 6)
            return Result<Senha>.Failure(
                "Senha",
                "Senha deve possuir pelo menos 6 caracteres.");

        return Result<Senha>.Success(new Senha(valor));
    }
}