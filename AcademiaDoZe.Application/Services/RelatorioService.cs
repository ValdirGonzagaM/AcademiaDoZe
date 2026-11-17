using AcademiaDoZe.Application.Seguranca;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public sealed record JornadaDiaria(int PessoaId, string Nome, DateOnly Dia, TimeSpan Tempo);
public sealed record IndicadorMensal(DateOnly Mes, int Vigentes, int Encerrados);
public sealed record RelatorioAcademia(int EntradasAlunos, int EntradasColaboradores, int Presentes, TimeSpan MediaPermanencia,
    IReadOnlyList<(int Hora, int Entradas)> Horarios, IReadOnlyList<JornadaDiaria> Jornadas, IReadOnlyList<IndicadorMensal> Contratos);

public sealed class RelatorioService(IAcessoRepository acessos, IMatriculaRepository matriculas, SessaoUsuario sessao, TimeProvider clock)
{
    public async Task<RelatorioAcademia> GerarAsync(DateOnly inicio, DateOnly fim)
    {
        sessao.Exigir(Permissao.Relatorios);
        if (fim < inicio || fim.DayNumber - inicio.DayNumber > 366) throw new ArgumentException("Informe um período válido de até 366 dias.");
        var visitas = await acessos.ConsultarAsync();
        var agora = clock.GetUtcNow(); var fuso = clock.LocalTimeZone;
        DateOnly Dia(DateTimeOffset data) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(data, fuso).DateTime);
        var periodo = visitas.Where(v => Dia(v.Entrada) >= inicio && Dia(v.Entrada) <= fim).ToList();
        var concluidas = periodo.Where(v => v.Tipo == PessoaAcesso.Aluno && v.Saida != null).ToList();
        var media = concluidas.Count == 0 ? TimeSpan.Zero : TimeSpan.FromTicks((long)concluidas.Average(v => v.Permanencia(agora).Ticks));
        var jornadas = new List<JornadaDiaria>();
        foreach (var grupo in visitas.Where(v => v.Tipo == PessoaAcesso.Colaborador).GroupBy(v => v.PessoaId))
            for (var dia = inicio; dia <= fim; dia = dia.AddDays(1))
            {
                var tempo = Jornada.NoDia(grupo, dia, agora, fuso);
                if (tempo > TimeSpan.Zero) jornadas.Add(new(grupo.Key, grupo.First().Nome, dia, tempo));
                if (dia == DateOnly.MaxValue) break;
            }
        var contratos = (await matriculas.ObterTodos()).ToList();
        var indicadores = new List<IndicadorMensal>();
        for (var mes = new DateOnly(inicio.Year, inicio.Month, 1); mes <= fim;)
        {
            var ultimo = new DateOnly(mes.Year, mes.Month, DateTime.DaysInMonth(mes.Year, mes.Month));
            var referencia = ultimo < fim ? ultimo : fim;
            var baseAlunos = contratos.Where(m => m.DataInicio <= referencia).Select(m => m.AlunoMatricula.Id).Distinct().ToHashSet();
            var vigentes = contratos.Where(m => m.DataInicio <= referencia && m.DataFim >= referencia).Select(m => m.AlunoMatricula.Id).Distinct().Count();
            indicadores.Add(new(mes, vigentes, baseAlunos.Count - vigentes));
            if (mes.Year == 9999 && mes.Month == 12) break;
            mes = mes.AddMonths(1);
        }
        return new(periodo.Count(v => v.Tipo == PessoaAcesso.Aluno), periodo.Count(v => v.Tipo == PessoaAcesso.Colaborador),
            visitas.Count(v => v.Saida == null), media,
            periodo.GroupBy(v => TimeZoneInfo.ConvertTime(v.Entrada, fuso).Hour).Select(g => (g.Key, g.Count())).OrderByDescending(x => x.Item2).ThenBy(x => x.Key).ToList(),
            jornadas, indicadores);
    }
}
