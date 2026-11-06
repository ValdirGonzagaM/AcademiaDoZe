using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.AppMaui.Services;

public sealed class MatriculaConsulta(IMatriculaService matriculas, IAlunoService alunos)
{
    public async Task<IReadOnlyList<MatriculaDto>> BuscarAsync(string filtro, string texto)
    {
        texto = texto.Trim();
        if (filtro == "Todas") return (await matriculas.ObterTodasAsync()).ToList();
        if (filtro == "Ativas")
            return (await matriculas.ObterAtivasAsync(texto.Length == 0 ? 0 : Inteiro(texto, false))).ToList();
        if (filtro == "Vencendo em dias")
            return (await matriculas.ObterVencendoEmDiasAsync(Inteiro(texto, true))).ToList();
        if (filtro == "ID")
        {
            var item = await matriculas.ObterPorIdAsync(Inteiro(texto, false));
            return item == null ? [] : [item];
        }
        if (filtro == "ID do aluno") return (await matriculas.ObterPorAlunoIdAsync(Inteiro(texto, false))).ToList();
        if (filtro == "CPF do aluno")
        {
            var cpf = new string(texto.Where(char.IsDigit).ToArray());
            if (cpf.Length != 11) throw new ArgumentException("Informe os 11 dígitos do CPF do aluno.");
            var aluno = await alunos.ObterPorCpfAsync(cpf);
            return aluno == null ? [] : (await matriculas.ObterPorAlunoIdAsync(aluno.Id)).ToList();
        }
        if (filtro == "Nome do aluno")
        {
            if (texto.Length == 0) return (await matriculas.ObterTodasAsync()).ToList();
            return (await matriculas.ObterTodasAsync()).Where(m => m.AlunoMatricula.Nome.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (filtro == "Plano")
        {
            var planos = Enum.GetValues<AppMatriculaPlano>().Where(p => p.GetDisplayName().Contains(texto, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (texto.Length == 0 || planos.Length == 0) throw new ArgumentException("Informe Mensal, Trimestral, Semestral ou Anual.");
            var lista = new List<MatriculaDto>();
            foreach (var plano in planos) lista.AddRange(await matriculas.ObterPorPlanoAsync(plano));
            return lista;
        }
        throw new ArgumentException("Selecione um filtro válido.");
    }
    private static int Inteiro(string texto, bool zero)
    {
        if (!int.TryParse(texto, out var valor) || valor < (zero ? 0 : 1))
            throw new ArgumentException(zero ? "Informe uma quantidade de dias maior ou igual a zero." : "Informe um ID inteiro maior que zero.");
        return valor;
    }
}
