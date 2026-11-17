using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Seguranca;

public enum PerfilUsuario { Administrador, Atendente, Instrutor, Aluno }
public enum Permissao { Alunos, Colaboradores, Matriculas, Logradouros, LerEnderecos, Relatorios, EntradaAluno, EntradaColaborador }
public sealed record UsuarioAutenticado(int Id, PessoaAcesso Tipo, string Nome, string Cpf, PerfilUsuario Perfil);

public sealed class SessaoUsuario
{
    public UsuarioAutenticado? Usuario { get; private set; }
    public bool Autenticado => Usuario != null;
    public bool ConfiguracaoInicial { get; internal set; }
    public event EventHandler? Alterada;
    internal void Iniciar(UsuarioAutenticado usuario) { Usuario = usuario; ConfiguracaoInicial = false; Alterada?.Invoke(this, EventArgs.Empty); }
    public void Encerrar() { Usuario = null; Alterada?.Invoke(this, EventArgs.Empty); }
    public bool Pode(Permissao permissao) => Usuario?.Perfil switch
    {
        PerfilUsuario.Administrador => true,
        PerfilUsuario.Atendente => permissao is Permissao.Alunos or Permissao.Matriculas or Permissao.LerEnderecos or Permissao.EntradaAluno,
        PerfilUsuario.Instrutor => permissao == Permissao.EntradaAluno,
        _ => false
    };
    public void Exigir(Permissao permissao)
    {
        if (!Pode(permissao)) throw new UnauthorizedAccessException("Seu perfil não possui permissão para esta operação.");
    }
    public void ExigirPessoa(PessoaAcesso tipo, int id)
    {
        if (Usuario is { } usuario && usuario.Tipo == tipo && usuario.Id == id) return;
        Exigir(tipo == PessoaAcesso.Aluno ? Permissao.Alunos : Permissao.Colaboradores);
    }
    public void ExigirRegistro(PessoaAcesso tipo, int id)
    {
        if (Usuario is { } usuario && usuario.Tipo == tipo && usuario.Id == id) return;
        Exigir(tipo == PessoaAcesso.Aluno ? Permissao.EntradaAluno : Permissao.EntradaColaborador);
    }
}
