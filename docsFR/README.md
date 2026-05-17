# Gravity Labyrinthe — Documentation des Scripts

**Projet :** Unity Mini-Game | ESCEN Tech – 3ème année  
**Contexte :** Learning Expedition – Montréal  
**Auteur :** Youness Fatine

---

## Présentation

*Gravity Labyrinthe* est un jeu de labyrinthe 3D en vue première personne où le joueur peut modifier la direction de la gravité pour naviguer dans un labyrinthe généré de façon procédurale. L'objectif est d'atteindre la zone de sortie avant la fin du chronomètre de 3 minutes.

---

## Index des scripts

| Script | Rôle |
|---|---|
| [PlayerController.md](PlayerController.md) | Gère le déplacement, la caméra et le saut du joueur |
| [GravityController.md](GravityController.md) | Gère les changements de direction de la gravité |
| [GravityStatusUI.md](GravityStatusUI.md) | Panneau HUD affichant l'état de la gravité et le cooldown |
| [GoalZone.md](GoalZone.md) | Détecte quand le joueur atteint la sortie |
| [MazeGenerator.md](MazeGenerator.md) | Génère un labyrinthe 3D par DFS avec des cubes primitifs |
| [SciFiMazeGenerator.md](SciFiMazeGenerator.md) | Génère un labyrinthe 2D avec des prefabs Sci-Fi |
| [StartAndSensitivityUI.md](StartAndSensitivityUI.md) | Écran de départ, chronomètre, sensibilité, victoire/défaite |

---

## Diagramme d'architecture

```
StartAndSensitivityUI  ──── contrôle ────▶  PlayerController
        │                                         │
        │ ShowVictory()                    lit/modifie
        │                                         │
      GoalZone  ◀── trigger ───────────────  (Collider)
                                                  
GravityController  ──── met à jour ────▶  GravityStatusUI
        │
   modifie Physics.gravity
        │
   affecte ──▶  PlayerController (projection gravité dans FixedUpdate)

MazeGenerator / SciFiMazeGenerator
   └─ crée GoalZone + place PlayerController au départ
```
