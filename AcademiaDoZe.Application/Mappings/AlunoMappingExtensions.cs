using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Mappings;
public static class AlunoMappingExtensions
{
    public static AlunoDto ToDTO(this Aluno entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new AlunoDto
        {
            Id = entity.Id, Nome = entity.Nome, Cpf = entity.Cpf.Valor,
            DataNascimento = entity.DataNascimento, Telefone = entity.Telefone.Valor,
            Email = entity.Email.Valor, Endereco = entity.Endereco.Logradouro.ToDto(),
            Numero = entity.Endereco.Numero, Complemento = entity.Endereco.Complemento,
            Foto = entity.Foto == null ? null : new ArquivoDto { Conteudo = Convert.FromBase64String(entity.Foto.Url) },

        };
    }
    public static Aluno ToEntity(this AlunoDto dto, string? senhaOverride = null)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.Endereco == null) throw new ArgumentException("Selecione um logradouro.");
        var result = Aluno.Criar(dto.Id, dto.Nome, dto.Cpf, dto.DataNascimento,
            dto.Telefone, dto.Email?.Trim() ?? "", dto.Endereco.ToEntity(), dto.Numero,
            dto.Complemento ?? "", senhaOverride ?? dto.Senha ?? "",
            dto.Foto == null ? null! : Arquivo.Criar("foto.jpg", Convert.ToBase64String(dto.Foto.Conteudo)));
        if (result.IsFailure) throw new ArgumentException(PessoaValidation.Mensagem(result.Notifications));
        return result.Value!;
    }
    public static IEnumerable<AlunoDto> ToDTOList(this IEnumerable<Aluno> entities) => entities.Select(e => e.ToDTO());
}
