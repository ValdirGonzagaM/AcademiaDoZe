using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaMappingExtensions
{
    public static MatriculaDto ToDTO(this Matricula entity)
    {
        if (entity == null) return null!;

        return new MatriculaDto
        {
            Id = entity.Id,
            AlunoMatricula = entity.AlunoMatricula?.ToDTO()!,
            Plano = entity.Plano.ToApplication(),
            DataInicio = entity.DataInicio,
            DataFim = entity.DataFim,
            Objetivo = entity.Objetivo,
            RestricoesMedicas = entity.RestricoesMedicas.ToApplication()
        };
    }

    public static Matricula ToEntity(this MatriculaDto dto)
    {
        if (dto == null) return null!;

        string laudoString = dto.LaudoMedico?.Conteudo != null ? Convert.ToBase64String(dto.LaudoMedico.Conteudo) : string.Empty;

        return Matricula.Criar(
            dto.Id,
            dto.AlunoMatricula?.ToEntity()!,
            dto.Plano.ToDomain(),
            dto.DataInicio,
            dto.DataFim,
            dto.Objetivo,
            dto.RestricoesMedicas.ToDomain(),
            dto.LaudoMedico != null ? Arquivo.Criar("laudo.pdf", laudoString) : null!
        ).Value!;
    }

    public static IEnumerable<MatriculaDto> ToDTOList(this IEnumerable<Matricula> entities)
    {
        return entities?.Select(e => e.ToDTO()) ?? Enumerable.Empty<MatriculaDto>();
    }
}