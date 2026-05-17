# PlayerController.cs

**Location:** `Assets/Scripts/PlayerController.cs`  
**Attached to:** The Player GameObject (with `Rigidbody` + `CapsuleCollider`)

---

## Purpose

Controls everything related to the player: movement, camera rotation, jumping, and cursor locking. It receives input via Unity's **Input System** and exposes methods for other scripts to enable/disable controls.

---

## Public Fields (Inspector)

| Field | Default | Description |
|---|---|---|
| `moveSpeed` | 5 | Movement speed in units/second |
| `mouseSensitivity` | 2 | Mouse look speed |
| `minMouseSensitivity` | 0.2 | Minimum sensitivity (clamped by UI) |
| `maxMouseSensitivity` | 8 | Maximum sensitivity (clamped by UI) |
| `jumpForce` | 5 | Upward impulse applied when jumping |
| `groundCheckDistance` | 0.1 | Extra radius for ground detection sphere |
| `controlsEnabled` | true | If false, player cannot move or look |
| `autoCreateStartUI` | true | If true, creates the start screen automatically on Start |
| `cameraTransform` | — | The child Camera transform used for vertical look |

---

## How It Works

### Movement (`FixedUpdate`)

Movement is applied in `FixedUpdate` so it stays frame-rate independent.

```
velocity = (forward * inputY + right * inputX) * moveSpeed
         + currentFallVelocityAlongGravity
```

The fall velocity is preserved along the current gravity direction — this is what allows the gravity-shift mechanic to work correctly (the player keeps falling in the new direction after a gravity change).

### Camera (`Update`)

- **Horizontal** rotation rotates the whole player body around `Vector3.up`.
- **Vertical** rotation tilts only the camera, clamped between -90° and 90° to prevent flipping.

### Ground Check (`IsGrounded`)

Uses `Physics.CheckSphere` at the player's feet (calculated from the `CapsuleCollider` dimensions). The gravity direction is used so ground detection always points "down" relative to the current gravity.

```csharp
Vector3 feetCenter = transform.position + gravityDirection * (halfHeight - radius);
Physics.CheckSphere(feetCenter, radius + groundCheckDistance, ~0, QueryTriggerInteraction.Ignore);
```

Triggers are excluded so the GoalZone trigger does not count as ground.

---

## Key Methods

| Method | Called by | Description |
|---|---|---|
| `OnMove(InputValue)` | Input System | Stores WASD/stick input |
| `OnLook(InputValue)` | Input System | Stores mouse/stick delta |
| `OnJump(InputValue)` | Input System | Requests a jump if grounded |
| `SetControlsEnabled(bool)` | `StartAndSensitivityUI` | Freezes or restores input |
| `SetMouseSensitivity(float)` | `StartAndSensitivityUI` | Adjusts sensitivity at runtime |
| `LockCursor(bool)` | `StartAndSensitivityUI` | Locks/unlocks the mouse cursor |

---

## Dependencies

- **Input System** — requires an `InputActionAsset` configured with `Move`, `Look`, `Jump`, `GravityLeft/Right/Front` actions.
- **StartAndSensitivityUI** — created automatically at `Start` if `autoCreateStartUI` is true.
- **GravityController** — shares the same GameObject; `PlayerController.OnGravityLeft()` is a stub (actual logic is in `GravityController`).
