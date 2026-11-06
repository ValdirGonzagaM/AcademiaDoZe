using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaEnumMappingExtensions
{
    public static MatriculaPlano ToDomain(this AppMatriculaPlano plano) => (MatriculaPlano)plano;
    public static AppMatriculaPlano ToApplication(this MatriculaPlano plano) => (AppMatriculaPlano)plano;
    public static MatriculaRestricoes ToDomain(this AppMatriculaRestricoes restricoes)
    {
        if (((int)restricoes & ~63) != 0) throw new ArgumentException("Restrições médicas inválidas.");
        // Mantém o valor de Alergia já utilizado na persistência de domínio.
        return (MatriculaRestricoes)(((int)restricoes & 51)
            | (restricoes.HasFlag(AppMatriculaRestricoes.Alergias) ? 4 : 0)
            | (restricoes.HasFlag(AppMatriculaRestricoes.Labirintite) ? 8 : 0));
    }
    public static AppMatriculaRestricoes ToApplication(this MatriculaRestricoes restricoes)
    {
        if (((int)restricoes & ~63) != 0) throw new ArgumentException("Restrições médicas inválidas.");
        return (AppMatriculaRestricoes)(((int)restricoes & 51)
            | (restricoes.HasFlag(MatriculaRestricoes.Alergia) ? 8 : 0)
            | (restricoes.HasFlag(MatriculaRestricoes.Labirintite) ? 4 : 0));
    }
}
