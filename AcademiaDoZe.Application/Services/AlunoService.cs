using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AlunoService : IAlunoService
{
    private readonly IAlunoRepository _alunoRepository;

    public AlunoService(IAlunoRepository alunoRepository)
    {
        _alunoRepository = alunoRepository ?? throw new ArgumentNullException(nameof(alunoRepository));
    }

    public async Task<AlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(id, cancellationToken);
        return aluno?.ToDTO();
    }

    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var alunos = await _alunoRepository.ObterTodosAsync(cancellationToken);
        return alunos.ToDTOList();
    }

    public async Task<AlunoDto> AdicionarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        if (await _alunoRepository.CpfJaExisteAsync(alunoDto.Cpf, alunoDto.Id == 0 ? null : alunoDto.Id, cancellationToken))
            throw new ArgumentException("CPF já cadastrado.");
        if (await _alunoRepository.EmailJaExisteAsync(alunoDto.Email ?? "", alunoDto.Id == 0 ? null : alunoDto.Id, cancellationToken))
            throw new ArgumentException("E-mail já cadastrado.");
        var entity = alunoDto.ToEntity();
        var hash = await Task.Run(() => AcademiaDoZe.Application.Security.PasswordHasher.Hash(alunoDto.Senha!), cancellationToken);
        entity = alunoDto.ToEntity(hash);
        var resultado = await _alunoRepository.AdicionarAsync(entity, cancellationToken);
        return resultado.ToDTO();
    }

    public async Task<AlunoDto> AtualizarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        var existente = await _alunoRepository.ObterPorIdAsync(alunoDto.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Cadastro não encontrado.");
        if (await _alunoRepository.CpfJaExisteAsync(alunoDto.Cpf, alunoDto.Id == 0 ? null : alunoDto.Id, cancellationToken))
            throw new ArgumentException("CPF já cadastrado.");
        if (await _alunoRepository.EmailJaExisteAsync(alunoDto.Email ?? "", alunoDto.Id == 0 ? null : alunoDto.Id, cancellationToken))
            throw new ArgumentException("E-mail já cadastrado.");
        var senha = existente.Senha.Valor;
        if (!string.IsNullOrWhiteSpace(alunoDto.Senha))
        {
            if (alunoDto.Senha.Length < 6) throw new ArgumentException("Senha deve possuir pelo menos 6 caracteres.");
            senha = await Task.Run(() => AcademiaDoZe.Application.Security.PasswordHasher.Hash(alunoDto.Senha), cancellationToken);
        }
        var entity = alunoDto.ToEntity(senha);
        var resultado = await _alunoRepository.AtualizarAsync(entity, cancellationToken);
        return resultado.ToDTO();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _alunoRepository.RemoverAsync(id, cancellationToken);
    }

    public async Task<AlunoDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepository.ObterPorCpfAsync(cpf, cancellationToken);
        return aluno?.ToDTO();
    }

    public async Task<AlunoDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepository.ObterPorEmailAsync(email, cancellationToken);
        return aluno?.ToDTO();
    }

    public async Task<IEnumerable<AlunoDto>> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        var alunos = await _alunoRepository.ObterPorNomeAsync(nome, cancellationToken);
        return alunos.ToDTOList();
    }

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        return await _alunoRepository.CpfJaExisteAsync(cpf, id, cancellationToken);
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        return await _alunoRepository.EmailJaExisteAsync(email, id, cancellationToken);
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 6) throw new ArgumentException("Senha deve possuir pelo menos 6 caracteres.");
        return await _alunoRepository.TrocarSenhaAsync(id, AcademiaDoZe.Application.Security.PasswordHasher.Hash(novaSenha), cancellationToken);
    }
}