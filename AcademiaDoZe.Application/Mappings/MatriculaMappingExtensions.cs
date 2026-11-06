using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class MatriculaMappingExtensions
{
    public static MatriculaDto ToDTO(this Matricula entity) => new()
    {
        Id = entity.Id, AlunoMatricula = entity.AlunoMatricula.ToDTO(), Plano = entity.Plano.ToApplication(),
        DataInicio = entity.DataInicio, DataFim = entity.DataFim, Objetivo = entity.Objetivo,
        RestricoesMedicas = entity.RestricoesMedicas.ToApplication(), ObservacoesRestricoes = entity.ObservacoesRestricoes,
        LaudoMedico = entity.LaudoMedico == null ? null : new ArquivoDto { Conteudo = Convert.FromBase64String(entity.LaudoMedico.Url) }
    };

    public static Matricula ToEntity(this MatriculaDto dto, Aluno aluno)
    {
        var laudo = dto.LaudoMedico?.Conteudo;
        if (laudo is { Length: > 10 * 1024 * 1024 }) throw new ArgumentException("O laudo deve ter no máximo 10 MB.");
        var result = Matricula.Criar(dto.Id, aluno, dto.Plano.ToDomain(), dto.DataInicio, dto.DataFim,
            dto.Objetivo, dto.RestricoesMedicas.ToDomain(), laudo is { Length: > 0 }
                ? Arquivo.Criar("laudo.jpg", Convert.ToBase64String(laudo)) : null, dto.ObservacoesRestricoes ?? "");
        if (result.IsFailure) throw new ArgumentException(string.Join("; ", result.Notifications.Select(n => n.Mensagem switch
        {
            "OBJETIVO_OBRIGATORIO" => "Informe o objetivo da matrícula.", "ALUNO_OBRIGATORIO" => "Selecione um aluno cadastrado.",
            "PLANO_INVALIDO" => "Selecione um plano válido.", "RESTRICOES_MEDICAS_INVALIDAS" => "Restrições médicas inválidas.",
            _ => n.Mensagem
        })));
        return result.Value!;
    }
    public static IEnumerable<MatriculaDto> ToDTOList(this IEnumerable<Matricula> entities) => entities.Select(e => e.ToDTO());
}
