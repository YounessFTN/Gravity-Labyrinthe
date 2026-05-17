# GravityController.cs

**Location:** `Assets/Scripts/GravityController.cs`  
**Attached to:** The Player GameObject (same as `PlayerController`)

---

## Purpose

Lets the player shift the direction of gravity at runtime. When a gravity key is pressed, the script smoothly rotates the player to the new "up" direction, applies the new gravity vector via `Physics.gravity`, and enforces a cooldown before the next shift.

---

## Public Fields (Inspector)

| Field | Default | Description |
|---|---|---|
| `gravityStrength` | 9.81 | Magnitude of gravity (same as Earth default) |
| `rotationDuration` | 0.5 | Seconds to rotate the player to the new orientation |
| `gravityCooldown` | 3 | Seconds before the player can shift gravity again |
| `gravityStatusUI` | — | Reference to the HUD panel (auto-created if null) |
| `autoCreateGravityStatusUI` | true | Creates the UI automatically if not assigned |

---

## Controls

| Key | Action |
|---|---|
| `Z` | Shift gravity to the **left** |
| `C` | Shift gravity to the **right** |
| `X` | Shift gravity **forward** |

These are mapped via Unity's Input System (`OnGravityLeft`, `OnGravityRight`, `OnGravityFront`).

---

## How It Works

### Gravity Shift Coroutine

When a gravity key is pressed and the cooldown is ready:

1. **Freeze** — sets `Physics.gravity = Vector3.zero` and zeroes `Rigidbody.linearVelocity` so the player floats during the transition.
2. **Rotate** — uses `Quaternion.Slerp` over `rotationDuration` seconds to smoothly rotate the player so the new "down" aligns with the new gravity direction.
3. **Apply gravity** — sets `Physics.gravity` to the new direction × `gravityStrength`.
4. **Cooldown** — waits `gravityCooldown` seconds, updating the UI bar each frame.
5. **Ready** — sets `canChangeGravity = true` and notifies the UI.

```
press key
  └─ TryChangeGravity()
       └─ GetNearestAxis(direction)   ← snaps to nearest world axis
            └─ StartCoroutine(ChangeGravity)
                 1. freeze physics
                 2. Slerp rotation  ← updates UI progress bar
                 3. apply new Physics.gravity
                 4. cooldown loop   ← updates UI cooldown bar
                 5. unlock gravity shift
```

### `GetNearestAxis`

Converts an arbitrary direction vector to the nearest cardinal axis (±X, ±Y, ±Z). This prevents diagonal gravity and keeps the game manageable.

---

## Dependencies

- **Rigidbody** — must be on the same GameObject.
- **GravityStatusUI** — updated every frame during transitions and cooldown.
- **Input System** — action names `GravityLeft`, `GravityRight`, `GravityFront`.
- `Physics.gravity` — a **global** Unity setting; changing it affects every Rigidbody in the scene.

---

## Important Note

`Physics.gravity` is a global setting. Changing it mid-game affects all physics objects (art decorations, etc.), which is intentional — the whole maze world obeys the new gravity direction.
