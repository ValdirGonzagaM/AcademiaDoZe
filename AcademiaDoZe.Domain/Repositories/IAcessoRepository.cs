using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Domain.Repositories;

public sealed record PessoaIdentificada(int Id, PessoaAcesso Tipo, string Nome, string Cpf, DateOnly Nascimento,
    string? Foto, ColaboradorVinculo? Vinculo);
public sealed record ResultadoVisita(Visita Visita, TimeSpan Permanencia, TimeSpan TotalDia, TimeSpan? LimiteDia, DateOnly? FimPlano);

public interface IAcessoRepository
{
    Task<PessoaIdentificada?> IdentificarAsync(PessoaAcesso tipo, string cpf, CancellationToken cancellationToken = default);
    Task<ResultadoVisita> RegistrarAsync(PessoaAcesso tipo, int pessoaId, bool entrada, DateTimeOffset agora, TimeZoneInfo fuso, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Visita>> ConsultarAsync(PessoaAcesso? tipo = null, int? pessoaId = null, CancellationToken cancellationToken = default);
}
