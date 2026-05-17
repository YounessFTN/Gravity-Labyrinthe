# MazeGenerator.cs

**Emplacement :** `Assets/Scripts/MazeGenerator.cs`  
**Attaché à :** Un GameObject vide (ex. `"Maze"`)

---

## Rôle

Génère un **labyrinthe procédural 3D** à base de cubes primitifs. Il utilise un algorithme de Parcours en Profondeur (DFS) pour creuser des chemins dans une grille 3D, puis construit la géométrie, place une salle de sortie, disperse des objets décoratifs et positionne le joueur au départ.

---

## Champs publics (Inspecteur)

### Dimensions
| Champ | Plage | Description |
|---|---|---|
| `width` | 2–7 | Nombre de cellules sur l'axe X |
| `height` | 2–7 | Nombre de cellules sur l'axe Y |
| `depth` | 2–7 | Nombre de cellules sur l'axe Z |

### Géométrie
| Champ | Défaut | Description |
|---|---|---|
| `cellSize` | 4 | Taille de chaque couloir en unités |
| `wallThickness` | 0.5 | Épaisseur de chaque dalle de mur |

### Style des surfaces (`SurfaceStyle`)
Chaque type de surface (murs, sol, plafond) possède un `SurfaceStyle` avec :
- `materiaux` — tableau de matériaux (un choisi aléatoirement par cube)
- `texture` — une `Texture2D` appliquée si aucun matériau n'est assigné
- `couleurAuto` — couleur de secours si ni l'un ni l'autre n'est fourni

### Autres
| Champ | Description |
|---|---|
| `artPrefabs[6]` | Objets décoratifs Sci-Fi dispersés dans les couloirs |
| `artEvery` | Place un objet décoratif toutes les N cellules |
| `goalPlatformMaterial` | Matériau de la plateforme lumineuse de sortie |
| `player` | `Transform` du joueur à téléporter à la cellule de départ |
| `seed` | Graine aléatoire (0 = aléatoire à chaque lancement) |

---

## Algorithme : Parcours en Profondeur (DFS) 3D

La grille a pour dimensions `(2·width+1) × (2·height+1) × (2·depth+1)`. Les indices impairs sont des cellules ; les indices pairs sont des murs entre cellules.

1. Toutes les cellules commencent **solides** (`grid[x,y,z] = true`).
2. Le DFS démarre depuis la cellule `(1,1,1)`.
3. À chaque étape, les 6 directions (±X, ±Y, ±Z) sont mélangées puis on tente de se déplacer vers un voisin non visité 2 cases plus loin. Si accessible, le mur entre eux est creusé (`grid[x,y,z] = false`).
4. Récursion jusqu'à ce que toutes les cellules soient visitées.

Cela garantit un **labyrinthe parfait** — toutes les cellules sont accessibles et il n'existe qu'un seul chemin entre deux cellules quelconques.

---

## Pipeline de génération

```
Generate()
  ├── ClearMaze()             — détruit les objets enfants existants
  ├── PrepareAutoMaterials()  — crée les matériaux de secours
  ├── CarveDFS(1,1,1)         — exécute l'algorithme du labyrinthe
  ├── ComputeAxes()           — calcule les positions monde de chaque tranche
  ├── BuildGeometry()         — crée un Cube par cellule solide de la grille
  ├── BuildGoalRoom()         — crée la plateforme de sortie + trigger GoalZone
  ├── StaticBatchingUtility.Combine() — optimise les draw calls
  ├── PlaceArt()              — disperse les prefabs décoratifs
  └── PlaceActors()           — déplace le Joueur à la cellule de départ (1,1,1)
```

---

## Sélection des matériaux

| Indice Y de la grille | Surface |
|---|---|
| Pair (pas dernière ligne) | Sol |
| Pair (dernière ligne) | Plafond |
| Impair | Mur |

Un matériau est choisi aléatoirement dans le tableau `materiaux` du `SurfaceStyle` correspondant. Si le tableau est vide, le matériau auto-généré est utilisé.

---

## Salle de sortie (`BuildGoalRoom`)

Placée à la **dernière cellule** `(gridW-2, gridH-2, gridD-2)` :
- Une plateforme verte lumineuse (matériau émissif).
- Un trigger `SphereCollider` de rayon = `cellSize × 0.48`.
- Un composant `GoalZone` pour gérer la détection du joueur.

---

## Placement de l'art (`PlaceArt`)

Toutes les `artEvery` cellules ouvertes reçoivent un prefab décoratif aléatoire :
- Placé légèrement au-dessus du sol de la cellule (`artHeightOffset`).
- Doté d'un `Rigidbody` (si absent) pour réagir aux changements de gravité.
- Doté d'un `BoxCollider` (si aucun collider enfant) pour ne pas traverser les sols.

---

## Commandes du menu contextuel

Clic droit sur le composant dans l'Inspecteur pour accéder à :
- **Generate Maze** — reconstruit le labyrinthe immédiatement (fonctionne aussi en mode Édition via `[ExecuteAlways]`).
- **Clear Maze** — détruit tous les objets enfants nommés `Maze_*`, `GoalRoom` ou `Art_Deco`.

---

## Dépendances

- **GoalZone** — instancié à la sortie.
- **PlayerController** — téléporté au départ.
- `SurfaceStyle` — classe sérialisable d'aide définie dans le même fichier.
