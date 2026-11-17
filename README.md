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
- Autenticação por CPF e senha, com permissões de administrador, atendente, instrutor e aluno.
- Entrada e saída com validação da matrícula, duração da visita e saldo do plano.
- Jornada diária acumulada de colaboradores: 8 horas para CLT e 6 horas para estágio.
- Histórico individual, troca de senha e relatórios de horários, permanência, jornadas e retenção contratual.
- Interface adaptável, temas claro/escuro e idiomas português, inglês e espanhol.

## Estrutura

- `AcademiaDoZe.Domain`: entidades, objetos de valor, enums e contratos de repositório.
- `AcademiaDoZe.Application`: serviços, DTOs, validações e mapeamentos.
- `AcademiaDoZe.Infrastructure`: acesso a dados e inicialização do banco SQLite.
- `AcademiaDoZe.Presentation.AppMaui`: interface .NET MAUI para Android e Windows, com alvos iOS e MacCatalyst habilitados no macOS.
- Projetos `*.Tests`: testes de domínio e integração.

O banco fica no diretório privado do aplicativo. A inicialização cria as tabelas ausentes e atualiza a estrutura antiga do aluno para aceitar e-mail opcional, preservando os cadastros, fotos, senhas e vínculos. E-mail de colaborador continua obrigatório. Cadastros com matrículas ou acessos vinculados possuem exclusão protegida pela integridade do banco.

Os horários de acesso são persistidos em UTC e apresentados no fuso do dispositivo. O limite de trabalho considera a soma das visitas no dia local, incluindo visitas que atravessam a meia-noite. A saída permanece disponível mesmo após o limite diário ou o vencimento do plano. O relógio do dispositivo deve estar correto.

## Execução

Use Visual Studio com suporte a .NET 10 e a carga de desenvolvimento .NET MAUI. Abra `AcademiaDoZe.slnx`, defina `AcademiaDoZe.Presentation.AppMaui` como projeto de inicialização e selecione Windows Machine ou um dispositivo Android.

Para Android, são necessários o workload `maui-android`, JDK 21 e Android SDK/API 36. A câmera depende das permissões concedidas ao aplicativo no dispositivo.

Para iOS e MacCatalyst, compile no macOS com Xcode e os workloads MAUI correspondentes. A assinatura, a câmera e os seletores nativos precisam ser conferidos no dispositivo de destino.

## Primeiro acesso

Não há usuário nem senha padrão. Sem administrador cadastrado, a tela de entrada permite cadastrar o endereço inicial e o primeiro administrador. Depois, entre como colaborador usando o CPF e a senha definidos no cadastro.

Em um banco existente sem administrador, a opção de configurar um cadastro CLT exige o CPF, a senha atual e a confirmação da promoção. Essa opção deixa de estar disponível quando já existe um administrador.

O administrador gerencia todos os módulos. O atendente gerencia alunos e matrículas e registra acessos de alunos. O instrutor registra acessos de alunos. Todos os perfis podem registrar o próprio acesso e alterar a própria senha; alunos consultam seus dados e contratos. A sessão é encerrada ao fechar o aplicativo. Idioma e tema são mantidos nas preferências do dispositivo.

## Testes

```bash
dotnet test AcademiaDoZe.Domain.Tests/AcademiaDoZe.Domain.Tests.csproj
dotnet test AcademiaDoZe.Application.Tests/AcademiaDoZe.Application.Tests.csproj
```

Os testes de integração desses projetos usam bancos SQLite temporários e incluem regras de acesso, permissões, concorrência, migração do banco e compatibilidade dos recursos de idioma. Os testes do projeto `AcademiaDoZe.Infrastructure.Tests` utilizam MySQL e exigem um banco de testes configurado separadamente, com a conexão na variável de ambiente `ACADEMIA_TEST_MYSQL`.
