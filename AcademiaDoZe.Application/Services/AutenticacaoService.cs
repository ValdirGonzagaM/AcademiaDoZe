using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Application.Seguranca;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public sealed class AutenticacaoService(IAlunoRepository alunos, IColaboradorRepository colaboradores, ILogradouroRepository logradouros, SessaoUsuario sessao)
{
    public async Task<bool> PrecisaAdministradorAsync()
    {
        var precisa = !await colaboradores.ExisteAdministradorAsync();
        sessao.ConfiguracaoInicial = precisa; return precisa;
    }
    public async Task EntrarAsync(PessoaAcesso tipo, string cpf, string senha)
    {
        if (!Enum.IsDefined(tipo)) throw new ArgumentException("Tipo de pessoa inválido.");
        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11) throw new ArgumentException("Informe o CPF completo com 11 dígitos.");
        Pessoa? pessoa = tipo == PessoaAcesso.Aluno ? await alunos.ObterPorCpfAsync(digitos) : await colaboradores.ObterPorCpfAsync(digitos);
        if (pessoa == null || !await Task.Run(() => PasswordHasher.Verify(senha, pessoa.Senha.Valor)))
            throw new UnauthorizedAccessException("CPF ou senha inválidos.");
        var perfil = pessoa is Colaborador colaborador ? (PerfilUsuario)(int)colaborador.Tipo : PerfilUsuario.Aluno;
        sessao.Iniciar(new(pessoa.Id, tipo, pessoa.Nome, pessoa.Cpf.Valor, perfil));
    }
    public async Task ConfigurarAdministradorExistenteAsync(string cpf, string senha)
    {
        var pessoa = await colaboradores.ObterPorCpfAsync(cpf);
        if (pessoa == null || !await Task.Run(() => PasswordHasher.Verify(senha, pessoa.Senha.Valor)))
            throw new UnauthorizedAccessException("CPF ou senha inválidos.");
        if (!await colaboradores.PromoverPrimeiroAdministradorAsync(pessoa.Id))
            throw new InvalidOperationException("Já existe um administrador ou o cadastro não possui vínculo CLT.");
        await EntrarAsync(PessoaAcesso.Colaborador, cpf, senha);
    }
    public async Task<ColaboradorDto> CriarPrimeiroAdministradorAsync(ColaboradorDto dto)
    {
        if (await colaboradores.ExisteAdministradorAsync()) throw new UnauthorizedAccessException("Já existe um administrador cadastrado. Faça login para continuar.");
        if (await colaboradores.CpfJaExisteAsync(dto.Cpf)) throw new ArgumentException("CPF já cadastrado. Use a opção de configurar o cadastro existente.");
        if (await colaboradores.EmailJaExisteAsync(dto.Email ?? "")) throw new ArgumentException("E-mail já cadastrado.");
        dto.Tipo = AppColaboradorTipo.Administrador; dto.Vinculo = AppColaboradorVinculo.CLT;
        if (dto.Endereco == null || await logradouros.ObterPorId(dto.Endereco.Id) == null) throw new ArgumentException("Selecione um logradouro cadastrado.");
        _ = dto.ToEntity();
        var hash = await Task.Run(() => PasswordHasher.Hash(dto.Senha!));
        var criado = await colaboradores.CriarPrimeiroAdministradorAsync(dto.ToEntity(hash));
        return criado.ToDTO();
    }
    public async Task TrocarMinhaSenhaAsync(string atual, string nova, string confirmacao)
    {
        var usuario = sessao.Usuario ?? throw new UnauthorizedAccessException("Faça login para continuar.");
        if (nova.Length < 6 || nova != confirmacao) throw new ArgumentException("Informe e confirme uma senha com pelo menos 6 caracteres.");
        Pessoa? pessoa = usuario.Tipo == PessoaAcesso.Aluno ? await alunos.ObterPorIdAsync(usuario.Id) : await colaboradores.ObterPorIdAsync(usuario.Id);
        if (pessoa == null || !await Task.Run(() => PasswordHasher.Verify(atual, pessoa.Senha.Valor)))
            throw new UnauthorizedAccessException("Senha atual inválida.");
        var hash = await Task.Run(() => PasswordHasher.Hash(nova));
        if (usuario.Tipo == PessoaAcesso.Aluno) await alunos.TrocarSenhaAsync(usuario.Id, hash);
        else await colaboradores.TrocarSenhaAsync(usuario.Id, hash);
    }
}
