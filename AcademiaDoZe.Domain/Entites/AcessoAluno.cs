using System;
using System.Collections.Generic;
using System.Text;

// Valdir Gonzaga

using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity
{
    public Aluno Aluno { get; private set; }
    public DateTime DataHora { get; private set; }

    private AcessoAluno(
        int id,
        Aluno aluno,
        DateTime dataHora)
        : base(id)
    {
        Aluno = aluno;
        DataHora = dataHora;
    }

    public static Result<AcessoAluno> Criar(
        int id,
        Aluno aluno,
        DateTime dataHora)
    {
        var notifications = new List<Notification>();

        if (aluno == null)
            notifications.Add(
                new Notification("Aluno", "ALUNO_OBRIGATORIO"));

        if (dataHora == default)
            notifications.Add(
                new Notification("DataHora", "DATA_HORA_OBRIGATORIA"));

        if (notifications.Count != 0)
            return Result<AcessoAluno>.Failure(notifications);

        var acesso = new AcessoAluno(
            id,
            aluno!,
            dataHora);

        return Result<AcessoAluno>.Success(acesso);
    }
}