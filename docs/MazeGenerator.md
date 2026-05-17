# MazeGenerator.cs

**Location:** `Assets/Scripts/MazeGenerator.cs`  
**Attached to:** An empty GameObject (e.g., `"Maze"`)

---

## Purpose

Generates a **3D procedural maze** using primitive cubes. It uses a Depth-First Search (DFS) algorithm to carve paths through a 3D grid, then builds the geometry, places a goal room at the exit, scatters decorative art objects, and positions the player at the start.

---

## Public Fields (Inspector)

### Dimensions
| Field | Range | Description |
|---|---|---|
| `width` | 2–7 | Number of cells on the X axis |
| `height` | 2–7 | Number of cells on the Y axis |
| `depth` | 2–7 | Number of cells on the Z axis |

### Geometry
| Field | Default | Description |
|---|---|---|
| `cellSize` | 4 | Size of each corridor cell in units |
| `wallThickness` | 0.5 | Thickness of each wall slab |

### Surface Style (`SurfaceStyle`)
Each surface type (walls, floor, ceiling) has a `SurfaceStyle` with:
- `materiaux` — array of materials (one picked at random per cube)
- `texture` — a `Texture2D` applied if no material is assigned
- `couleurAuto` — fallback color if neither is provided

### Other
| Field | Description |
|---|---|
| `artPrefabs[6]` | Sci-Fi decorative objects scattered in corridors |
| `artEvery` | Place art every N cells |
| `goalPlatformMaterial` | Material for the glowing exit platform |
| `player` | Player `Transform` to teleport to the start cell |
| `seed` | Random seed (0 = random each run) |

---

## Algorithm: 3D Depth-First Search

The grid has dimensions `(2·width+1) × (2·height+1) × (2·depth+1)`. Odd indices are cells; even indices are walls between cells.

1. Start all cells as **solid** (`grid[x,y,z] = true`).
2. Begin DFS from cell `(1,1,1)`.
3. At each step, shuffle the 6 directions (±X, ±Y, ±Z) and try to move to an unvisited neighbor 2 steps away. If reachable, carve the wall between them (`grid[x,y,z] = false`).
4. Recurse until all cells are visited.

This guarantees a **perfect maze** — every cell is reachable and there is exactly one path between any two cells.

---

## Generation Pipeline

```
Generate()
  ├── ClearMaze()          — destroy existing child objects
  ├── PrepareAutoMaterials() — create fallback materials
  ├── CarveDFS(1,1,1)      — run the maze algorithm
  ├── ComputeAxes()        — compute world positions for each grid slice
  ├── BuildGeometry()      — create one Cube per solid grid cell
  ├── BuildGoalRoom()      — create exit platform + GoalZone trigger
  ├── StaticBatchingUtility.Combine() — optimize draw calls
  ├── PlaceArt()           — scatter decorative prefabs
  └── PlaceActors()        — move Player to start cell (1,1,1)
```

---

## Material Selection

| Grid Y index | Surface |
|---|---|
| Even (not last row) | Floor |
| Even (last row) | Ceiling |
| Odd | Wall |

A random material is picked from the relevant `SurfaceStyle.materiaux` array. If the array is empty, the auto-generated material is used.

---

## Goal Room (`BuildGoalRoom`)

Placed at the **last cell** `(gridW-2, gridH-2, gridD-2)`:
- A glowing green platform (emissive material).
- A `SphereCollider` trigger with radius = `cellSize × 0.48`.
- A `GoalZone` component to handle player detection.

---

## Art Placement (`PlaceArt`)

Every `artEvery`-th open cell gets a random art prefab from `artPrefabs`:
- Spawned just above the cell floor (`artHeightOffset`).
- Given a `Rigidbody` (if not present) so it reacts to gravity shifts.
- Given a `BoxCollider` (if no child collider) so it does not fall through floors.

---

## Context Menu Commands

Right-click the component in the Inspector to access:
- **Generate Maze** — rebuild the maze immediately (works in Edit mode too via `[ExecuteAlways]`).
- **Clear Maze** — destroy all child objects named `Maze_*`, `GoalRoom`, or `Art_Deco`.

---

## Dependencies

- **GoalZone** — instantiated at the exit.
- **PlayerController** — teleported to the start.
- `SurfaceStyle` — serializable helper class defined in the same file.
