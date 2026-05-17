# Gravity Labyrinthe

**Institution:** ESCEN Tech — 3rd Year, Tech Major  
**Context:** Learning Expedition – Montreal  
**Author:** Youness Fatine  
**Engine:** Unity (C#)

---

## Story

> *Near-future Tokyo, 2090.*

Every 5,000 years, the cosmic entity **Amatsu-Mikaboshi** reincarnates within the human world — a being whose very existence threatens the structural integrity of reality itself.

A seemingly ordinary young man named **Ashiro Nagamoro** is born carrying that burden: an anomalous, physically impossible ability to directly manipulate gravitational forces.

His emerging powers rapidly destabilize Japanese government infrastructure. Fearing systemic collapse, the state captures Ashiro and confines him inside a **classified, maximum-security experimental labyrinth** — an architectural prison engineered to monitor, test, and ultimately weaponize his gravitational control.

Ashiro has one objective: **master his inherited power**, solve the labyrinth's ever-shifting architectural puzzles, and **escape before the countdown reaches zero** — or his family outside the facility will face the consequences.

---

## Gameplay

| Element | Description |
|---|---|
| Genre | First-person 3D maze |
| Core mechanic | Real-time gravity direction shifting |
| Win condition | Reach the exit (Goal Zone) before time runs out |
| Lose condition | Timer reaches 00:00 |
| Time limit | 3 minutes |

### Controls

| Key | Action |
|---|---|
| `W` `A` `S` `D` | Move |
| `Mouse` | Look around |
| `Space` | Jump |
| `Z` | Shift gravity — Left |
| `X` | Shift gravity — Forward |
| `C` | Shift gravity — Right |
| `Ctrl` | Toggle mouse sensitivity panel |
| `Enter` / `Space` | Start game (on start screen) |

---

## Project Structure

```
Gravity-Labyrinthe/
├── Assets/
│   └── Scripts/          ← All C# game scripts
│       ├── PlayerController.cs
│       ├── GravityController.cs
│       ├── GravityStatusUI.cs
│       ├── GoalZone.cs
│       ├── MazeGenerator.cs
│       └── StartAndSensitivityUI.cs
│
├── docs/                 ← Script documentation (English)
│   ├── README.md
│   ├── PlayerController.md
│   ├── GravityController.md
│   ├── GravityStatusUI.md
│   ├── GoalZone.md
│   ├── MazeGenerator.md
│   └── StartAndSensitivityUI.md
│
└── docsFR/               ← Documentation des scripts (Français)
    ├── README.md
    ├── PlayerController.md
    ├── GravityController.md
    ├── GravityStatusUI.md
    ├── GoalZone.md
    ├── MazeGenerator.md
    └── StartAndSensitivityUI.md
```

---

## Script Documentation

Each script has its own dedicated documentation file. Choose your language:

### English — [docs/](docs/README.md)

| Script | Role | Doc |
|---|---|---|
| `PlayerController.cs` | Player movement, camera, jump | [docs/PlayerController.md](docs/PlayerController.md) |
| `GravityController.cs` | Gravity direction shifting + cooldown | [docs/GravityController.md](docs/GravityController.md) |
| `GravityStatusUI.cs` | HUD panel — gravity state & cooldown bar | [docs/GravityStatusUI.md](docs/GravityStatusUI.md) |
| `GoalZone.cs` | Exit trigger — detects player, fires victory | [docs/GoalZone.md](docs/GoalZone.md) |
| `MazeGenerator.cs` | Procedural 3D maze (DFS + primitive cubes) | [docs/MazeGenerator.md](docs/MazeGenerator.md) |
| `StartAndSensitivityUI.cs` | Start screen, timer, victory/defeat, sensitivity | [docs/StartAndSensitivityUI.md](docs/StartAndSensitivityUI.md) |

### Français — [docsFR/](docsFR/README.md)

| Script | Rôle | Doc |
|---|---|---|
| `PlayerController.cs` | Déplacement, caméra, saut | [docsFR/PlayerController.md](docsFR/PlayerController.md) |
| `GravityController.cs` | Changement de gravité + cooldown | [docsFR/GravityController.md](docsFR/GravityController.md) |
| `GravityStatusUI.cs` | HUD — état gravité et barre de cooldown | [docsFR/GravityStatusUI.md](docsFR/GravityStatusUI.md) |
| `GoalZone.cs` | Trigger de sortie — détecte le joueur, victoire | [docsFR/GoalZone.md](docsFR/GoalZone.md) |
| `MazeGenerator.cs` | Labyrinthe 3D procédural (DFS + cubes) | [docsFR/MazeGenerator.md](docsFR/MazeGenerator.md) |
| `StartAndSensitivityUI.cs` | Écran démarrage, chrono, victoire/défaite, sensibilité | [docsFR/StartAndSensitivityUI.md](docsFR/StartAndSensitivityUI.md) |

---

## Architecture Overview

```
StartAndSensitivityUI  ──── enables/disables ────▶  PlayerController
        │                                                  │
        │  ShowVictory()                         gravity projection
        │                                                  │
      GoalZone  ◀──────────── trigger ────────────  (Collider)

GravityController  ─── updates each frame ───▶  GravityStatusUI
        │
   writes Physics.gravity  (global — affects all Rigidbodies)

MazeGenerator
   └─ generates the level, spawns GoalZone, places player at start
```

---

## Academic Use of AI

This project was developed with AI assistance (Claude). As required by the academic integrity policy, all AI usage is documented: AI was used for scripting support, code structure, and documentation generation. All technical choices, game concept, and design decisions reflect the author's own understanding and direction.
