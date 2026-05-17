# GoalZone.cs

**Location:** `Assets/Scripts/GoalZone.cs`  
**Attached to:** The `GoalZoneTrigger` GameObject (created by `MazeGenerator` at the maze exit)

---

## Purpose

Detects when the player enters the exit zone and triggers the **victory screen**. It is the win condition of the game.

---

## How It Works

### `Awake` — Collider Safety Check

On startup the script ensures a **trigger collider** exists on the GameObject:

1. If no collider at all → adds a `SphereCollider` with radius 2.
2. If a collider exists but `isTrigger` is false → forces it to true.
3. If a trigger collider already exists → does nothing.

This prevents the common Unity bug where `OnTriggerEnter` never fires because the collider is not a trigger.

### `OnTriggerEnter` — Player Detection

When any collider enters the zone, the script checks if it belongs to the player using **two independent methods**:

```csharp
bool isPlayer = other.CompareTag("Player")           // tag check
             || other.GetComponent<PlayerController>() != null    // component check
             || other.GetComponentInParent<PlayerController>() != null;  // parent check
```

Using both tag and component checks makes the detection robust even if the player tag is not set in the Inspector.

If the player is detected:
1. Fires `onPlayerReached` (a public `UnityEvent` — can be wired in the Inspector).
2. Calls `StartAndSensitivityUI.Instance.ShowVictory()`.
3. If the UI instance is null, creates it on the fly via `EnsureInstance`.

---

## Public Fields (Inspector)

| Field | Description |
|---|---|
| `onPlayerReached` | `UnityEvent` — extra callbacks you can wire in the Inspector (optional) |

---

## Editor Visualization (`OnDrawGizmos`)

Draws a **semi-transparent green cube** in the Scene view at the goal zone's position so the exit is easy to spot during development.

```
Solid fill: green at 40% opacity
Wire outline: green at 100% opacity
```

---

## Dependencies

- **PlayerController** — used to identify the player.
- **StartAndSensitivityUI** — called to show the victory screen.
- **MazeGenerator** — creates and places this component at the last cell of the maze.

---

## Setup (automatic)

`MazeGenerator.BuildGoalRoom()` creates the `GoalZoneTrigger` GameObject and adds `GoalZone` automatically. No manual setup needed in the Inspector.
