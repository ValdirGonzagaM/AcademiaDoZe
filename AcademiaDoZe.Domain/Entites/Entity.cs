using System;
using System.Collections.Generic;
using System.Text;

// Valdir Gonzaga

using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Entities;

public abstract class Entity
{
    public int Id { get; protected set; }

    protected Entity(int id = 0)
    {
        if (id < 0)
            throw new DomainException("ID_NEGATIVO");

        Id = id;
    }
}