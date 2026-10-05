using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class AlunoMappingExtensions
{
    public static AlunoDto ToDTO(this Aluno entity)
    {
        if (entity == null) return null!;

        return new AlunoDto
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Cpf = entity.Cpf?.Valor ?? string.Empty,
            DataNascimento = entity.DataNascimento,
            Telefone = entity.Telefone?.Valor ?? string.Empty,
            Email = entity.Email?.Valor ?? string.Empty,
            Numero = string.Empty,
            Foto = entity.Foto != null ? new ArquivoDto { Conteudo = Convert.FromBase64String(entity.Foto.Url ?? string.Empty) } : null!
        };
    }

    public static Aluno ToEntity(this AlunoDto dto)
    {
        if (dto == null) return null!;

        string conteudoString = dto.Foto?.Conteudo != null ? Convert.ToBase64String(dto.Foto.Conteudo) : string.Empty;

        return Aluno.Criar(
            dto.Id,
            dto.Nome,
            dto.Cpf ?? string.Empty,
            dto.DataNascimento,
            dto.Telefone ?? string.Empty,
            dto.Email ?? string.Empty,
            null!, // Logradouro
            dto.Numero ?? string.Empty,
            string.Empty, // Complemento
            string.Empty, // Bairro
            dto.Foto != null ? Arquivo.Criar("foto.jpg", conteudoString) : null!
        ).Value!;
    }

    public static IEnumerable<AlunoDto> ToDTOList(this IEnumerable<Aluno> entities)
    {
        return entities?.Select(e => e.ToDTO()) ?? Enumerable.Empty<AlunoDto>();
    }
}