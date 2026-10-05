using System;
using System.Collections.Generic;
using System.Text;

// Valdir Gonzaga

namespace AcademiaDoZe.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string mensagem)
        : base(mensagem)
    {
    }
}