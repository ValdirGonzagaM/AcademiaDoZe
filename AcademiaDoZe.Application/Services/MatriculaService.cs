using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class MatriculaService : IMatriculaService
{
    private readonly IMatriculaRepository _matriculaRepository;

    public MatriculaService(IMatriculaRepository matriculaRepository)
    {
        _matriculaRepository = matriculaRepository ?? throw new ArgumentNullException(nameof(matriculaRepository));
    }

    public async Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var matricula = await _matriculaRepository.ObterPorId(id, cancellationToken);
        return matricula?.ToDTO();
    }

    public async Task<IEnumerable<MatriculaDto>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        var matriculas = await _matriculaRepository.ObterTodos(cancellationToken);
        return matriculas.ToDTOList();
    }

    public async Task<MatriculaDto> AdicionarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        var entity = matriculaDto.ToEntity();
        var resultado = await _matriculaRepository.Adicionar(entity, cancellationToken);
        return resultado.ToDTO();
    }

    public async Task<MatriculaDto> AtualizarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        var entity = matriculaDto.ToEntity();
        var resultado = await _matriculaRepository.Atualizar(entity, cancellationToken);
        return resultado.ToDTO();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _matriculaRepository.Remover(id, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoIdAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var matriculas = await _matriculaRepository.ObterPorAluno(alunoId, cancellationToken);
        return matriculas.ToDTOList();
    }

    public async Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var matricula = await _matriculaRepository.ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        return matricula?.ToDTO();
    }

    public async Task<bool> PossuiMatriculaAtivaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        return await _matriculaRepository.PossuiMatriculaAtiva(alunoId, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterAtivasAsync(int alunoId = 0, CancellationToken cancellationToken = default)
    {
        var matriculas = await _matriculaRepository.ObterAtivas(alunoId, cancellationToken);
        return matriculas.ToDTOList();
    }

    public async Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int dias, CancellationToken cancellationToken = default)
    {
        var matriculas = await _matriculaRepository.ObterVencendoEmDias(dias, cancellationToken);
        return matriculas.ToDTOList();
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(AppMatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        var matriculas = await _matriculaRepository.ObterPorPlano(plano.ToDomain(), cancellationToken);
        return matriculas.ToDTOList();
    }
}