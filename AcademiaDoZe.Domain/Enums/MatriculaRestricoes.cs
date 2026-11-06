using System;
using System.Collections.Generic;
using System.Text;

// Valdir Gonzaga

namespace AcademiaDoZe.Domain.Enums;

[Flags]
public enum MatriculaRestricoes
{
    Nenhuma = 0,
    Diabetes = 1,
    Hipertensao = 2,
    Alergia = 4,
    Labirintite = 8,
    ProblemasRespiratorios = 16,
    RemedioContinuo = 32
}