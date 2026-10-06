using System.Data.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Infrastructure.Repositories;
public class AlunoRepository : PessoaRepository<Aluno>, IAlunoRepository
{
    public AlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType, false) { }
    protected override Aluno Create(int id, string nome, string cpf, DateOnly nascimento,
        string telefone, string email, Logradouro logradouro, string numero,
        string complemento, string senha, Arquivo foto, DbDataReader reader)
    {
        var result = Aluno.Criar(id, nome, cpf, nascimento, telefone, email, logradouro, numero, complemento, senha, foto);
        if (result.IsFailure) throw new InvalidOperationException(string.Join("; ", result.Notifications.Select(n => n.Mensagem)));
        return result.Value!;
    }
    public async Task<IEnumerable<Aluno>> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default) =>
        (await ReadAsync(cancellationToken: cancellationToken)).Where(p => p.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase));
    public Task<Aluno> Adicionar(Aluno entity, CancellationToken cancellationToken = default) => AdicionarAsync(entity, cancellationToken);
}
