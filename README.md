# Flipper — Documentation du projet

Jeu de flipper 3D développé avec Unity et C#. Le joueur lance une bille, commande deux palettes et cherche à accumuler des points en touchant des cibles, tout en évitant que la bille ne tombe de la table.

Cette documentation décrit les fichiers et les paramètres présents dans le projet au 22/11/2026. Les fonctionnalités décrites ont été repérées à partir des scripts et des scènes enregistrées ; l'exécution dans l'éditeur et la génération d'un exécutable n'ont pas été vérifiées pendant cette analyse.

## Sommaire

- [Technologies et dépendances](#technologies-et-dépendances)
- [Structure du projet](#structure-du-projet)
- [Ouvrir et lancer le projet](#ouvrir-et-lancer-le-projet)
- [Commandes](#commandes)
- [Déroulement d'une partie](#déroulement-dune-partie)
- [Scripts et responsabilités](#scripts-et-responsabilités)
- [Configuration de la scène](#configuration-de-la-scène)
- [Outil de création de l'interface](#outil-de-création-de-linterface)
- [Générer une build](#générer-une-build)
- [Vérification manuelle](#vérification-manuelle)
- [Problèmes connus et dépannage](#problèmes-connus-et-dépannage)
- [Gestion des versions](#gestion-des-versions)

## Technologies et dépendances

| Technologie | Version/configuration | Utilisation |
| --- | --- | --- |
| Unity | `6000.6.0f1` | Éditeur et moteur du jeu |
| C# | Scripts `MonoBehaviour` | Logique et outils d'édition |
| Universal Render Pipeline | `17.6.0` | Rendu |
| Input System | `1.20.0` | Actions clavier et interface |
| Unity UI (uGUI) | `2.6.0` | Canevas, panneaux et boutons |
| TextMeshPro | Ressources dans `Assets/TextMesh Pro/` | Textes de l'interface |
| ProBuilder | `6.1.2` | Modélisation dans l'éditeur |
| Unity Test Framework | `1.8.0` | Dépendance disponible pour les tests |

Les versions de l'éditeur et des paquets sont indiquées dans [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt) et [manifest.json](Packages/manifest.json). Le fichier `Packages/packages-lock.json` enregistre la résolution des dépendances.

Le manifeste comprend également AI Navigation, glTFast, Timeline, Visual Scripting et des intégrations d'IDE. Le paquet MCP pour Unity a été retiré des dépendances du projet : il ne sera plus proposé à l'installation lors de son ouverture.

Le projet est configuré pour utiliser le nouveau Input System (`activeInputHandler: 1`). Le nom du produit est `Flipper`.

## Structure du projet

```text
UNITY_Flipper/
├── Assets/
│   ├── Animation/                  # Ressources d'animation
│   ├── Editor/
│   │   └── CreatePinballUI.cs      # Générateur d'interface dans l'éditeur
│   ├── Materials/                  # Matériaux de la table et des objets
│   ├── Models/                     # Modèles 3D
│   ├── PolyRonin/                  # Ressources visuelles et scène de démonstration
│   ├── Scenes/
│   │   ├── Level_1_recup.unity     # Scène contenant les systèmes de jeu et l'interface repérés
│   │   ├── Level1.unity
│   │   └── Level2.unity
│   ├── Scripts/Gameplay/           # Logique du flipper
│   ├── Settings/                   # Ressources de configuration
│   ├── TextMesh Pro/               # Ressources de texte
│   ├── Texture/                    # Textures
│   ├── TutorialInfo/               # Contenu d'aide du modèle de projet
│   ├── _Recovery/                  # Scène de récupération
│   ├── computer_controls.inputactions
│   ├── Computer_System_input.inputactions
│   └── InputSystem_Actions.inputactions
├── Packages/                       # Manifeste et dépendances
├── ProjectSettings/                # Configuration Unity
└── README.md
```

`Library/`, `Temp/`, `Logs/` et les fichiers de solution/projet de l'IDE sont générés par Unity ou par les outils de développement.

Plusieurs scènes sont présentes, mais aucun système de progression ou de changement de niveau n'a été repéré dans les scripts de jeu. Le redémarrage et le retour au menu sont des opérations effectuées dans la scène actuelle.

## Ouvrir et lancer le projet

1. Installer Unity Hub et l'éditeur **6000.6.0f1**, avec le module de compilation de la plateforme souhaitée.
2. Dans Hub, ajouter le dossier racine du projet, qui contient `Assets/`, `Packages/` et `ProjectSettings/`.
3. Ouvrir le projet et attendre l'importation des ressources et la résolution des paquets.
4. Ouvrir [Assets/Scenes/Level_1_recup.unity](Assets/Scenes/Level_1_recup.unity), qui contient les gestionnaires, le HUD et les panneaux de jeu repérés dans cette analyse.
5. Consulter la Console et résoudre les références manquantes ou les erreurs avant de passer en mode Play. Vérifier notamment les problèmes liés au tag `Ball` et au contrôleur des palettes.
6. Passer en mode Play et utiliser le bouton de démarrage de l'interface.

Si les ressources essentielles de TextMeshPro sont demandées, les importer via `Window > TextMeshPro > Import TMP Essential Resources`.

## Commandes

Les touches ci-dessous sont définies dans les fichiers d'actions. Pour fonctionner, les actions doivent être affectées aux champs des composants dans l'Inspector.

### Jeu

Fichier : [computer_controls.inputactions](Assets/computer_controls.inputactions), carte `computer`.

| Action | Touches | Champ du composant |
| --- | --- | --- |
| `ball_launcher` | Flèche vers le haut ou Espace | `BallLauncher.actionLancer` |
| `flipper_left` | Flèche vers la gauche ou Q | `leftFlipAction` du contrôleur des palettes |
| `flipper_right` | Flèche vers la droite ou D | `rightFlipAction` du contrôleur des palettes |

Dans la scène `Level_1_recup`, ce fichier est référencé par le lanceur et par l'un des composants de contrôle des palettes.

### Menus

Fichier : [Computer_System_input.inputactions](Assets/Computer_System_input.inputactions).

| Carte | Action | Touche |
| --- | --- | --- |
| `game` | `MenuStart` | Entrée |
| `game` | `MenuResume` | Maj |
| `game` | `MenuRestart` | R |
| `game` | `MenuRestartPause` | P |
| `game` | `MenuMainMenu` | K |
| `game` | `enter` | Entrée |
| `naviagation` | `up`, `down`, `left`, `right` | Flèches directionnelles |

Le nom `naviagation` correspond à l'orthographe actuelle de la ressource. `MenuInputController` gère la navigation haut/bas, la confirmation et les raccourcis directs associés aux boutons. Aucune instance de ce composant n'a été repérée dans `Level_1_recup` ; la présence de ces actions dans le fichier ne signifie donc pas qu'elles sont actives.

Le `GameManager` permet d'activer ou de désactiver la pause via une action affectée à `pauseAction`. Ce champ est vide dans la scène analysée ; aucune touche de pause n'est donc liée par cette référence. L'interface dispose d'un bouton de pause.

`InputSystem_Actions.inputactions` contient les cartes génériques `Player` et `UI` ; il faut les distinguer des ressources dédiées au jeu et aux menus.

## Déroulement d'une partie

Le `GameManager` coordonne quatre états :

| État | Comportement |
| --- | --- |
| `Ready` | Affiche le menu initial, immobilise la bille et désactive les comportements de jeu enregistrés. |
| `Playing` | Libère la bille, active les commandes enregistrées et permet de comptabiliser les pertes de vie. |
| `Paused` | Affiche le panneau de pause, désactive ces commandes et définit `Time.timeScale = 0`. |
| `GameOver` | Immobilise la bille, désactive les commandes et affiche le score final. |

Déroulement habituel : `Ready → Playing → GameOver`. Pendant la partie, il est possible d'alterner entre `Playing` et `Paused`.

Lors de la préparation d'une nouvelle partie, le gestionnaire remet le score à zéro, restaure les vies et repositionne la bille. Un redémarrage passe directement à `Playing` ; un retour au menu passe à `Ready`.

### Bille et collisions

- Le lanceur applique une impulsion sur l'axe Z local (`forward`) du point de lancement lorsque l'action est déclenchée et que la bille se trouve dans le rayon autorisé. La force n'augmente pas progressivement.
- Les palettes utilisent des pivots créés à l'exécution, avec un `Rigidbody` cinématique et une rotation dans `FixedUpdate`. L'axe de rotation est `table.up`, ou l'axe Y global si `table` n'est pas affecté.
- Les bumpers appliquent une impulsion qui éloigne la bille. Le `BumperController` actuel n'ajoute pas de points.
- Les cibles munies de `TargetController` appliquent une impulsion, ajoutent des points et tentent d'activer le déclencheur d'animation `Impact`.

### Vies et score

Par défaut, le `LifeManager` commence avec **3 vies**. Lorsque la bille entre dans une `DeathZone` pendant l'état `Playing`, une vie est perdue et la bille est immobilisée. S'il reste des vies, elle réapparaît après **1 seconde** par défaut. Une protection empêche de comptabiliser plusieurs pertes pendant le traitement d'une même chute.

Le point de réapparition est enregistré dans `Awake` : le gestionnaire utilise `ballSpawnPoint` s'il est affecté, sinon la position et la rotation initiales de la bille. La réapparition annule les vitesses linéaire et angulaire. Le délai utilise `WaitForSeconds` et est donc également suspendu pendant la pause.

Lorsqu'il ne reste plus de vies, `OnOutOfLives` avertit le `GameManager`, qui passe à l'état `GameOver` et demande au `GameOverManager` d'afficher le score final.

Le `ScoreManager` conserve le score en mémoire via `ScoreManager.Instance`. `ScoreUI` suit l'événement `onScoreChanged`. Aucune persistance des records ou des parties n'a été repérée.

## Scripts et responsabilités

Les scripts d'exécution se trouvent dans [Assets/Scripts/Gameplay/](Assets/Scripts/Gameplay/).

| Fichier | Responsabilité et API principale |
| --- | --- |
| `BallLauncher.cs` | Références à la bille et au point de lancement ; lit `actionLancer` et applique l'impulsion. |
| `FlipperController.cs` | Contrôle les deux palettes et crée les pivots physiques. La classe actuellement déclarée s'appelle `NewMonoBehaviourScript`. |
| `BumperController.cs` | Filtre les collisions par tag et applique `bounceForce`. |
| `TargetController.cs` | Renvoie la bille, attribue `scoreValue` et déclenche l'animation dans `OnTargetHit()`. |
| `ScoreManager.cs` | Singleton de scène ; `CurrentScore`, `AddScore(int)`, `ResetScore()`, `onScoreChanged` et `onScoreAdded`. |
| `ScoreUI.cs` | Met à jour un `TMP_Text` avec le score et un préfixe configurable. |
| `LifeManager.cs` | Vies, réapparition et icônes ; `ResetLives()`, `ResetBall()`, `SetTracking(bool)`, `SetBallFrozen(bool)` et événements de vies. |
| `DeathZone.cs` | Transmet `OnTriggerEnter` à `LifeManager.HandleBallLost()`. |
| `GameManager.cs` | États ; `StartGame()`, `PauseGame()`, `ResumeGame()`, `RestartGame()`, `ReturnToMainMenu()` et `OnStateChanged`. |
| `GameOverManager.cs` | Affiche/masque le panneau final avec `Show(int)` et `Hide()` ; émet `RestartRequested` et `MainMenuRequested`. |
| `MenuInputController.cs` | Relie les actions aux boutons visibles/interactifs et gère leur sélection au clavier. |

### Valeurs de référence

Les valeurs définies dans l'Inspector peuvent remplacer les valeurs par défaut des scripts.

| Paramètre | Valeur par défaut dans le code | Valeur repérée dans `Level_1_recup` |
| --- | --- | --- |
| Impulsion du lancement | `12` | `20` |
| Rayon de lancement | `0.5` | `5` |
| Vitesse d'activation des palettes | `500` degrés/s | Vérifier le composant concerné dans l'Inspector |
| Vitesse de retour des palettes | `250` degrés/s | Vérifier dans l'Inspector |
| Angle actif des palettes | `45` degrés | Vérifier dans l'Inspector |
| Force des bumpers/cibles | `10` | `10` sur les composants repérés |
| Points par cible | `100` | `100` sur les cibles repérées |
| Vies initiales | `3` | `3` |
| Délai de réapparition | `1` seconde | `1` seconde |

## Configuration de la scène

### Bille et table

1. Créer le tag `Ball` dans `Project Settings > Tags and Layers` et l'affecter à l'objet qui participe aux collisions de la bille.
2. Vérifier que la bille possède un `Rigidbody` et un collider 3D adapté.
3. Configurer les colliders de la table, des palettes, des bumpers et des cibles.
4. Dans le lanceur, affecter `balle`, `pointDeLancement` et `actionLancer`. Orienter l'axe Z du point de lancement dans la direction souhaitée.
5. Dans le contrôleur des palettes, affecter `leftFlipper`, `rightFlipper`, les deux actions et, si nécessaire, `table`. Chaque palette doit avoir un `BoxCollider`, sur elle-même ou sur ses enfants.

### Gestionnaires et interface

1. Garder `GameManager`, `LifeManager`, `GameOverManager` et `ScoreManager` sur des objets actifs, en dehors des panneaux susceptibles d'être désactivés.
2. Dans le `GameManager`, affecter les trois gestionnaires, les panneaux et les boutons. Ajouter le lanceur et les contrôleurs des palettes à `gameplayBehaviours`.
3. Dans le `LifeManager`, affecter la bille, le texte des vies et, si nécessaire, le point de réapparition et le modèle d'icône.
4. Créer sous la sortie de la table une zone munie d'un collider 3D en mode **Is Trigger**, du composant `DeathZone` et d'une référence au `LifeManager`.
5. Ajouter `ScoreUI` au texte TextMeshPro du score et ne conserver qu'un seul `ScoreManager` dans la scène.
6. Dans le `GameOverManager`, affecter le panneau, les textes et les boutons de redémarrage/menu.
7. Vérifier la présence d'un `EventSystem` avec `InputSystemUIInputModule` pour les boutons de l'interface.
8. Pour les raccourcis du menu, ajouter et configurer `MenuInputController`, notamment la liste ordonnée des boutons et les actions. La référence facultative au `GameManager` désactive la navigation pendant `Playing` ; les raccourcis directs restent soumis à la visibilité et à l'interactivité des boutons.

Pour les cibles animées, configurer un `Animator` sur l'objet, un parent ou un enfant, avec un déclencheur dont le nom correspond à `impactTrigger`.

## Outil de création de l'interface

[Assets/Editor/CreatePinballUI.cs](Assets/Editor/CreatePinballUI.cs) ajoute la commande **Tools > Créer l’interface Pinball** à Unity.

L'outil crée un `PinballCanvas` en Screen Space Overlay, avec une échelle basée sur une résolution de référence de `1920 × 1080`, un HUD pour le score et les vies, ainsi que les panneaux initial, de pause et de fin de partie. Il crée ou réutilise les gestionnaires, prépare l'`EventSystem`, cherche une bille portant le tag `Ball` et relie les références de l'Inspector. Il cherche également une `DeathZone` ou en crée une.

Après l'exécution de la commande :

1. Confirmer la bille trouvée et les références des gestionnaires.
2. Positionner et dimensionner la `DeathZone` sous la sortie de la table.
3. Confirmer les comportements enregistrés dans `gameplayBehaviours`.
4. Affecter l'action de pause si un raccourci clavier est souhaité.
5. Configurer séparément `MenuInputController` si des raccourcis de menu sont souhaités.
6. Enregistrer la scène ; l'outil la marque comme modifiée, mais ne l'enregistre pas automatiquement.

La commande refuse de créer une autre interface si un objet nommé `PinballCanvas` existe déjà. La scène `Level_1_recup` contient déjà cet objet. L'opération prend en charge Undo ; il n'est pas nécessaire de l'exécuter pour utiliser une interface existante.

## Générer une build

**Point à corriger :** `ProjectSettings/EditorBuildSettings.asset` référence `Assets/Scenes/SampleScene.unity`, mais cette scène ne figure pas parmi les fichiers analysés.

1. Ouvrir `File > Build Profiles` dans Unity.
2. Choisir la plateforme et vérifier que le module correspondant est installé.
3. Corriger la liste des scènes : retirer la référence manquante et ajouter la scène de jeu souhaitée, par exemple `Assets/Scenes/Level_1_recup.unity`, en première scène active.
4. Résoudre les erreurs de compilation et les références de scène.
5. Générer la build dans un dossier tel que `Builds/`.
6. Exécuter le résultat et vérifier le lancement, les palettes, l'interface, la pause, les pertes de vies et le redémarrage.

Aucun paramètre d'intégration continue ni script de build automatisée n'a été repéré.

## Vérification manuelle

Aucun test C# propre au projet n'a été trouvé dans `Assets/`, même si Unity Test Framework est installé. La liste suivante sert de procédure de validation ; elle ne correspond pas à des tests déjà exécutés.

- À l'ouverture de la scène, le menu initial s'affiche et la bille reste immobile.
- Au démarrage, les palettes et le lanceur deviennent disponibles.
- La bille ne reçoit l'impulsion du lanceur que dans le rayon configuré.
- Les deux palettes s'activent et reviennent à leur position initiale sans traverser indûment la bille.
- Les bumpers renvoient la bille ; les cibles attribuent des points et déclenchent l'animation configurée.
- Le score affiché suit le total du `ScoreManager`.
- Une chute ne retire qu'une vie et la bille réapparaît après le délai configuré.
- La pause interrompt la simulation et permet de reprendre la partie.
- La dernière vie ouvre le panneau de fin avec le bon score.
- Le redémarrage réinitialise la bille, les vies et le score ; le retour au menu affiche le panneau initial.
- Les commandes et l'interface fonctionnent également dans l'exécutable généré.

## Problèmes connus et dépannage

| Situation | Indice/configuration à vérifier | Action suggérée |
| --- | --- | --- |
| Référence de build invalide | La liste de build contient `SampleScene.unity`, qui est absente. | Mettre à jour la liste des scènes avant de générer l'exécutable. |
| Tag de la bille manquant | `TagManager.asset` contient `tags: []`, alors que les scripts utilisent `Ball`. | Créer et affecter le tag ; `CompareTag` nécessite un tag valide. |
| Nom incohérent du contrôleur des palettes | `FlipperController.cs` déclare `NewMonoBehaviourScript`. | Aligner le nom du fichier et celui de la classe dans une modification de code dédiée, puis vérifier les composants et références des scènes dans Unity. |
| La touche de pause ne fonctionne pas | `GameManager.pauseAction` est vide dans la scène analysée. | Affecter une action de pause ; ne pas supposer que P ou Échap sont déjà configurées. |
| Les raccourcis du menu ne fonctionnent pas | Les actions existent, mais aucune instance de `MenuInputController` n'a été repérée dans la scène analysée. | Ajouter le composant et relier les actions et les boutons. |
| Une palette ne répond pas | La scène contient plusieurs composants identifiés comme `NewMonoBehaviourScript` ; l'un d'eux n'a pas d'actions affectées. | Vérifier lesquels sont actifs, les palettes affectées et les entrées de `gameplayBehaviours`. |
| La bille ne se lance pas | Références manquantes, action incorrecte ou bille hors du rayon. | Vérifier `balle`, `pointDeLancement`, `actionLancer`, la distance et l'orientation. |
| Une chute ne retire pas de vie | Le jeu doit être dans l'état `Playing` ; le déclencheur et la référence au gestionnaire doivent être configurés. | Vérifier `Is Trigger`, les collisions entre layers, l'identification de la bille et les journaux de `DeathZone`/`LifeManager`. |
| Plusieurs billes/zones dans la scène | Des objets `Ball` et `Ball (1)` ont été repérés, ainsi que plusieurs composants `DeathZone`. | Confirmer lesquels appartiennent à la table souhaitée et quel `Rigidbody` est utilisé par les gestionnaires et le lanceur. |
| Une cible ne s'anime pas | `Animator` ou déclencheur manquant. | Configurer l'`Animator` et le déclencheur `Impact`, ou ajuster `impactTrigger`. |
| Mélange de langues dans l'interface | L'outil crée des libellés français et les scripts mettent à jour plusieurs textes en portugais. | Uniformiser les libellés lorsque la langue finale de l'interface sera définie. |

Ces observations proviennent de la lecture des fichiers enregistrés et ne remplacent pas une vérification dans l'éditeur.

## Gestion des versions

Versionner `Assets/` avec les fichiers `.meta` associés, ainsi que `Packages/` et `ProjectSettings/`. Les `.meta` préservent les GUID utilisés dans les références entre ressources et scènes.

Le `.gitignore` existant exclut les dossiers générés tels que `Library/`, `Temp/`, `Logs/`, `UserSettings/` et `Builds/`, ainsi que les fichiers d'IDE. Lors de l'ajout ou du déplacement de ressources, préserver les fichiers `.meta` correspondants ; il est préférable d'effectuer ces opérations dans Unity.

Le projet contient des ressources tierces, notamment dans `Assets/PolyRonin/` et `Assets/TextMesh Pro/`. Vérifier les licences applicables avant de redistribuer ces ressources ; leur présence dans le projet ne définit pas une licence globale pour l'ensemble du contenu.
