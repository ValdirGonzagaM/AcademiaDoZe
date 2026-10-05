using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings;

public static class LogradouroMappingExtensions
{
    public static LogradouroDto ToDto(this Logradouro logradouro)
    {
        ArgumentNullException.ThrowIfNull(logradouro);
        return new LogradouroDto
        {
            Id = logradouro.Id,
            Cep = logradouro.Cep.Valor,
            Nome = logradouro.Nome,
            Bairro = logradouro.Bairro,
            Cidade = logradouro.Cidade,
            Estado = logradouro.Estado,
            Pais = logradouro.Pais
        };
    }

    public static Logradouro ToEntity(this LogradouroDto logradouroDto)
    {
        ArgumentNullException.ThrowIfNull(logradouroDto);

        // Instancia e valida o Value Object Cep
        var cepResult = Cep.Criar(logradouroDto.Cep);
        if (cepResult.IsFailure)
            throw new InvalidOperationException($"CEP inválido: {string.Join(", ", cepResult.Notifications.Select(n => n.Mensagem))}");

        // Passa o cepResult.Value (objeto Cep) em vez da string
        var result = Logradouro.Criar(
            logradouroDto.Id,
            cepResult.Value!,
            logradouroDto.Nome,
            logradouroDto.Bairro,
            logradouroDto.Cidade,
            logradouroDto.Estado,
            logradouroDto.Pais
        );

        if (result.IsFailure)
            throw new InvalidOperationException($"Erro de validação ao converter Logradouro: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");

        return result.Value!;
    }

    public static Logradouro UpdateFromDto(this Logradouro logradouro, LogradouroDto logradouroDto)
    {
        ArgumentNullException.ThrowIfNull(logradouro);
        ArgumentNullException.ThrowIfNull(logradouroDto);

        var cepString = logradouroDto.Cep ?? logradouro.Cep.Valor;
        var cepResult = Cep.Criar(cepString);
        if (cepResult.IsFailure)
            throw new InvalidOperationException($"CEP inválido: {string.Join(", ", cepResult.Notifications.Select(n => n.Mensagem))}");

        var result = Logradouro.Criar(
            logradouro.Id,
            cepResult.Value!,
            logradouroDto.Nome ?? logradouro.Nome,
            logradouroDto.Bairro ?? logradouro.Bairro,
            logradouroDto.Cidade ?? logradouro.Cidade,
            logradouroDto.Estado ?? logradouro.Estado,
            logradouroDto.Pais ?? logradouro.Pais
        );

        if (result.IsFailure)
            throw new InvalidOperationException($"Erro de validação ao atualizar Logradouro: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");

        return result.Value!;
    }
}