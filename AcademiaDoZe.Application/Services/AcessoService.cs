using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Application.Seguranca;

namespace AcademiaDoZe.Application.Services;

public sealed class AcessoService(IAcessoRepository repository, SessaoUsuario sessao, TimeProvider clock)
{
    public DateTimeOffset Agora => clock.GetUtcNow();
    public TimeZoneInfo Fuso => clock.LocalTimeZone;
    public async Task<PessoaIdentificada> LocalizarAsync(PessoaAcesso tipo, string cpf)
    {
        var pessoa = await repository.IdentificarAsync(tipo, cpf) ?? throw new ArgumentException("Cadastro não encontrado.");
        sessao.ExigirRegistro(tipo, pessoa.Id);
        return pessoa;
    }
    public Task<ResultadoVisita> RegistrarAsync(PessoaAcesso tipo, int id, bool entrada)
    {
        sessao.ExigirRegistro(tipo, id);
        return repository.RegistrarAsync(tipo, id, entrada, Agora, Fuso);
    }
    public Task<IReadOnlyList<Visita>> HistoricoAsync(PessoaAcesso tipo, int id)
    {
        sessao.ExigirRegistro(tipo, id);
        return repository.ConsultarAsync(tipo, id);
    }
    public Task<IReadOnlyList<Visita>> RelatorioAsync()
    {
        sessao.Exigir(Permissao.Relatorios);
        return repository.ConsultarAsync();
    }
}
