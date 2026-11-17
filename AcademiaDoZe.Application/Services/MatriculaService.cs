using AcademiaDoZe.Application.Seguranca;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class MatriculaService : IMatriculaService
{
    private readonly SessaoUsuario? _sessao;
    private readonly IMatriculaRepository _matriculaRepository;
    private readonly IAlunoRepository _alunos;

    public MatriculaService(IMatriculaRepository matriculaRepository, IAlunoRepository alunos, SessaoUsuario? sessao = null)
    {
        _matriculaRepository = matriculaRepository ?? throw new ArgumentNullException(nameof(matriculaRepository));
        _alunos = alunos; _sessao = sessao;
    }

    public Task<int> ContarAsync(CancellationToken cancellationToken = default) { _sessao?.Exigir(Permissao.Matriculas); return _matriculaRepository.Contar(cancellationToken); }

    private async Task<AcademiaDoZe.Domain.Entities.Matricula> PrepararAsync(MatriculaDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var referencia = dto.AlunoMatricula ?? throw new ArgumentException("Busque um aluno cadastrado pelo CPF.");
        var aluno = referencia.Id > 0 ? await _alunos.ObterPorIdAsync(referencia.Id, cancellationToken)
            : await _alunos.ObterPorCpfAsync(referencia.Cpf ?? "", cancellationToken);
        if (aluno == null) throw new ArgumentException("Aluno não encontrado. Busque um aluno cadastrado pelo CPF.");
        var menor16 = aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-16));
        if ((menor16 || dto.RestricoesMedicas != AppMatriculaRestricoes.None)
            && dto.LaudoMedico?.Conteudo is not { Length: > 0 })
            throw new ArgumentException("Laudo médico obrigatório para menores de 16 anos ou alunos com restrições médicas.");
        dto.DataFim = MatriculaPeriodo.CalcularFim(dto.Plano, dto.DataInicio);
        return dto.ToEntity(aluno);
    }

    public async Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Matriculas);
        var matricula = await _matriculaRepository.ObterPorId(id, cancellationToken);
        return matricula?.ToDTO();
    }

    public async Task<IEnumerable<MatriculaDto>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Matriculas);
        var matriculas = await _matriculaRepository.ObterTodos(cancellationToken);
        return matriculas.ToDTOList();
    }

    public async Task<MatriculaDto> AdicionarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Matriculas);
        var entity = await PrepararAsync(matriculaDto, cancellationToken);
        var resultado = await _matriculaRepository.Adicionar(entity, cancellationToken);
        return resultado.ToDTO();
    }

    public async Task<MatriculaDto> AtualizarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Matriculas);
        var entity = await PrepararAsync(matriculaDto, cancellationToken);
        var resultado = await _matriculaRepository.Atualizar(entity, cancellationToken);
        return resultado.ToDTO();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Matriculas);
        return await _matriculaRepository.Remover(id, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoIdAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        _sessao?.ExigirPessoa(PessoaAcesso.Aluno, alunoId);
        var matriculas = await _matriculaRepository.ObterPorAluno(alunoId, cancellationToken);
        return matriculas.ToDTOList();
    }

    public async Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        _sessao?.ExigirPessoa(PessoaAcesso.Aluno, alunoId);
        var matricula = await _matriculaRepository.ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        return matricula?.ToDTO();
    }

    public async Task<bool> PossuiMatriculaAtivaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        _sessao?.ExigirPessoa(PessoaAcesso.Aluno, alunoId);
        return await _matriculaRepository.PossuiMatriculaAtiva(alunoId, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterAtivasAsync(int alunoId = 0, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Matriculas);
        var matriculas = await _matriculaRepository.ObterAtivas(alunoId, cancellationToken);
        return matriculas.ToDTOList();
    }

    public async Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int dias, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Matriculas);
        var matriculas = await _matriculaRepository.ObterVencendoEmDias(dias, cancellationToken);
        return matriculas.ToDTOList();
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(AppMatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Matriculas);
        var matriculas = await _matriculaRepository.ObterPorPlano(plano.ToDomain(), cancellationToken);
        return matriculas.ToDTOList();
    }
}