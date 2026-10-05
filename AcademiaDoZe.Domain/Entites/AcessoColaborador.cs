using System;
using System.Collections.Generic;
using System.Text;

// Valdir Gonzaga

using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity
{
    public Colaborador Colaborador { get; private set; }
    public DateTime DataHora { get; private set; }

    private AcessoColaborador(
        int id,
        Colaborador colaborador,
        DateTime dataHora)
        : base(id)
    {
        Colaborador = colaborador;
        DataHora = dataHora;
    }

    public static Result<AcessoColaborador> Criar(
        int id,
        Colaborador colaborador,
        DateTime dataHora)
    {
        var notifications = new List<Notification>();

        if (colaborador == null)
            notifications.Add(
                new Notification("Colaborador", "COLABORADOR_OBRIGATORIO"));

        if (dataHora == default)
            notifications.Add(
                new Notification("DataHora", "DATA_HORA_OBRIGATORIA"));

        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);

        var acesso = new AcessoColaborador(
            id,
            colaborador,
            dataHora);

        return Result<AcessoColaborador>.Success(acesso);
    }
}