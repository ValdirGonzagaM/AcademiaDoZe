using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Mappings;
public static class ColaboradorMappingExtensions
{
    public static ColaboradorDto ToDTO(this Colaborador entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new ColaboradorDto
        {
            Id = entity.Id, Nome = entity.Nome, Cpf = entity.Cpf.Valor,
            DataNascimento = entity.DataNascimento, Telefone = entity.Telefone.Valor,
            Email = entity.Email.Valor, Endereco = entity.Endereco.Logradouro.ToDto(),
            Numero = entity.Endereco.Numero, Complemento = entity.Endereco.Complemento,
            Foto = entity.Foto == null ? null : new ArquivoDto { Conteudo = Convert.FromBase64String(entity.Foto.Url) },
            DataAdmissao = entity.DataAdmissao,
            Tipo = entity.Tipo.ToApplication(), Vinculo = entity.Vinculo.ToApplication(),
        };
    }
    public static Colaborador ToEntity(this ColaboradorDto dto, string? senhaOverride = null)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.Endereco == null) throw new ArgumentException("Selecione um logradouro.");
        var result = Colaborador.Criar(dto.Id, dto.Nome, dto.Cpf, dto.DataNascimento,
            dto.Telefone, dto.Email?.Trim() ?? "", dto.Endereco.ToEntity(), dto.Numero,
            dto.Complemento ?? "", senhaOverride ?? dto.Senha ?? "",
            dto.Foto == null ? null! : Arquivo.Criar("foto.jpg", Convert.ToBase64String(dto.Foto.Conteudo)), dto.DataAdmissao, dto.Tipo.ToDomain(), dto.Vinculo.ToDomain());
        if (result.IsFailure) throw new ArgumentException(PessoaValidation.Mensagem(result.Notifications));
        return result.Value!;
    }
    public static IEnumerable<ColaboradorDto> ToDTOList(this IEnumerable<Colaborador> entities) => entities.Select(e => e.ToDTO());
}
