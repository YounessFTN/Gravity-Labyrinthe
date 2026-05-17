# GravityStatusUI.cs

**Emplacement :** `Assets/Scripts/GravityStatusUI.cs`  
**Attaché à :** Un GameObject créé en temps réel (`"Gravity Status UI"`)

---

## Rôle

Un panneau HUD sci-fi affiché dans le **coin inférieur gauche** de l'écran. Il montre :
- L'état actuel de la gravité (`READY`, `GRAVITY SHIFT`, `RECHARGE X.Xs`)
- Une barre de progression du cooldown
- Les touches de contrôle (Z / C / X)
- Un mini personnage qui tourne pour représenter la direction de la gravité courante

L'intégralité de l'UI est **construite en code** — aucun prefab ni référence de scène n'est nécessaire.

---

## Patron Singleton

```csharp
GravityStatusUI.EnsureInstance()
```

Appelé par `GravityController.Start()`. Si un `GravityStatusUI` existe déjà dans la scène, il le retourne ; sinon il crée un nouveau GameObject avec `DontDestroyOnLoad`.

---

## Méthodes d'état (appelées par GravityController)

| Méthode | Quand appelée | Ce qu'elle fait |
|---|---|---|
| `SetGravity(direction, strength)` | Après un changement de gravité | Fait tourner le mini personnage, remet la barre à fond |
| `SetTransitionProgress(t)` | Chaque frame pendant la rotation | Affiche "GRAVITY SHIFT" en rose, la barre montre la progression |
| `SetCooldown(remaining, duration)` | Chaque frame pendant le cooldown | Affiche "RECHARGE X.Xs", la barre se vide de gauche à droite |
| `SetReady()` | Quand le cooldown se termine | Affiche "READY" en cyan, barre pleine |
| `Bind(controller)` | À l'initialisation | Stocke la référence vers `GravityController` |

---

## Disposition visuelle

```
┌──────────────────────────────────────────┐  ← contour accent
│ ▌  GRAVITY CORE              READY       │
│ ▌  [Z]  GRAVITE GAUCHE                   │
│ ▌  [C]  GRAVITE DROITE      [mini perso] │
│ ▌  [X]  GRAVITE AVANT                    │
│ ▌                                        │
│    ══════════════════════════════        │  ← barre de cooldown
└──────────────────────────────────────────┘
```

### Palette de couleurs

| Nom | Couleur | Utilisée pour |
|---|---|---|
| `AccentColor` | Cyan `#00D9FF` | Bordures, état READY, barre de cooldown |
| `HotColor` | Rose `#FF4390` | États GRAVITY SHIFT / RECHARGE |
| `TextColor` | Blanc cassé | Labels des touches |
| `MutedTextColor` | Vert-bleu doux | Descriptions des actions, titre |
| `PanelColor` | Bleu très sombre (82% opacité) | Fond du panneau |

---

## Mini personnage

Un petit personnage en stick made de rectangles `Image` colorés (tête, corps, bras, jambes). Il tourne en douceur via `Quaternion.Lerp` dans `Update` pour refléter la direction de la gravité courante :

| Axe de gravité | Rotation du personnage |
|---|---|
| Bas (défaut) | 0° |
| Droite | 90° |
| Gauche | -90° |
| Haut (inversé) | 180° |
| Avant | 45° |
| Arrière | -45° |

---

## Dépendances

- `UnityEngine.UI` — `Image`, `Text`, `Canvas`, `CanvasScaler`, `Outline`
- `GravityController` — fournit les mises à jour d'état
- Aucun asset de scène requis ; la police utilise le `LegacyRuntime.ttf` intégré d'Unity
