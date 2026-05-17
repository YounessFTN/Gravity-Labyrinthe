# SciFiMazeGenerator.cs

**Emplacement :** `Assets/Scripts/SciFiMazeGenerator.cs`  
**Attaché à :** Un GameObject vide (alternative à `MazeGenerator`)

---

## Rôle

Un **générateur de labyrinthe alternatif** qui utilise des prefabs modulaires Sci-Fi du pack *Sci-Fi Styled Modular Pack* à la place de cubes primitifs. Il génère un labyrinthe en **grille 2D** (colonnes × rangées) par DFS, puis construit chaque cellule en plaçant des dalles de sol, des murs et optionnellement des prefabs de couloirs.

---

## Différence avec MazeGenerator

| Fonctionnalité | MazeGenerator | SciFiMazeGenerator |
|---|---|---|
| Dimensions du labyrinthe | 3D (largeur × hauteur × profondeur) | 2D (colonnes × rangées) |
| Géométrie | Cubes primitifs | Prefabs Sci-Fi |
| Compatible gravité | Oui (couloirs 3D) | Partiel |
| Auto-assigne les prefabs | Via `SurfaceStyle` | Via chemins `AssetDatabase` |

---

## Champs publics (Inspecteur)

### Prefabs de couloirs (auto-assignés)
| Champ | Asset | Description |
|---|---|---|
| `prefabDeadEnd` | `Corridor_1.prefab` | Cellule en cul-de-sac (1 ouverture) |
| `prefabStraight` | `Corridor_I.prefab` | Couloir droit (2 ouvertures opposées) |
| `prefabCorner` | `Corridor_L.prefab` | Virage (2 ouvertures adjacentes) |
| `prefabT` | `Corridor_T.prefab` | Jonction en T (3 ouvertures) |
| `prefabCross` | `Corridor_X.prefab` | Carrefour (4 ouvertures) |

### Décor
| Champ | Asset |
|---|---|
| `prefabFloor` | `floor_1.prefab` |
| `prefabWall` | `blank_wall_A.prefab` |
| `prefabLight` | `light_celing_1.prefab` |

### Style des murs
| Champ | Description |
|---|---|
| `wallTexture` | Texture appliquée aux murs générés |
| `wallMaterial` | Matériau de base (optionnel) |
| `wallTint` | Teinte de couleur si aucune texture |

### Paramètres
| Champ | Défaut | Description |
|---|---|---|
| `cols` | 8 | Nombre de colonnes |
| `rows` | 8 | Nombre de rangées |
| `cellSize` | 4 | Taille monde de chaque cellule |
| `lightDensity` | 0.3 | Probabilité (0–1) de placer une lumière au plafond par cellule |
| `seed` | 0 | Graine aléatoire (0 = aléatoire) |
| `useCorridorPrefabs` | false | Si true, utilise les prefabs Corridor_* ; si false, génère des murs individuels |

---

## Algorithme : Parcours en Profondeur (DFS) 2D

Le DFS utilise une grille `bool[cols, rows]` pour les cellules visitées et un tableau `open[cols, rows, 4]` où la dernière dimension indique si chacun des 4 murs (N/E/S/O) est ouvert (passage) ou fermé (mur).

```
CarveDFS(0, 0)
  → marquer comme visité
  → mélanger les directions [N, E, S, O]
  → pour chaque direction :
      si le voisin est dans les limites et non visité :
        ouvrir le mur entre la cellule courante et le voisin
        récursion dans le voisin
```

---

## Construction des cellules (`BuildCells`)

Pour chaque cellule `(x, z)` :
1. Placer le prefab de sol.
2. Optionnellement placer une lumière au plafond (selon `lightDensity`).
3. Soit :
   - **Mode couloir** (`useCorridorPrefabs = true`) : choisir le bon prefab Corridor_* selon les murs ouverts et le faire pivoter correctement.
   - **Mode murs** (par défaut) : générer des murs individuels (ou cubes de secours) sur les côtés fermés uniquement.

### Sélection du prefab de couloir

| Nombre de murs ouverts | Prefab choisi |
|---|---|
| 1 | Cul-de-sac (`Corridor_1`) |
| 2 (opposés) | Droit (`Corridor_I`) |
| 2 (adjacents) | Virage (`Corridor_L`) |
| 3 | Jonction T (`Corridor_T`) |
| 4 | Carrefour (`Corridor_X`) |

La rotation est calculée selon quels murs sont ouverts grâce aux méthodes `DeadEndRot`, `CornerRot` et `TRot`.

---

## Application du style des murs

Les murs générés reçoivent un matériau partagé construit en temps réel à partir de `wallTexture` et `wallTint`. La même instance de matériau est réutilisée sur tous les murs pour minimiser les draw calls.

---

## Auto-assignation (éditeur uniquement)

Quand le composant est ajouté dans l'éditeur (`Reset`) ou validé (`OnValidate`), tous les champs de prefabs sont automatiquement renseignés depuis le dossier `Assets/Sci-Fi Styled Modular Pack/Prefabs/`. Aucun glisser-déposer nécessaire.

---

## Commandes du menu contextuel

- **Generate Maze** — construire le labyrinthe.
- **Clear Maze** — détruire tous les objets enfants `Cell_*`.
- **Apply Wall Style To Existing Walls** — réappliquer le matériau de mur actuel sans régénérer.

---

## Placement des acteurs

| Acteur | Position |
|---|---|
| `player` | `(0, 1, 0)` — première cellule, légèrement au-dessus du sol |
| `goal` | `((cols-1)·cellSize, 1, (rows-1)·cellSize)` — dernière cellule |

---

## Dépendances

- **Sci-Fi Styled Modular Pack** — requis pour les prefabs de couloirs, murs et sols.
- **PlayerController** — trouvé automatiquement dans la scène pour la référence `player`.
- **GoalZone** — trouvé automatiquement dans la scène pour la référence `goal`.
