namespace AcademiaDoZe.Domain.Entities;

public enum PessoaAcesso { Aluno = 1, Colaborador = 2 }

public sealed record Visita(int Id, PessoaAcesso Tipo, int PessoaId, string Nome, string Cpf,
    DateTimeOffset Entrada, DateTimeOffset? Saida)
{
    public TimeSpan Permanencia(DateTimeOffset agora) => (Saida ?? agora) > Entrada ? (Saida ?? agora) - Entrada : TimeSpan.Zero;
}

public static class Jornada
{
    public static TimeSpan NoDia(IEnumerable<Visita> visitas, DateOnly dia, DateTimeOffset agora, TimeZoneInfo fuso)
    {
        var inicio = InicioDia(dia, fuso);
        var fim = dia == DateOnly.MaxValue ? DateTimeOffset.MaxValue : InicioDia(dia.AddDays(1), fuso);
        var total = TimeSpan.Zero;
        foreach (var visita in visitas)
        {
            var de = visita.Entrada > inicio ? visita.Entrada : inicio;
            var ate = visita.Saida ?? agora;
            if (ate > agora) ate = agora;
            if (ate > fim) ate = fim;
            if (ate > de) total += ate - de;
        }
        return total;
    }
    public static DateTimeOffset InicioDia(DateOnly dia, TimeZoneInfo fuso)
    {
        var data = dia.ToDateTime(TimeOnly.MinValue);
        while (fuso.IsInvalidTime(data)) data = data.AddMinutes(1);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(data, fuso));
    }
}
