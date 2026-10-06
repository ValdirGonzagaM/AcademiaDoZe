using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.AppMaui.Services;

// Os dois módulos usam o mesmo fluxo de tela e seus respectivos serviços de aplicação.
public sealed class PessoaCadastro(IAlunoService alunos, IColaboradorService colaboradores, ILogradouroService logradouros)
{
    public Task<IEnumerable<LogradouroDto>> LogradourosAsync() => logradouros.ObterTodosAsync();
    public async Task<IReadOnlyList<PessoaDto>> BuscarAsync(bool colaborador, string filtro, string texto)
    {
        texto = texto.Trim();
        IEnumerable<PessoaDto> pessoas;
        if (filtro == "CPF" && texto.Length > 0)
        {
            var cpf = new string(texto.Where(char.IsDigit).ToArray());
            if (cpf.Length != 11) throw new ArgumentException("Informe o CPF completo com 11 dígitos.");
            PessoaDto? pessoa = colaborador ? await colaboradores.ObterPorCpfAsync(cpf) : await alunos.ObterPorCpfAsync(cpf);
            pessoas = pessoa == null ? [] : [pessoa];
        }
        else
        {
            pessoas = colaborador ? await colaboradores.ObterTodosAsync() : await alunos.ObterTodosAsync();
            if (texto.Length > 0) pessoas = pessoas.Where(p => p.Nome.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }
        return pessoas.ToList();
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
