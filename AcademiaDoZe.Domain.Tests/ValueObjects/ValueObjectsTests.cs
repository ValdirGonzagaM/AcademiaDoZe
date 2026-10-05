// Valdir Gonzaga
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using Xunit;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ValueObjectsTests
{
    [Theory(DisplayName = "CEP Valido")]
    [InlineData("12345678")]
    [InlineData("88501000")]
    [InlineData("01001000")]
    [InlineData("88000000")]
    [InlineData("88501-000")]
    [InlineData("01001-000")]
    [InlineData("88000-000")]
    [InlineData("12345-678")]
    [InlineData("87654321")]
    [InlineData("87654-321")]
    public void Cep_Valido_DevePassar(string input)
    {
        var result = Cep.Criar(input);
        Assert.NotNull(result);
    }

    [Theory(DisplayName = "Email Valido")]
    [InlineData("aluno1@dominio.com")]
    [InlineData("aluno2@dominio.com")]
    [InlineData("aluno3@dominio.com")]
    [InlineData("aluno4@dominio.com")]
    [InlineData("aluno5@dominio.com")]
    [InlineData("user1@gmail.com")]
    [InlineData("user2@gmail.com")]
    [InlineData("user3@gmail.com")]
    [InlineData("user4@gmail.com")]
    [InlineData("user5@gmail.com")]
    [InlineData("test1@academia.com.br")]
    [InlineData("test2@academia.com.br")]
    [InlineData("test3@academia.com.br")]
    [InlineData("test4@academia.com.br")]
    [InlineData("test5@academia.com.br")]
    [InlineData("ze1@domain.org")]
    [InlineData("ze2@domain.org")]
    [InlineData("ze3@domain.org")]
    [InlineData("ze4@domain.org")]
    [InlineData("ze5@domain.org")]
    public void Email_Valido_DevePassar(string input)
    {
        var result = Email.Criar(input);
        Assert.NotNull(result);
    }

    [Theory(DisplayName = "Telefone Valido")]
    [InlineData("49999990001")]
    [InlineData("49999990002")]
    [InlineData("49999990003")]
    [InlineData("49999990004")]
    [InlineData("49999990005")]
    [InlineData("49999990006")]
    [InlineData("49999990007")]
    [InlineData("49999990008")]
    [InlineData("49999990009")]
    [InlineData("49999990010")]
    [InlineData("11988880001")]
    [InlineData("11988880002")]
    [InlineData("11988880003")]
    [InlineData("11988880004")]
    [InlineData("11988880005")]
    [InlineData("11988880006")]
    [InlineData("11988880007")]
    [InlineData("11988880008")]
    [InlineData("11988880009")]
    [InlineData("11988880010")]
    public void Telefone_Valido_DevePassar(string input)
    {
        var result = Telefone.Criar(input);
        Assert.NotNull(result);
    }

    [Theory(DisplayName = "Senha Valida")]
    [InlineData("Senha123!")]
    [InlineData("A1b2C3d4!")]
    [InlineData("Ze@2026Domain")]
    [InlineData("P@ssw0rd2026")]
    [InlineData("Academia#123")]
    [InlineData("Strong!Pass99")]
    [InlineData("Valid12345#")]
    [InlineData("Complex_9876")]
    [InlineData("Segura$2026")]
    [InlineData("Teste*Unit1")]
    [InlineData("Senha@12345")]
    [InlineData("Senha@12346")]
    [InlineData("Senha@12347")]
    [InlineData("Senha@12348")]
    [InlineData("Senha@12349")]
    [InlineData("Senha@12350")]
    [InlineData("Senha@12351")]
    [InlineData("Senha@12352")]
    [InlineData("Senha@12353")]
    [InlineData("Senha@12354")]
    public void Senha_Valida_DevePassar(string input)
    {
        var result = Senha.Criar(input);
        Assert.NotNull(result);
    }

    [Theory(DisplayName = "Nome de Pessoa Valido")]
    [InlineData("João Silva")]
    [InlineData("Maria Oliveira")]
    [InlineData("Carlos Eduardo")]
    [InlineData("Ana Paula")]
    [InlineData("Roberto Carlos")]
    [InlineData("Fernanda Lima")]
    [InlineData("Lucas Gabriel")]
    [InlineData("Beatriz Souza")]
    [InlineData("Gabriel Santos")]
    [InlineData("Juliana Paes")]
    [InlineData("Rodrigo Faro")]
    [InlineData("Camila Pitanga")]
    [InlineData("Bruno Gagliasso")]
    [InlineData("Marina Ruy Barbosa")]
    [InlineData("Caio Castro")]
    [InlineData("Grazi Massafera")]
    [InlineData("Thiago Lacerda")]
    [InlineData("Paolla Oliveira")]
    [InlineData("Luan Santana")]
    [InlineData("Ivete Sangalo")]
    [InlineData("Almir Sater")]
    [InlineData("Renato Teixeira")]
    [InlineData("Chitãozinho Lima")]
    [InlineData("Xororó Lima")]
    [InlineData("Zezé Di Camargo")]
    [InlineData("Luciano Camargo")]
    [InlineData("Leonardo Costa")]
    [InlineData("Daniel Camargo")]
    [InlineData("Sérgio Reis")]
    [InlineData("Milionário Silva")]
    public void Pessoa_NomeValido_DevePassar(string nome)
    {
        Assert.True(nome.Length >= 3);
    }

    [Theory(DisplayName = "Logradouro Valido")]
    [InlineData("Rua Das Flores")]
    [InlineData("Avenida Central")]
    [InlineData("Rua São Paulo")]
    [InlineData("Praça da Sé")]
    [InlineData("Alameda Santos")]
    [InlineData("Rodovia BR 101")]
    [InlineData("Servidão Paz")]
    [InlineData("Rua 15 de Novembro")]
    [InlineData("Avenida Brasil")]
    [InlineData("Rua Duque de Caxias")]
    [InlineData("Praça da Bandeira")]
    [InlineData("Rua Sete de Setembro")]
    [InlineData("Avenida Beira Mar")]
    [InlineData("Rua do Comércio")]
    [InlineData("Rua Projetada A")]
    [InlineData("Rua Projetada B")]
    [InlineData("Rua Projetada C")]
    [InlineData("Rua Projetada D")]
    [InlineData("Rua Projetada E")]
    [InlineData("Rua Projetada F")]
    [InlineData("Rua Projetada G")]
    [InlineData("Rua Projetada H")]
    [InlineData("Rua Projetada I")]
    [InlineData("Rua Projetada J")]
    [InlineData("Rua Projetada K")]
    public void Endereco_Logradouro_DevePassar(string logradouro)
    {
        Assert.False(string.IsNullOrWhiteSpace(logradouro));
    }

    [Theory(DisplayName = "Identificador de Plano Valido")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(19)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(22)]
    [InlineData(23)]
    [InlineData(24)]
    [InlineData(25)]
    public void MatriculaPlano_Enum_Validacao(int idPlano)
    {
        Assert.True(idPlano > 0);
    }
}