using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;

public static class ColaboradorMappingExtensions
{
    public static ColaboradorDto ToDTO(this Colaborador entity)
    {
        if (entity == null) return null!;

        return new ColaboradorDto
        {
            Id = entity.Id,
            Nome = entity.Nome,
            Cpf = entity.Cpf?.Valor ?? entity.Cpf?.ToString() ?? string.Empty,
            Email = entity.Email?.Valor ?? entity.Email?.ToString() ?? string.Empty,
            DataNascimento = entity.DataNascimento,
            Telefone = entity.Telefone?.Valor ?? entity.Telefone?.ToString() ?? string.Empty,
            Numero = entity.Endereco?.Numero ?? string.Empty,
            DataAdmissao = entity.DataAdmissao,
            Tipo = entity.Tipo.ToApplication(),
            Vinculo = entity.Vinculo.ToApplication()
        };
    }

    public static Colaborador ToEntity(this ColaboradorDto dto)
    {
        if (dto == null) return null!;

        var result = Colaborador.Criar(
            id: dto.Id,
            nome: dto.Nome,
            cpf: dto.Cpf ?? string.Empty,
            dataNascimento: dto.DataNascimento,
            telefone: dto.Telefone ?? string.Empty,
            email: dto.Email ?? string.Empty,
            endereco: null!, // Ou forneça a instância de Logradouro mapeada se disponível
            numero: dto.Numero ?? string.Empty,
            complemento: string.Empty,
            senha: "SenhaPadrao123!", // Senha temporária/padrão exigida pelo método
            foto: null!,
            dataAdmissao: dto.DataAdmissao,
            tipo: dto.Tipo.ToDomain(),
            vinculo: dto.Vinculo.ToDomain()
        );

        return result.Value!;
    }

    public static IEnumerable<ColaboradorDto> ToDTOList(this IEnumerable<Colaborador> entities)
    {
        return entities?.Select(e => e.ToDTO()) ?? Enumerable.Empty<ColaboradorDto>();
    }
}