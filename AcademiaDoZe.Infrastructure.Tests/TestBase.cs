using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;

//Valdir Gonzaga

namespace AcademiaDoZe.Infrastructure.Tests;
public abstract class TestBase
{
    protected string ConnectionString => "Server=localhost;Port=3307;Database=db_academia;Uid=root;Pwd=Miguel123!;";
    protected DatabaseType DatabaseType => DatabaseType.MySQL;

    protected async Task<Logradouro> CriarEInserirLogradouroAsync(string? cepPersonalizado = null)
    {
        var repo = new LogradouroRepository(ConnectionString, DatabaseType);

        string cepTexto = string.IsNullOrWhiteSpace(cepPersonalizado) ? "88000-000" : cepPersonalizado;
        var cep = Cep.Criar(cepTexto).Value!;

        var logradouro = Logradouro.Criar(
            id: 0,
            cep: cep,
            nome: "Jose Berlim" + Guid.NewGuid().ToString("55")[..5],
            bairro: "Universitario",
            cidade: "Lages",
            estado: LogradouroUf.SC.ToString(),
            pais: LogradouroPais.Brasil.ToString()
        ).Value!;

        return await repo.Adicionar(logradouro);
    }

    protected string GerarCpf()
    {
        var random = new Random();
        int sum = 0;
        int[] cpfArray = new int[9];

        for (int i = 0; i < 9; i++)
        {
            cpfArray[i] = random.Next(0, 9);
            sum += cpfArray[i] * (10 - i);
        }

        int remainder = sum % 11;
        int digit1 = remainder < 2 ? 0 : 11 - remainder;

        sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += cpfArray[i] * (11 - i);
        }
        sum += digit1 * 2;

        remainder = sum % 11;
        int digit2 = remainder < 2 ? 0 : 11 - remainder;

        return $"{string.Join("", cpfArray)}{digit1}{digit2}";
    }

    protected string GerarTelefone()
    {
        var random = new Random();
        return $"489{random.Next(10000000, 99999999)}";
    }

    protected string GerarEmail()
    {
        return $"teste_{Guid.NewGuid().ToString("N")[..6]}@dominio.com";
    }
}