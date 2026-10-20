# Demonstração — Academia do Zé / Valdir Gonzaga

A aplicação usa SQLite no diretório privado do aplicativo. Não exige MySQL para os fluxos de aluno e colaborador. Cadastre primeiro um endereço em **Logradouros → Novo logradouro** (CEP, rua, bairro, cidade, UF e país).

## Roteiro de Dashboard e Logradouros (slide dev09de20)

1. Mostre o funcionamento do menu e abra a **Dashboard**. Deixe visíveis a logo, o nome **Valdir Gonzaga**, o total de logradouros e o botão de acesso a Logradouros.
2. Abra **Logradouros → Novo logradouro**. Preencha CEP, Nome / Rua, Bairro, Cidade, UF e País. No campo **Nome / Rua**, digite obrigatoriamente seu **nome completo** (por exemplo, **Valdir Gonzaga**). Use um CEP ainda não cadastrado. Salve e mostre a confirmação.
3. Na listagem, mostre o registro salvo. Selecione o filtro **Nome**, informe o nome cadastrado e clique em **Buscar**. O filtro também aceita parte do nome; para CEP, informe os oito dígitos.
4. Clique em **Editar** no registro encontrado. Altere **Bairro** para exatamente **Abc Bolinhas**, salve e mostre esse texto no cartão da listagem. O filtro permanece ao retornar da edição. Abra **Editar** novamente, se desejar mostrar os dados recuperados do banco, e depois cancele.
5. Clique em **Excluir**, confirme em **Excluir** e mostre a confirmação e a lista sem o registro. Repita a busca para demonstrar que o registro saiu do banco. Volte à Dashboard para mostrar o total atualizado.

Use neste ciclo um logradouro exclusivo para a demonstração, sem vínculo com alunos ou colaboradores. O aplicativo protege endereços em uso; nesse caso, altere o endereço dos cadastros vinculados antes de excluir. Para os ciclos de pessoas abaixo, cadastre outro endereço após concluir o vídeo de Logradouros.

O slide também pede vídeo com boa qualidade: grave a própria tela, com texto legível e tempo para observar as confirmações e os resultados. A execução e a gravação no Windows devem ser verificadas no seu computador.

## Colaboradores conforme o slide dev11de20

Acesse **Colaboradores** no menu ou na Dashboard. Preencha os dados pessoais, a foto, o endereço, a data de admissão, o tipo e o vínculo.

- **Listagem:** foto, ID, nome, CPF, e-mail e, para colaborador, tipo, vínculo e admissão. Use os botões **Editar** e **Excluir** do cartão.
- **Filtros:** CPF completo, parte do nome, ID, e-mail, tipo (Administrador, Atendente, Instrutor) ou vínculo (CLT, Estagiário). A busca de nomes e enums aceita diferenças de maiúsculas e acentos. Deixe o texto vazio para listar todos.
- **Buscar CPF no formulário:** informe os 11 dígitos e clique em **Buscar CPF**. Se houver cadastro, confirme **Carregar** para abrir os dados para edição. Isso substitui os campos e a foto atuais, sem criar outro registro. Senha e confirmação permanecem vazias. Se o CPF não existir, preencha um novo cadastro.
- **Buscar CEP:** informe os oito dígitos de um logradouro já cadastrado e clique em **Buscar CEP**. A tela seleciona o endereço e mostra rua, bairro, cidade, UF e país. Preencha número e complemento. Também é possível selecionar o logradouro diretamente.
- **Senha:** no cadastro, preencha senha e confirmação iguais, com pelo menos seis caracteres. Na edição, deixe ambos vazios para manter a senha existente ou informe e confirme uma nova senha.
- **Foto:** use **Tirar foto** ou **Galeria**. Cancelar a escolha preserva a imagem anterior. A correção que evita o processamento de streams fechados no Windows permanece ativa.
- **Tipo e vínculo:** os seletores exibem suas descrições, incluindo **Estagiário**. O domínio exige vínculo CLT para Administrador. Datas de nascimento e admissão são convertidas entre os seletores de data e DateOnly.
- **Dashboard:** totais reais de colaboradores e alunos são consultados sem carregar as fotos e atualizados quando a tela reaparece. A logo e a identificação Valdir Gonzaga permanecem.

As telas usam espaçamentos menores no celular e limitam a largura dos formulários/listas em telas grandes. Valide a execução visual e a câmera no dispositivo usado para gravar.

## Roteiro de colaboradores e alunos

Execute todo o ciclo de colaborador e depois todo o ciclo de aluno, sem cortes que escondam o salvamento ou o resultado da busca:

| Etapa | Colaborador | Aluno |
|---|---|---|
| Cadastro | Novo colaborador, preencher dados, buscar/selecionar endereço, informar senha e confirmação, tipo e vínculo | Novo aluno, preencher dados, buscar/selecionar endereço e informar senha e confirmação |
| Câmera | Tocar **Tirar foto**, autorizar a câmera, capturar, confirmar a foto e salvar | Repetir pela câmera |
| Busca | Na listagem, selecionar **CPF**, digitar o CPF completo e tocar **Buscar** | Repetir com o CPF do aluno |
| Edição | Tocar **Editar**, mudar o nome para **zé dos testes**, tocar **Galeria**, escolher uma foto diferente e salvar | Repetir no aluno |
| Verificação | Buscar novamente pelo CPF, mostrar nome e foto novos; abrir o cadastro para mostrar a foto maior | Repetir no aluno |
| Exclusão | Tocar **Excluir**, confirmar e mostrar a listagem sem o registro | Repetir no aluno |

Use dados fictícios distintos: por exemplo, CPF `08615141908` no colaborador e `52998224725` no aluno; e-mails diferentes e senha com pelo menos seis caracteres. Para colaborador Administrador, use vínculo CLT. A senha e sua confirmação podem ficar em branco na edição para manter a atual. Cancelar câmera/galeria mantém a foto anterior.

## Gravação no emulador

1. Crie um AVD Android com câmera traseira **VirtualScene**, **Emulated** ou webcam, conforme os recursos disponíveis. A captura deve acontecer dentro do aplicativo de câmera; selecionar uma imagem diretamente não demonstra esta etapa.
2. Coloque uma foto diferente em Pictures/DCIM no emulador e indexe-a na galeria. Mostre a escolha dessa foto durante a edição.
3. Grave em modo retrato, idealmente 1080 × 1920, com texto legível e tempo para observar os resultados. Exiba a Dashboard e a identificação **Valdir Gonzaga** no início.
4. Grave cada ciclo com o gravador do emulador/Android Studio ou `adb shell screenrecord --bit-rate 12000000 /sdcard/colaborador.mp4`. O `screenrecord` pode ter limite de três minutos por arquivo; use um arquivo por entidade e mantenha o ciclo completo em cada trecho. Copie os arquivos com `adb pull` e junte-os preservando a ordem.
5. Identifique o vídeo como demonstração em emulador quando a câmera usar cena virtual; isso não comprova captura em aparelho físico.

## Validação automatizada

```bash
dotnet test AcademiaDoZe.Application.Tests/AcademiaDoZe.Application.Tests.csproj
```

A suíte usa um SQLite temporário por teste e cobre o ciclo completo de Logradouros (nome completo, filtro, edição para **Abc Bolinhas**, reabertura e exclusão), proteção de endereços vinculados, rejeição de CEP duplicado e os dois ciclos de CRUD de pessoas, normalização do CPF na busca, persistência dos bytes de foto após reabrir o banco, endereço/complemento, senha preservada e armazenada com hash, filtros (CPF, nome, ID, e-mail, tipo e vínculo), buscas de CPF/CEP do formulário, confirmação de senha, contagens sem fotos e rejeição de duplicatas/dados inválidos. Ela não substitui a execução de câmera, galeria e gravação na interface.

Os 174 testes de domínio passaram após corrigir as expectativas de `CpfTests`. A regra atual aceita 11 dígitos após normalização; ela não calcula os dígitos verificadores do CPF. Os testes MySQL preexistentes dependem de banco externo e não são necessários para o fluxo SQLite do aplicativo.

## Compilação

Use .NET 10, workload `maui-android`, JDK 21 e Android SDK/API 36. Compile Android com `dotnet build AcademiaDoZe.Presentation.AppMaui/AcademiaDoZe.Presentation.AppMaui.csproj -f net10.0-android`. Em host Linux, informe `AndroidSdkDirectory` e `JavaSdkDirectory` se as ferramentas não estiverem nos locais padrão. Para um APK de demonstração autossuficiente, use `-p:EmbedAssembliesIntoApk=true`; escolha `RuntimeIdentifier=android-x64` para emulador x86_64 ou `android-arm64` para dispositivo ARM64. Use a assinatura de desenvolvimento apenas para testes.

Permissões de câmera e biblioteca foram declaradas também nos arquivos iOS/MacCatalyst, mas estes alvos não estão habilitados no projeto atual e precisam de macOS/toolchain Apple para compilação e validação.

## Resultado desta tentativa na nuvem

- Compilação Android x86_64 concluída; APK de desenvolvimento em `artifacts/AcademiaDoZe-emulador.apk`.
- Sete testes novos de integração passaram. Após a correção dos testes de CPF, a suíte de domínio passou com 174 aprovados e nenhuma falha.
- O host não oferece KVM/virtualização de CPU (`emulator-check accel` informa ausência de vmx/svm). Foi tentada a emulação TCG por software.
- O Android chegou a concluir o boot, mas exibiu **Process system isn't responding**; a instalação e a abertura do aplicativo não foram concluídas. Captura dessa falha em `artifacts/emulador-sem-aceleracao.png`.
- Não foi produzido vídeo dos ciclos, nem foi validada a câmera/galeria na interface. Execute o APK num emulador x86_64 com aceleração de hardware para concluir o roteiro. Este resultado é uma limitação do ambiente de execução; não estabelece o funcionamento visual do aplicativo.
