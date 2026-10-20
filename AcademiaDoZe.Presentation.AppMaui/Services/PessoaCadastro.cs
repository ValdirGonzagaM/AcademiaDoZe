using System.Globalization;
using System.Text;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.AppMaui.Services;

// Fluxo compartilhado; cada entidade mantém seus serviços e repositórios próprios.
public sealed class PessoaCadastro(IAlunoService alunos, IColaboradorService colaboradores, ILogradouroService logradouros)
{
    public Task<IEnumerable<LogradouroDto>> LogradourosAsync() => logradouros.ObterTodosAsync();
    public async Task<PessoaDto?> PorIdAsync(bool colaborador, int id) => colaborador
        ? await colaboradores.ObterPorIdAsync(id) : await alunos.ObterPorIdAsync(id);
    public async Task<PessoaDto?> PorCpfAsync(bool colaborador, string cpf)
    {
        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11) throw new ArgumentException("Informe o CPF completo com 11 dígitos.");
        return colaborador ? await colaboradores.ObterPorCpfAsync(digitos) : await alunos.ObterPorCpfAsync(digitos);
    }
    public async Task<LogradouroDto?> PorCepAsync(string cep)
    {
        var digitos = new string(cep.Where(char.IsDigit).ToArray());
        if (digitos.Length != 8) throw new ArgumentException("Informe o CEP completo com 8 dígitos.");
        return await logradouros.ObterPorCepAsync(digitos);
    }
    public async Task<IReadOnlyList<PessoaDto>> BuscarAsync(bool colaborador, string filtro, string texto)
    {
        texto = texto.Trim();
        IEnumerable<PessoaDto> pessoas;
        if (texto.Length == 0)
            pessoas = colaborador ? await colaboradores.ObterTodosAsync() : await alunos.ObterTodosAsync();
        else if (filtro == "CPF") return Lista(await PorCpfAsync(colaborador, texto));
        else if (filtro == "ID")
        {
            if (!int.TryParse(texto, out var id) || id <= 0) throw new ArgumentException("Informe um ID inteiro maior que zero.");
            return Lista(await PorIdAsync(colaborador, id));
        }
        else if (filtro == "E-mail")
            return Lista(colaborador ? await colaboradores.ObterPorEmailAsync(texto) : await alunos.ObterPorEmailAsync(texto));
        else if (filtro == "Nome")
        {
            pessoas = colaborador ? await colaboradores.ObterTodosAsync() : await alunos.ObterTodosAsync();
            pessoas = pessoas.Where(p => Normalizar(p.Nome).Contains(Normalizar(texto)));
        }
        else if (colaborador && filtro == "Tipo")
        {
            var tipos = Correspondentes<AppColaboradorTipo>(texto);
            var lista = new List<ColaboradorDto>();
            foreach (var tipo in tipos) lista.AddRange(await colaboradores.ObterPorTipoAsync(tipo));
            pessoas = lista.DistinctBy(p => p.Id);
        }
        else if (colaborador && filtro == "Vínculo")
        {
            var vinculos = Correspondentes<AppColaboradorVinculo>(texto);
            var lista = new List<ColaboradorDto>();
            foreach (var vinculo in vinculos) lista.AddRange(await colaboradores.ObterPorVinculoAsync(vinculo));
            pessoas = lista.DistinctBy(p => p.Id);
        }
        else throw new ArgumentException("Selecione um filtro válido.");
        return pessoas.ToList();
    }
    private static IReadOnlyList<PessoaDto> Lista(PessoaDto? pessoa) => pessoa == null ? [] : [pessoa];
    private static string Normalizar(string texto) => new string(texto.Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()).ToUpperInvariant();
    private static IEnumerable<T> Correspondentes<T>(string texto) where T : struct, Enum
    {
        var termo = Normalizar(texto);
        var valores = Enum.GetValues<T>().Where(v => Normalizar(v.ToString()).Contains(termo)
            || Normalizar(v.GetDisplayName()).Contains(termo)).ToArray();
        if (valores.Length == 0) throw new ArgumentException("Informe um tipo ou vínculo válido, conforme as opções do cadastro.");
        return valores;
    }
    public static void ValidarConfirmacaoSenha(string? senha, string? confirmacao, bool editando)
    {
        if (editando && string.IsNullOrEmpty(senha) && string.IsNullOrEmpty(confirmacao)) return;
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
            throw new ArgumentException("Informe uma senha com pelo menos 6 caracteres.");
        if (senha != confirmacao) throw new ArgumentException("A senha e a confirmação devem ser iguais.");
    }
    public async Task<PessoaDto> SalvarAsync(PessoaDto pessoa)
    {
        if (pessoa is ColaboradorDto colaborador)
            return pessoa.Id == 0 ? await colaboradores.AdicionarAsync(colaborador) : await colaboradores.AtualizarAsync(colaborador);
        var aluno = (AlunoDto)pessoa;
        return pessoa.Id == 0 ? await alunos.AdicionarAsync(aluno) : await alunos.AtualizarAsync(aluno);
    }
    public Task<bool> ExcluirAsync(PessoaDto pessoa) => pessoa is ColaboradorDto
        ? colaboradores.RemoverAsync(pessoa.Id) : alunos.RemoverAsync(pessoa.Id);
}
