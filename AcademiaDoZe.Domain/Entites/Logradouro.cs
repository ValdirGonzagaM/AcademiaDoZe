using System;
using System.Collections.Generic;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities; // ou namespace AcademiaDoZe.Domain.Entites conforme seu projeto

public class Logradouro : Entity
{
    public Cep Cep { get; private set; }
    public string Nome { get; private set; }
    public string Bairro { get; private set; }
    public string Cidade { get; private set; }
    public string Estado { get; private set; }
    public string Pais { get; private set; }

    private Logradouro(
        int id,
        Cep cep,
        string nome,
        string bairro,
        string cidade,
        string estado,
        string pais)
        : base(id)
    {
        Cep = cep;
        Nome = nome;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Pais = pais;
    }

    public static Result<Logradouro> Criar(
        int id,
        Cep cep,
        string nome,
        string bairro,
        string cidade,
        string estado,
        string pais)
    {
        var notifications = new List<Notification>();

        if (cep == null)
            notifications.Add(new Notification("Cep", "CEP_OBRIGATORIO"));

        if (string.IsNullOrWhiteSpace(nome))
            notifications.Add(new Notification("Nome", "LOGRADOURO_NOME_OBRIGATORIO"));

        if (notifications.Count > 0)
            return Result<Logradouro>.Failure(notifications);

        return Result<Logradouro>.Success(new Logradouro(id, cep, nome, bairro, cidade, estado, pais));
    }
}