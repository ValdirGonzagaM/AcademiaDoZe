using System;
using System.Collections.Generic;
using System.Text;

// Valdir Gonzaga

namespace AcademiaDoZe.Domain.ValueObjects;

public record Arquivo
{
    public string Nome { get; }
    public string Url { get; }

    private Arquivo(string nome, string url)
    {
        Nome = nome;
        Url = url;
    }

    public static Arquivo Criar(string nome, string url)
    {
        return new Arquivo(nome, url);
    }
}