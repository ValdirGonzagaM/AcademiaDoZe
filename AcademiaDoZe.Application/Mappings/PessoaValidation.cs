using AcademiaDoZe.Domain.Common;
namespace AcademiaDoZe.Application.Mappings;

internal static class PessoaValidation
{
    public static string Mensagem(IEnumerable<Notification> notifications) => string.Join("; ", notifications.Select(n => n.Mensagem switch
    {
        "NOME_OBRIGATORIO" => "Informe o nome completo.",
        "DATA_NASCIMENTO_OBRIGATORIO" => "Informe a data de nascimento.",
        "DATA_NASCIMENTO_MINIMA_INVALIDA" => "O colaborador deve ter pelo menos 12 anos.",
        "DATA_ADMISSAO_OBRIGATORIO" => "Informe a data de admissão.",
        "DATA_ADMISSAO_MAIOR_ATUAL" => "A admissão não pode ser uma data futura.",
        "TIPO_COLABORADOR_INVALIDO" => "Selecione o tipo do colaborador.",
        "VINCULO_COLABORADOR_INVALIDO" => "Selecione o vínculo do colaborador.",
        "ADMINISTRADOR_CLT_INVALIDO" => "O administrador deve ter vínculo CLT.",
        "LOGRADOURO_OBRIGATORIO" => "Selecione um logradouro.",
        "NUMERO_OBRIGATORIO" => "Informe o número do endereço.",
        _ => n.Mensagem
    }));
}
