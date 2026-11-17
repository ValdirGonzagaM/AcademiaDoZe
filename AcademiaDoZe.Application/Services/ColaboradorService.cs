using AcademiaDoZe.Application.Seguranca;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class ColaboradorService : IColaboradorService
{
    private readonly SessaoUsuario? _sessao;
    private readonly IColaboradorRepository _colaboradorRepository;

    public ColaboradorService(IColaboradorRepository colaboradorRepository, SessaoUsuario? sessao = null)
    {
        _sessao = sessao;
        _colaboradorRepository = colaboradorRepository ?? throw new ArgumentNullException(nameof(colaboradorRepository));
    }

    public Task<int> ContarAsync(CancellationToken cancellationToken = default) { _sessao?.Exigir(Permissao.Colaboradores); return _colaboradorRepository.ContarAsync(cancellationToken); }

    public async Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _sessao?.ExigirPessoa(PessoaAcesso.Colaborador, id);
        var colaborador = await _colaboradorRepository.ObterPorIdAsync(id, cancellationToken);
        return colaborador?.ToDTO();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Colaboradores);
        var colaboradores = await _colaboradorRepository.ObterTodosAsync(cancellationToken);
        return colaboradores.ToDTOList();
    }

    public async Task<ColaboradorDto> AdicionarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Colaboradores);
        if (await _colaboradorRepository.CpfJaExisteAsync(colaboradorDto.Cpf, colaboradorDto.Id == 0 ? null : colaboradorDto.Id, cancellationToken))
            throw new ArgumentException("CPF já cadastrado.");
        if (await _colaboradorRepository.EmailJaExisteAsync(colaboradorDto.Email ?? "", colaboradorDto.Id == 0 ? null : colaboradorDto.Id, cancellationToken))
            throw new ArgumentException("E-mail já cadastrado.");
        var entity = colaboradorDto.ToEntity();
        var hash = await Task.Run(() => AcademiaDoZe.Application.Security.PasswordHasher.Hash(colaboradorDto.Senha!), cancellationToken);
        entity = colaboradorDto.ToEntity(hash);
        var resultado = await _colaboradorRepository.AdicionarAsync(entity, cancellationToken);
        return resultado.ToDTO();
    }

    public async Task<ColaboradorDto> AtualizarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Colaboradores);
        var existente = await _colaboradorRepository.ObterPorIdAsync(colaboradorDto.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Cadastro não encontrado.");
        if (await _colaboradorRepository.CpfJaExisteAsync(colaboradorDto.Cpf, colaboradorDto.Id == 0 ? null : colaboradorDto.Id, cancellationToken))
            throw new ArgumentException("CPF já cadastrado.");
        if (await _colaboradorRepository.EmailJaExisteAsync(colaboradorDto.Email ?? "", colaboradorDto.Id == 0 ? null : colaboradorDto.Id, cancellationToken))
            throw new ArgumentException("E-mail já cadastrado.");
        var senha = existente.Senha.Valor;
        if (!string.IsNullOrWhiteSpace(colaboradorDto.Senha))
        {
            if (colaboradorDto.Senha.Length < 6) throw new ArgumentException("Senha deve possuir pelo menos 6 caracteres.");
            senha = await Task.Run(() => AcademiaDoZe.Application.Security.PasswordHasher.Hash(colaboradorDto.Senha), cancellationToken);
        }
        var entity = colaboradorDto.ToEntity(senha);
        var resultado = await _colaboradorRepository.AtualizarAsync(entity, cancellationToken);
        return resultado.ToDTO();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Colaboradores);
        return await _colaboradorRepository.RemoverAsync(id, cancellationToken);
    }

    public async Task<ColaboradorDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Colaboradores);
        var colaborador = await _colaboradorRepository.ObterPorCpfAsync(cpf, cancellationToken);
        return colaborador?.ToDTO();
    }

    public async Task<ColaboradorDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Colaboradores);
        var colaborador = await _colaboradorRepository.ObterPorEmailAsync(email, cancellationToken);
        return colaborador?.ToDTO();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorTipoAsync(AppColaboradorTipo tipo, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Colaboradores);
        var colaboradores = await _colaboradorRepository.ObterPorTipoAsync(tipo.ToDomain(), cancellationToken);
        return colaboradores.ToDTOList();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorVinculoAsync(AppColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Colaboradores);
        var colaboradores = await _colaboradorRepository.ObterPorVinculoAsync(vinculo.ToDomain(), cancellationToken);
        return colaboradores.ToDTOList();
    }

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Colaboradores);
        return await _colaboradorRepository.CpfJaExisteAsync(cpf, id, cancellationToken);
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        _sessao?.Exigir(Permissao.Colaboradores);
        return await _colaboradorRepository.EmailJaExisteAsync(email, id, cancellationToken);
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        _sessao?.ExigirPessoa(PessoaAcesso.Colaborador, id);
        if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 6) throw new ArgumentException("Senha deve possuir pelo menos 6 caracteres.");
        return await _colaboradorRepository.TrocarSenhaAsync(id, AcademiaDoZe.Application.Security.PasswordHasher.Hash(novaSenha), cancellationToken);
    }
}