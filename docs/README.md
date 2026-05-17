# Gravity Labyrinthe — Scripts Documentation

**Project:** Unity Mini-Game | ESCEN Tech – 3rd Year  
**Context:** Learning Expedition – Montreal  
**Author:** Youness Fatine

---

## Overview

*Gravity Labyrinthe* is a 3D first-person maze game where the player can shift the direction of gravity to navigate a procedurally generated labyrinth. The goal is to reach the exit zone before the 3-minute timer runs out.

---

## Script Index

| Script | Role |
|---|---|
| [PlayerController.md](PlayerController.md) | Handles player movement, camera, and jump |
| [GravityController.md](GravityController.md) | Manages real-time gravity direction shifts |
| [GravityStatusUI.md](GravityStatusUI.md) | HUD panel showing gravity state and cooldown |
| [GoalZone.md](GoalZone.md) | Detects when the player reaches the exit |
| [MazeGenerator.md](MazeGenerator.md) | Generates a 3D maze using DFS and primitive cubes |
| [SciFiMazeGenerator.md](SciFiMazeGenerator.md) | Generates a 2D maze using Sci-Fi prefabs |
| [StartAndSensitivityUI.md](StartAndSensitivityUI.md) | Start screen, timer, sensitivity panel, victory/defeat |

---

## Architecture Diagram

```
StartAndSensitivityUI  ──── controls ────▶  PlayerController
        │                                         │
        │ ShowVictory()                    reads/writes
        │                                         │
      GoalZone  ◀── trigger ───────────────  (Collider)
                                                  
GravityController  ──── updates ────▶  GravityStatusUI
        │
   modifies Physics.gravity
        │
   affects ──▶  PlayerController (FixedUpdate gravity projection)

MazeGenerator / SciFiMazeGenerator
   └─ spawns GoalZone + places PlayerController at start
```
