using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class ColaboradorRepository : BaseRepository, IColaboradorRepository
{
    public ColaboradorRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    public async Task<Colaborador?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Colaborador>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Colaborador> AdicionarAsync(Colaborador entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Colaborador> AtualizarAsync(Colaborador entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Colaborador?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Colaborador?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Colaborador>> ObterPorTipoAsync(ColaboradorTipo tipo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Colaborador>> ObterPorVinculoAsync(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}