# Academia do Zé - Sistema de Gestão

Projeto desenvolvido em C# e .NET seguindo os princípios de **Domain-Driven Design (DDD)** e arquitetura em camadas desacopladas.

## 🛠️ Arquitetura do Sistema

A solução está estruturada em 4 projetos principais:

1. **`AcademiaDoZe.Domain`**: Camada central contendo o modelo de domínio rico (Entidades como `Aluno`, `Colaborador`, `Matricula`), Value Objects (`Cpf`, `Email`, `Endereco`, `Telefone`), Enums e Interfaces de Repositório. Regras de validação estritas são aplicadas nas fábricas (`Criar`).
2. **`AcademiaDoZe.Application`**: Responsável pelos fluxos de uso da aplicação, Serviços de Negócio (`AlunoService`, `ColaboradorService`, `MatriculaService`), DTOs para tráfego de dados e extensões de mapeamento (`MappingExtensions`).
3. **`AcademiaDoZe.Infrastructure`**: Implementação do acesso a dados utilizando **ADO.NET (MySQL)** através da abstração `BaseRepository`, contendo os repositórios concretos e injeção de dependência.
4. **`AcademiaDoZe.Domain.Tests` / `Infrastructure.Tests`**: Cobertura de testes unitários validando entidades, serviços e regras de negócio.

## 🧪 Qualidade e Testes

- **CRUD de pessoas (SQLite):** testes de integração em `AcademiaDoZe.Application.Tests` cobrem aluno e colaborador, incluindo persistência de fotos, filtros e exclusão.
- **Domínio:** a suíte existente executa 173 casos; três expectativas de `CpfTests` divergem da implementação atual.
- **Android:** aplicação MAUI com módulos de alunos/colaboradores, câmera, galeria e banco SQLite inicializado no diretório privado do app.
- **Demonstração:** veja o [roteiro e instruções de execução](docs/DEMONSTRACAO.md).

## 🚀 Tecnologias Utilizadas

- **Linguagem:** C# (.NET)
- **Persistência:** ADO.NET / MySQL
- **Testes:** MSTest / xUnit
- **Padrões:** DDD, Repository Pattern, Factory Methods, DTOs & Mappings