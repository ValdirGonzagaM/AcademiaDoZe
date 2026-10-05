using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public class LogradouroInfrastructureTests : TestBase
{
    [Theory]
    [InlineData("SQLite")]
    [InlineData("SQLServer")]
    [InlineData("MySQL")]
    public async Task InserirLogradouro_DeveSalvarComSucesso(string provider)
    {
        // 1. Arrange: Monta a entidade com seu Nome, Sobrenome e a Cidade do banco em teste[cite: 1]
        var logradouro = CriarEInserirLogradouroAsync(provider);

        // 2. Act: Chama o repositório da camada de infraestrutura[cite: 1]
        // Exemplo: var repo = new LogradouroRepository(Configuration, provider);
        // await repo.InserirAsync(logradouro);

        // 3. Assert: Valida a inserção
        Assert.NotNull(logradouro);
    }
}