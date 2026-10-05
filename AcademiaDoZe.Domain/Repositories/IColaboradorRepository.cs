using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Domain.Repositories;

public interface IColaboradorRepository
{
    Task<Colaborador?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Colaborador>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<Colaborador> AdicionarAsync(Colaborador entity, CancellationToken cancellationToken = default);
    Task<Colaborador> AtualizarAsync(Colaborador entity, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    Task<Colaborador?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<Colaborador?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Colaborador>> ObterPorTipoAsync(ColaboradorTipo tipo, CancellationToken cancellationToken = default);
    Task<IEnumerable<Colaborador>> ObterPorVinculoAsync(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default);
    Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default);
    Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default);
    Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default);
}