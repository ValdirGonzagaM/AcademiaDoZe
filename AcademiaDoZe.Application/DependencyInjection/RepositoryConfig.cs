using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Infrastructure;

namespace AcademiaDoZe.Application.DependencyInjection;

public class RepositoryConfig
{
    public required string ConnectionString { get; set; }
    public required DatabaseType DatabaseType { get; set; }
}