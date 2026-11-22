# Flipper — Documentação do projeto

Jogo de pinball 3D desenvolvido em Unity e C#. O jogador lança uma bola, controla duas palhetas e procura acumular pontos ao atingir alvos, evitando que a bola caia para fora da mesa.

Esta documentação descreve os ficheiros e as configurações presentes no projeto em 22/11/2026. As funcionalidades descritas foram identificadas por análise dos scripts e das cenas guardadas; a execução no Editor e a geração de um executável não foram verificadas durante esta documentação.

## Índice

- [Tecnologias e dependências](#tecnologias-e-dependências)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Abrir e executar](#abrir-e-executar)
- [Controlos](#controlos)
- [Funcionamento da partida](#funcionamento-da-partida)
- [Scripts e responsabilidades](#scripts-e-responsabilidades)
- [Configuração da cena](#configuração-da-cena)
- [Ferramenta de criação da interface](#ferramenta-de-criação-da-interface)
- [Gerar uma build](#gerar-uma-build)
- [Verificação manual](#verificação-manual)
- [Pendências e resolução de problemas](#pendências-e-resolução-de-problemas)
- [Controlo de versões](#controlo-de-versões)

## Tecnologias e dependências

| Tecnologia | Versão/configuração | Utilização |
| --- | --- | --- |
| Unity | `6000.6.0f1` | Editor e motor do jogo |
| C# | Scripts `MonoBehaviour` | Lógica e ferramentas de edição |
| Universal Render Pipeline | `17.6.0` | Renderização |
| Input System | `1.20.0` | Ações de teclado e interface |
| Unity UI (uGUI) | `2.6.0` | Canvas, painéis e botões |
| TextMeshPro | Recursos em `Assets/TextMesh Pro/` | Textos da interface |
| ProBuilder | `6.1.2` | Modelação no Editor |
| Unity Test Framework | `1.8.0` | Dependência disponível para testes |

As versões do Editor e dos pacotes estão em [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt) e [manifest.json](Packages/manifest.json). O ficheiro `Packages/packages-lock.json` regista a resolução das dependências.

O manifesto inclui também AI Navigation, glTFast, Timeline, Visual Scripting, integrações de IDE e o pacote `com.coplaydev.unity-mcp`, obtido por Git. A presença destes pacotes não significa que todos sejam usados na lógica do pinball. A resolução do pacote Git requer acesso ao respetivo repositório e Git disponível no sistema.

O projeto está configurado para usar o novo Input System (`activeInputHandler: 1`). O nome do produto é `Flipper`.

## Estrutura do projeto

```text
UNITY_Flipper/
├── Assets/
│   ├── Animation/                  # Recursos de animação
│   ├── Editor/
│   │   └── CreatePinballUI.cs      # Gerador de interface no Editor
│   ├── Materials/                  # Materiais da mesa e dos objetos
│   ├── Models/                     # Modelos 3D
│   ├── PolyRonin/                  # Recursos visuais e cena de demonstração
│   ├── Scenes/
│   │   ├── Level_1_recup.unity     # Cena com os sistemas de jogo e UI identificados
│   │   ├── Level1.unity
│   │   └── Level2.unity
│   ├── Scripts/Gameplay/           # Lógica do pinball
│   ├── Settings/                   # Recursos de configuração
│   ├── TextMesh Pro/              # Recursos de texto
│   ├── Texture/                    # Texturas
│   ├── TutorialInfo/               # Conteúdo de orientação do template
│   ├── _Recovery/                  # Cena de recuperação
│   ├── computer_controls.inputactions
│   ├── Computer_System_input.inputactions
│   └── InputSystem_Actions.inputactions
├── Packages/                       # Manifesto e dependências
├── ProjectSettings/                # Configuração do Unity
└── README.md
```

`Library/`, `Temp/`, `Logs/` e os ficheiros de solução/projeto do IDE são gerados pelo Unity ou pelas ferramentas de desenvolvimento.

Existem várias cenas, mas não foi identificado nos scripts de gameplay um sistema de progressão ou de mudança entre níveis. Reiniciar e voltar ao menu são operações dentro da cena atual.

## Abrir e executar

1. Instalar o Unity Hub e o Editor **6000.6.0f1**, com o módulo de build da plataforma pretendida.
2. No Hub, adicionar a pasta raiz deste projeto, que contém `Assets/`, `Packages/` e `ProjectSettings/`.
3. Abrir o projeto e aguardar a importação dos recursos e a resolução dos pacotes.
4. Abrir [Assets/Scenes/Level_1_recup.unity](Assets/Scenes/Level_1_recup.unity), que contém os gestores, o HUD e os painéis de jogo identificados nesta análise.
5. Consultar a Console e resolver referências ou erros antes de entrar em Play Mode. Ver especialmente as pendências sobre a tag `Ball` e o controlador das palhetas.
6. Entrar em Play Mode e utilizar o botão de início da interface.

Se forem pedidos os recursos essenciais do TextMeshPro, importá-los através de `Window > TextMeshPro > Import TMP Essential Resources`.

## Controlos

As teclas abaixo estão definidas nos ficheiros de ações. Para funcionarem, as ações precisam de estar atribuídas aos campos dos componentes no Inspector.

### Jogabilidade

Ficheiro: [computer_controls.inputactions](Assets/computer_controls.inputactions), mapa `computer`.

| Ação | Teclas | Campo no componente |
| --- | --- | --- |
| `ball_launcher` | Seta para cima ou Espaço | `BallLauncher.actionLancer` |
| `flipper_left` | Seta para a esquerda ou Q | `leftFlipAction` do controlador das palhetas |
| `flipper_right` | Seta para a direita ou D | `rightFlipAction` do controlador das palhetas |

Na cena `Level_1_recup`, existem referências a este ficheiro no lançador e num dos componentes de controlo das palhetas.

### Menus

Ficheiro: [Computer_System_input.inputactions](Assets/Computer_System_input.inputactions).

| Mapa | Ação | Tecla |
| --- | --- | --- |
| `game` | `MenuStart` | Enter |
| `game` | `MenuResume` | Shift |
| `game` | `MenuRestart` | R |
| `game` | `MenuRestartPause` | P |
| `game` | `MenuMainMenu` | K |
| `game` | `enter` | Enter |
| `naviagation` | `up`, `down`, `left`, `right` | Setas direcionais |

O nome `naviagation` corresponde à grafia atual do recurso. O `MenuInputController` suporta navegação para cima/baixo, confirmação e atalhos diretos associados a botões. Não foi identificada uma instância deste componente em `Level_1_recup`; estes atalhos não devem ser considerados ativos apenas por existirem no ficheiro de ações.

O `GameManager` permite alternar pausa por uma ação atribuída a `pauseAction`. Esse campo está vazio na cena analisada, pelo que não existe uma tecla de pausa ligada por essa referência. A interface dispõe de um botão de pausa.

`InputSystem_Actions.inputactions` contém mapas genéricos `Player` e `UI`; deve ser distinguido dos recursos específicos de jogabilidade e menus.

## Funcionamento da partida

O `GameManager` coordena quatro estados:

| Estado | Comportamento |
| --- | --- |
| `Ready` | Mostra o menu inicial, imobiliza a bola e desativa os comportamentos de jogabilidade registados. |
| `Playing` | Liberta a bola, ativa os controlos registados e permite contar perdas de vidas. |
| `Paused` | Mostra o painel de pausa, desativa esses controlos e define `Time.timeScale = 0`. |
| `GameOver` | Imobiliza a bola, desativa os controlos e apresenta a pontuação final. |

Fluxo habitual: `Ready → Playing → GameOver`. Durante a partida é possível alternar entre `Playing` e `Paused`.

Ao preparar uma nova partida, o gestor repõe a pontuação a zero, restaura as vidas e reposiciona a bola. Reiniciar entra diretamente em `Playing`; voltar ao menu entra em `Ready`.

### Bola e colisões

- O lançador aplica uma impulsão no eixo Z local (`forward`) do ponto de lançamento quando a ação é premida e a bola está dentro do raio permitido. Não existe carregamento progressivo da força.
- As palhetas usam pivôs criados em execução, com `Rigidbody` cinemático e rotação em `FixedUpdate`. O eixo de rotação é `table.up`, ou o eixo Y global quando `table` não está atribuído.
- Os bumpers aplicam uma impulsão de afastamento à bola. O `BumperController` atual não adiciona pontos.
- Os alvos com `TargetController` aplicam impulsão, adicionam pontos e tentam ativar o trigger de animação `Impact`.

### Vidas e pontuação

O `LifeManager` começa, por predefinição, com **3 vidas**. Quando a bola entra numa `DeathZone` durante `Playing`, perde uma vida e fica imobilizada. Havendo vidas, é reposta após **1 segundo**, por predefinição. Uma proteção evita contar várias perdas enquanto a mesma queda está a ser processada.

O ponto de reposição é capturado em `Awake`: utiliza `ballSpawnPoint`, quando atribuído, ou a posição e rotação iniciais da bola. A reposição limpa as velocidades linear e angular. O atraso usa `WaitForSeconds`, pelo que também fica suspenso durante a pausa.

Sem vidas, `OnOutOfLives` avisa o `GameManager`, que passa para `GameOver` e pede ao `GameOverManager` para mostrar a pontuação final.

O `ScoreManager` mantém a pontuação em memória através de `ScoreManager.Instance`. O `ScoreUI` acompanha o evento `onScoreChanged`. Não foi identificada persistência de recordes ou de partidas.

## Scripts e responsabilidades

Os scripts de execução estão em [Assets/Scripts/Gameplay/](Assets/Scripts/Gameplay/).

| Ficheiro | Responsabilidade e API principal |
| --- | --- |
| `BallLauncher.cs` | Referências da bola e do ponto de lançamento; lê `actionLancer` e aplica a impulsão. |
| `FlipperController.cs` | Controla as duas palhetas e cria os pivôs físicos. A classe atualmente declarada chama-se `NewMonoBehaviourScript`. |
| `BumperController.cs` | Filtra colisões pela tag e aplica `bounceForce`. |
| `TargetController.cs` | Rebate a bola, atribui `scoreValue` e ativa a animação em `OnTargetHit()`. |
| `ScoreManager.cs` | Singleton da cena; `CurrentScore`, `AddScore(int)`, `ResetScore()`, `onScoreChanged` e `onScoreAdded`. |
| `ScoreUI.cs` | Atualiza um `TMP_Text` com a pontuação e um prefixo configurável. |
| `LifeManager.cs` | Vidas, reposição e ícones; `ResetLives()`, `ResetBall()`, `SetTracking(bool)`, `SetBallFrozen(bool)` e eventos de vidas. |
| `DeathZone.cs` | Reencaminha `OnTriggerEnter` para `LifeManager.HandleBallLost()`. |
| `GameManager.cs` | Estados; `StartGame()`, `PauseGame()`, `ResumeGame()`, `RestartGame()`, `ReturnToMainMenu()` e `OnStateChanged`. |
| `GameOverManager.cs` | Mostra/esconde o painel final com `Show(int)` e `Hide()`; emite `RestartRequested` e `MainMenuRequested`. |
| `MenuInputController.cs` | Liga ações a botões visíveis/interativos e gere seleção por teclado. |

### Valores de referência

Os valores do Inspector podem substituir as predefinições dos scripts.

| Parâmetro | Predefinição no código | Valor identificado em `Level_1_recup` |
| --- | --- | --- |
| Impulsão do lançamento | `12` | `20` |
| Raio de lançamento | `0.5` | `5` |
| Velocidade de ativação das palhetas | `500` graus/s | Consultar o componente pretendido no Inspector |
| Velocidade de retorno das palhetas | `250` graus/s | Consultar o Inspector |
| Ângulo ativo das palhetas | `45` graus | Consultar o Inspector |
| Força dos bumpers/alvos | `10` | `10` nos componentes encontrados |
| Pontos por alvo | `100` | `100` nos alvos encontrados |
| Vidas iniciais | `3` | `3` |
| Atraso de reposição | `1` segundo | `1` segundo |

## Configuração da cena

### Bola e mesa

1. Criar a tag `Ball` em `Project Settings > Tags and Layers` e atribuí-la ao objeto que participa nas colisões da bola.
2. Garantir que a bola tem `Rigidbody` e um collider 3D adequado.
3. Configurar os colliders da mesa, das palhetas, dos bumpers e dos alvos.
4. No lançador, atribuir `balle`, `pointDeLancement` e `actionLancer`. Orientar o eixo Z do ponto de lançamento na direção desejada.
5. No controlador das palhetas, atribuir `leftFlipper`, `rightFlipper`, as duas ações e, se necessário, `table`. Cada palheta precisa de um `BoxCollider` nela própria ou nos filhos.

### Gestores e interface

1. Manter `GameManager`, `LifeManager`, `GameOverManager` e `ScoreManager` em objetos ativos, fora de painéis que sejam desativados.
2. No `GameManager`, atribuir os três gestores, os painéis e os botões. Incluir o lançador e os controladores das palhetas em `gameplayBehaviours`.
3. No `LifeManager`, atribuir a bola, o texto de vidas e, opcionalmente, o ponto de reposição e o modelo de ícone.
4. Criar uma zona sob a saída da mesa com collider 3D em modo **Is Trigger**, componente `DeathZone` e referência ao `LifeManager`.
5. Adicionar `ScoreUI` ao texto TextMeshPro da pontuação e manter apenas um `ScoreManager` na cena.
6. No `GameOverManager`, atribuir painel, textos e botões de reinício/menu.
7. Garantir a existência de um `EventSystem` com `InputSystemUIInputModule` para os botões da interface.
8. Para atalhos de menu, adicionar e configurar `MenuInputController`, incluindo a lista ordenada de botões e as ações. A referência opcional ao `GameManager` desliga a navegação durante `Playing`; os atalhos diretos continuam sujeitos à visibilidade e interatividade dos botões.

Para os alvos animados, configurar um `Animator` no objeto, num ascendente ou num descendente, com um trigger cujo nome corresponda a `impactTrigger`.

## Ferramenta de criação da interface

[Assets/Editor/CreatePinballUI.cs](Assets/Editor/CreatePinballUI.cs) acrescenta o comando **Tools > Créer l’interface Pinball** ao Unity.

A ferramenta cria um `PinballCanvas` em Screen Space Overlay, com escala baseada numa resolução de referência de `1920 × 1080`, HUD de pontuação/vidas e painéis inicial, de pausa e de fim de jogo. Cria ou reutiliza os gestores, prepara o `EventSystem`, procura uma bola com a tag `Ball` e liga referências do Inspector. Também procura ou cria uma `DeathZone`.

Depois de executar o comando:

1. Confirmar a bola encontrada e as referências dos gestores.
2. Posicionar e dimensionar a `DeathZone` sob a saída da mesa.
3. Confirmar os comportamentos registados em `gameplayBehaviours`.
4. Atribuir a ação de pausa, se for pretendido um atalho de teclado.
5. Configurar separadamente o `MenuInputController`, caso se pretendam atalhos de menu.
6. Guardar a cena; a ferramenta marca-a como alterada, mas não a guarda automaticamente.

O comando recusa criar outra interface quando já existe um objeto chamado `PinballCanvas`. A cena `Level_1_recup` já contém esse objeto. A operação suporta Undo; não é necessário executá-la para utilizar uma interface já existente.

## Gerar uma build

**Pendência atual:** `ProjectSettings/EditorBuildSettings.asset` aponta para `Assets/Scenes/SampleScene.unity`, mas essa cena não existe nos ficheiros analisados.

1. Abrir `File > Build Profiles` no Unity.
2. Escolher a plataforma e confirmar que o módulo correspondente está instalado.
3. Corrigir a lista de cenas: remover a referência ausente e adicionar a cena de jogo pretendida, por exemplo `Assets/Scenes/Level_1_recup.unity`, como primeira cena ativa.
4. Resolver os erros de compilação e as referências da cena.
5. Gerar a build para uma pasta como `Builds/`.
6. Executar o resultado e verificar lançamento, palhetas, interface, pausa, perdas de vidas e reinício.

Não foi identificada uma configuração de integração contínua ou um script específico de build automatizada.

## Verificação manual

Não foram encontrados testes C# próprios em `Assets/`, embora o Unity Test Framework esteja instalado. A seguinte lista serve de roteiro de validação; não representa testes já executados.

- Ao abrir a cena, o menu inicial aparece e a bola permanece parada.
- Ao iniciar, as palhetas e o lançamento ficam disponíveis.
- A bola só recebe a impulsão do lançador dentro do raio configurado.
- Ambas as palhetas ativam e regressam à posição inicial, sem atravessar indevidamente a bola.
- Os bumpers rebatem a bola; os alvos atribuem pontos e ativam a animação configurada.
- A pontuação apresentada acompanha o total do `ScoreManager`.
- Uma queda retira apenas uma vida e repõe a bola após o atraso configurado.
- A pausa interrompe a simulação e permite retomar a partida.
- A última vida abre o painel final com a pontuação correta.
- Reiniciar repõe bola, vidas e pontuação; voltar ao menu apresenta o painel inicial.
- Os controlos e a interface também funcionam no executável gerado.

## Pendências e resolução de problemas

| Situação | Evidência/configuração a verificar | Ação sugerida |
| --- | --- | --- |
| Referência de build inválida | A lista de build contém `SampleScene.unity`, ausente. | Atualizar a lista de cenas antes de gerar o executável. |
| Tag da bola em falta | `TagManager.asset` tem `tags: []`, enquanto os scripts usam `Ball`. | Criar e atribuir a tag; `CompareTag` depende de uma tag válida. |
| Controlador das palhetas com nome inconsistente | `FlipperController.cs` declara `NewMonoBehaviourScript`. | Alinhar o nome do ficheiro e da classe numa alteração de código própria e verificar os componentes/referências das cenas no Unity. |
| Tecla de pausa sem efeito | `GameManager.pauseAction` está vazio na cena analisada. | Atribuir uma ação de pausa; não assumir que P ou Escape já estão configurados. |
| Atalhos de menu sem efeito | As ações existem, mas não foi encontrada uma instância de `MenuInputController` na cena analisada. | Adicionar o componente e ligar ações e botões. |
| Palheta sem resposta | A cena contém mais de um componente identificado como `NewMonoBehaviourScript`; um tem ações vazias. | Confirmar quais estão ativos, as palhetas atribuídas e as entradas em `gameplayBehaviours`. |
| Bola não lança | Referências ausentes, ação incorreta ou bola fora do raio. | Rever `balle`, `pointDeLancement`, `actionLancer`, distância e orientação. |
| Queda não desconta vidas | O jogo precisa de estar em `Playing`; o trigger e a referência ao gestor têm de estar configurados. | Verificar `Is Trigger`, colisões entre layers, identificação da bola e logs de `DeathZone`/`LifeManager`. |
| Mais do que uma bola/zona na cena | Foram encontrados objetos `Ball` e `Ball (1)`, além de vários componentes `DeathZone`. | Confirmar quais pertencem à mesa pretendida e qual `Rigidbody` é usado pelos gestores e lançador. |
| Alvo sem animação | `Animator` ou trigger ausente. | Configurar o `Animator` e o trigger `Impact`, ou ajustar `impactTrigger`. |
| Interface com idiomas misturados | A ferramenta cria rótulos franceses e os scripts atualizam vários textos em português. | Uniformizar os rótulos quando for definida a língua final da interface. |

Estas observações resultam da leitura dos ficheiros guardados e não substituem a verificação no Editor. Esta documentação não altera scripts, cenas ou configurações.

## Controlo de versões

Versionar `Assets/` com os respetivos ficheiros `.meta`, `Packages/` e `ProjectSettings/`. Os `.meta` preservam os GUID usados nas referências entre recursos e cenas.

O `.gitignore` existente exclui diretórios gerados como `Library/`, `Temp/`, `Logs/`, `UserSettings/` e `Builds/`, além de ficheiros de IDE. Ao adicionar ou mover recursos, preservar os respetivos `.meta`, preferencialmente efetuando essas operações dentro do Unity.

O projeto inclui recursos de terceiros, nomeadamente em `Assets/PolyRonin/` e `Assets/TextMesh Pro/`. Confirmar as licenças aplicáveis antes de redistribuir esses recursos; a presença no projeto não estabelece uma licença global para todo o conteúdo.
