# Academia do Zé

Sistema de gestão de academia em C# e .NET 10, com interface .NET MAUI e persistência local em SQLite.

## Funcionalidades

- Cadastro, consulta, edição e exclusão de logradouros, alunos e colaboradores.
- Busca por CPF e CEP, fotos por câmera ou galeria e confirmação de senha.
- Matrículas com planos mensal, trimestral, semestral e anual, data final calculada, restrições médicas e laudo em imagem.
- Bloqueio de nova matrícula enquanto o aluno possuir matrícula vigente ou agendada.
- Laudo obrigatório para menores de 16 anos e alunos com restrições médicas.
- Filtros de matrículas por ID, aluno, CPF, nome, plano, situação e prazo de vencimento.
- Dashboard com totais atualizados e acesso aos módulos.

## Estrutura

- `AcademiaDoZe.Domain`: entidades, objetos de valor, enums e contratos de repositório.
- `AcademiaDoZe.Application`: serviços, DTOs, validações e mapeamentos.
- `AcademiaDoZe.Infrastructure`: acesso a dados e inicialização do banco SQLite.
- `AcademiaDoZe.Presentation.AppMaui`: interface para Android e Windows.
- Projetos `*.Tests`: testes de domínio e integração.

O banco fica no diretório privado do aplicativo. A inicialização cria as tabelas ausentes sem apagar os cadastros existentes. Exclua as matrículas antes de remover um aluno que esteja vinculado a elas.

## Execução

Use Visual Studio com suporte a .NET 10 e a carga de desenvolvimento .NET MAUI. Abra `AcademiaDoZe.slnx`, defina `AcademiaDoZe.Presentation.AppMaui` como projeto de inicialização e selecione Windows Machine ou um dispositivo Android.

Para Android, são necessários o workload `maui-android`, JDK 21 e Android SDK/API 36. A câmera depende das permissões concedidas ao aplicativo no dispositivo.

## Testes

```bash
dotnet test AcademiaDoZe.Domain.Tests/AcademiaDoZe.Domain.Tests.csproj
dotnet test AcademiaDoZe.Application.Tests/AcademiaDoZe.Application.Tests.csproj
```

Os testes de integração desses projetos usam bancos SQLite temporários. Os testes do projeto `AcademiaDoZe.Infrastructure.Tests` utilizam MySQL e exigem um banco configurado separadamente.
