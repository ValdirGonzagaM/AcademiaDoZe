using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Repositories;

public interface IAlunoRepository
{
    Task<Aluno?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Aluno>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<Aluno> AdicionarAsync(Aluno entity, CancellationToken cancellationToken = default);
    Task<Aluno> AtualizarAsync(Aluno entity, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    Task<Aluno?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<Aluno?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Aluno>> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);
    Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default);
    Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default);
    Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default);
}