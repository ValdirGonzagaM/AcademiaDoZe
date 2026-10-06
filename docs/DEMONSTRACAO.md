# Demonstração — Academia do Zé / Valdir Gonzaga

A aplicação usa SQLite no diretório privado do aplicativo. Não exige MySQL para os fluxos de aluno e colaborador. Cadastre primeiro um endereço em **Logradouros → Novo logradouro** (CEP, rua, bairro, cidade, UF e país).

## Roteiro obrigatório

Execute todo o ciclo de colaborador e depois todo o ciclo de aluno, sem cortes que escondam o salvamento ou o resultado da busca:

| Etapa | Colaborador | Aluno |
|---|---|---|
| Cadastro | Novo colaborador, preencher dados, selecionar endereço, informar senha, tipo e vínculo | Novo aluno, preencher dados, selecionar endereço e informar senha |
| Câmera | Tocar **Tirar foto**, autorizar a câmera, capturar, confirmar a foto e salvar | Repetir pela câmera |
| Busca | Na listagem, selecionar **CPF**, digitar o CPF completo e tocar **Buscar** | Repetir com o CPF do aluno |
| Edição | Tocar **Editar**, mudar o nome para **zé dos testes**, tocar **Galeria**, escolher uma foto diferente e salvar | Repetir no aluno |
| Verificação | Buscar novamente pelo CPF, mostrar nome e foto novos; abrir o cadastro para mostrar a foto maior | Repetir no aluno |
| Exclusão | Tocar **Excluir**, confirmar e mostrar a listagem sem o registro | Repetir no aluno |

Use dados fictícios distintos: por exemplo, CPF `08615141908` no colaborador e `52998224725` no aluno; e-mails diferentes e senha com pelo menos seis caracteres. Para colaborador Administrador, use vínculo CLT. A senha pode ficar em branco na edição para manter a atual. Cancelar câmera/galeria mantém a foto anterior.

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

A suíte usa um SQLite temporário por teste e cobre os dois ciclos de CRUD, normalização do CPF na busca, persistência dos bytes de foto após reabrir o banco, endereço/complemento, senha preservada e armazenada com hash, filtros e rejeição de duplicatas/dados inválidos. Ela não substitui a execução de câmera, galeria e gravação na interface.

Os testes de domínio preexistentes têm três falhas em `CpfTests`, por divergências entre suas expectativas e a implementação. Os testes MySQL preexistentes dependem de banco externo e não são necessários para o fluxo SQLite do aplicativo.

## Compilação

Use .NET 10, workload `maui-android`, JDK 21 e Android SDK/API 36. Compile Android com `dotnet build AcademiaDoZe.Presentation.AppMaui/AcademiaDoZe.Presentation.AppMaui.csproj -f net10.0-android`. Em host Linux, informe `AndroidSdkDirectory` e `JavaSdkDirectory` se as ferramentas não estiverem nos locais padrão. Para um APK de demonstração autossuficiente, use `-p:EmbedAssembliesIntoApk=true`; escolha `RuntimeIdentifier=android-x64` para emulador x86_64 ou `android-arm64` para dispositivo ARM64. Use a assinatura de desenvolvimento apenas para testes.

Permissões de câmera e biblioteca foram declaradas também nos arquivos iOS/MacCatalyst, mas estes alvos não estão habilitados no projeto atual e precisam de macOS/toolchain Apple para compilação e validação.

## Resultado desta tentativa na nuvem

- Compilação Android x86_64 concluída; APK de desenvolvimento em `artifacts/AcademiaDoZe-emulador.apk`.
- Sete testes novos de integração passaram. A suíte de domínio manteve 170 aprovados e as três falhas preexistentes de CPF.
- O host não oferece KVM/virtualização de CPU (`emulator-check accel` informa ausência de vmx/svm). Foi tentada a emulação TCG por software.
- O Android chegou a concluir o boot, mas exibiu **Process system isn't responding**; a instalação e a abertura do aplicativo não foram concluídas. Captura dessa falha em `artifacts/emulador-sem-aceleracao.png`.
- Não foi produzido vídeo dos ciclos, nem foi validada a câmera/galeria na interface. Execute o APK num emulador x86_64 com aceleração de hardware para concluir o roteiro. Este resultado é uma limitação do ambiente de execução; não estabelece o funcionamento visual do aplicativo.
