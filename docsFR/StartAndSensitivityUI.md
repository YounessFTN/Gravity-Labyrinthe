# StartAndSensitivityUI.cs

**Emplacement :** `Assets/Scripts/StartAndSensitivityUI.cs`  
**Attaché à :** Un GameObject créé en temps réel (`"Start And Sensitivity UI"`)

---

## Rôle

Le gestionnaire central de l'UI du jeu. Il gère **cinq écrans distincts** construits entièrement en code :

1. **Écran de démarrage** — affiché avant le début du jeu (avec les règles, l'image de fond et un bouton START)
2. **HUD chronomètre** — affiché pendant le jeu (compte à rebours + temps écoulé)
3. **Panneau de sensibilité** — overlay pour ajuster la sensibilité de la souris en cours de partie
4. **Écran de victoire** — affiché quand le joueur atteint la GoalZone
5. **Écran de défaite** — affiché quand le chronomètre de 3 minutes arrive à zéro

---

## Patron Singleton

```csharp
StartAndSensitivityUI.EnsureInstance(PlayerController p)
```

Appelé par `PlayerController.Start()` et `GoalZone.OnTriggerEnter()`. Retourne l'instance existante ou en crée une nouvelle. `DontDestroyOnLoad` la conserve entre les rechargements de scène.

La propriété statique `Instance` donne à `GoalZone` un accès direct pour appeler `ShowVictory()`.

---

## États du jeu

```
[Écran démarrage]  ──── ENTRÉE/ESPACE/clic ────▶  [En jeu]
                                                       │
                                            chrono = 0
                                                       │
                                                       ▼
[Écran victoire] ◀── GoalZone atteinte ──  [En jeu]  ──── ▶  [Écran défaite]
       │                                                              │
       └─────────── REJOUER (RestartGame) ─────────────────────────────┘
```

---

## Chronomètre

- Durée : **180 secondes** (3 minutes), définie par la constante `GameDuration`.
- Décompte dans `Update` via `Time.deltaTime`.
- Format d'affichage : `MM:SS`
- Quand il reste ≤ 30 secondes, la couleur du chrono clignote en rouge via `Mathf.Sin`.
- Le temps écoulé est également affiché (`ECOULE MM:SS`).

---

## Panneau de sensibilité souris

- Basculé en appuyant sur **Ctrl gauche** ou **Ctrl droit** pendant le jeu.
- Quand ouvert, le déplacement du joueur est **désactivé** et le curseur est **déverrouillé**.
- La sensibilité s'ajuste via :
  - La molette de la souris
  - Touche `+` / `=` (augmenter de 0.1)
  - Touche `-` (diminuer de 0.1)
- Une barre de remplissage montre la sensibilité normalisée entre `minMouseSensitivity` et `maxMouseSensitivity`.
- Label d'état : `LOW` (< 35%), `BALANCED` (35–72%), `HIGH` (> 72%).

---

## Détails de l'écran de démarrage

### Image de fond

Le script cherche une image de fond dans cet ordre :
1. `Resources/StartBackground` (fonctionne dans les builds)
2. `Assets/bg-start.png` (chemin éditeur)
3. Toute texture nommée `StartBackground` dans `Assets/`
4. Toute image 16:9 (ratio 1.65–1.90) trouvée n'importe où dans `Assets/`

L'image est affichée en plein écran avec `AspectRatioFitter` en mode `EnvelopeParent`.

### Contenu
- Barre HUD supérieure : nom de l'école, auteur
- Panneau de règles à gauche : contrôles et objectif
- Panneau central : titre, bouton START, indication clavier

---

## Récapitulatif des écrans UI

| Écran | Déclencheur | Contrôles bloqués ? | Curseur |
|---|---|---|---|
| Démarrage | Au `Awake` | Oui (`Time.timeScale = 0`) | Visible |
| En jeu | ENTRÉE/ESPACE | Non | Verrouillé |
| Panneau sensibilité | Ctrl gauche/droit | Oui (déplacement uniquement) | Visible |
| Victoire | `ShowVictory()` | Oui | Visible |
| Défaite | Chrono = 0 | Oui | Visible |

---

## Méthodes principales

| Méthode | Description |
|---|---|
| `EnsureInstance(PlayerController)` | Fabrique statique — récupère ou crée l'UI |
| `Bind(PlayerController)` | Lie l'UI à une instance joueur (appelé après rechargement de scène) |
| `ShowVictory()` | Affiche l'écran de victoire, arrête le chrono, désactive les contrôles |
| `ShowStart()` | Réinitialise à l'état de l'écran de démarrage |
| `StartGame()` | Transition vers le jeu |

---

## Rechargement de scène (`RestartGame`)

```csharp
Time.timeScale = 1f;
SceneManager.LoadScene(SceneManager.GetActiveScene().name);
```

Recharge la scène courante. Comme ce GameObject a `DontDestroyOnLoad`, `Bind()` est rappelé par la nouvelle instance de `PlayerController` pour tout reconnecter.

---

## Dépendances

- **PlayerController** — les contrôles sont activés/désactivés et l'état du curseur est géré via lui.
- **UnityEngine.UI** — `Image`, `Text`, `Button`, `Canvas`, `CanvasScaler`, `Outline`
- **UnityEngine.InputSystem** — lit directement le clavier et la souris pour la navigation UI
- **UnityEngine.SceneManagement** — pour le rechargement de scène au redémarrage
- Aucun prefab ni asset de scène requis (sauf l'image de fond optionnelle)
