using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class ColaboradorService : IColaboradorService
{
    private readonly IColaboradorRepository _colaboradorRepository;

    public ColaboradorService(IColaboradorRepository colaboradorRepository)
    {
        _colaboradorRepository = colaboradorRepository ?? throw new ArgumentNullException(nameof(colaboradorRepository));
    }

    public async Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepository.ObterPorIdAsync(id, cancellationToken);
        return colaborador?.ToDTO();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var colaboradores = await _colaboradorRepository.ObterTodosAsync(cancellationToken);
        return colaboradores.ToDTOList();
    }

    public async Task<ColaboradorDto> AdicionarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        var entity = colaboradorDto.ToEntity();
        var resultado = await _colaboradorRepository.AdicionarAsync(entity, cancellationToken);
        return resultado.ToDTO();
    }

    public async Task<ColaboradorDto> AtualizarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        var entity = colaboradorDto.ToEntity();
        var resultado = await _colaboradorRepository.AtualizarAsync(entity, cancellationToken);
        return resultado.ToDTO();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _colaboradorRepository.RemoverAsync(id, cancellationToken);
    }

    public async Task<ColaboradorDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepository.ObterPorCpfAsync(cpf, cancellationToken);
        return colaborador?.ToDTO();
    }

    public async Task<ColaboradorDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepository.ObterPorEmailAsync(email, cancellationToken);
        return colaborador?.ToDTO();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorTipoAsync(AppColaboradorTipo tipo, CancellationToken cancellationToken = default)
    {
        var colaboradores = await _colaboradorRepository.ObterPorTipoAsync(tipo.ToDomain(), cancellationToken);
        return colaboradores.ToDTOList();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorVinculoAsync(AppColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
    {
        var colaboradores = await _colaboradorRepository.ObterPorVinculoAsync(vinculo.ToDomain(), cancellationToken);
        return colaboradores.ToDTOList();
    }

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        return await _colaboradorRepository.CpfJaExisteAsync(cpf, id, cancellationToken);
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        return await _colaboradorRepository.EmailJaExisteAsync(email, id, cancellationToken);
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        return await _colaboradorRepository.TrocarSenhaAsync(id, novaSenha, cancellationToken);
    }
}