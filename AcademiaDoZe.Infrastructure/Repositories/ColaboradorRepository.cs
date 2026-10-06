using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Infrastructure.Repositories;
public class ColaboradorRepository : PessoaRepository<Colaborador>, IColaboradorRepository
{
    public ColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType, true) { }
    protected override Colaborador Create(int id, string nome, string cpf, DateOnly nascimento,
        string telefone, string email, Logradouro logradouro, string numero,
        string complemento, string senha, Arquivo foto, DbDataReader reader)
    {
        var result = Colaborador.Criar(id, nome, cpf, nascimento, telefone, email, logradouro, numero, complemento, senha, foto, reader.GetDateOnlyValue("admissao"), (ColaboradorTipo)reader.GetInt32Value("tipo"), (ColaboradorVinculo)reader.GetInt32Value("vinculo"));
        if (result.IsFailure) throw new InvalidOperationException(string.Join("; ", result.Notifications.Select(n => n.Mensagem)));
        return result.Value!;
    }
    public async Task<IEnumerable<Colaborador>> ObterPorTipoAsync(ColaboradorTipo tipo, CancellationToken cancellationToken = default) =>
        await ReadAsync("p.tipo = @Value", (int)tipo, cancellationToken);
    public async Task<IEnumerable<Colaborador>> ObterPorVinculoAsync(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default) =>
        await ReadAsync("p.vinculo = @Value", (int)vinculo, cancellationToken);
}
