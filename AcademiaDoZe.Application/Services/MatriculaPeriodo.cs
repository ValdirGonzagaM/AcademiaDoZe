using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Application.Services;

public static class MatriculaPeriodo
{
    public static DateOnly CalcularFim(AppMatriculaPlano plano, DateOnly inicio)
    {
        if (inicio == default) throw new ArgumentException("Informe a data de início.");
        var meses = plano switch
        {
            AppMatriculaPlano.Mensal => 1, AppMatriculaPlano.Trimestral => 3,
            AppMatriculaPlano.Semestral => 6, AppMatriculaPlano.Anual => 12,
            _ => throw new ArgumentException("Selecione um plano válido.")
        };
        try { return inicio.AddMonths(meses); }
        catch (ArgumentOutOfRangeException) { throw new ArgumentException("A data de início ultrapassa o período permitido para o plano."); }
    }
}
