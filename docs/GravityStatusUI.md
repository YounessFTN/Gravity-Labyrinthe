# GravityStatusUI.cs

**Location:** `Assets/Scripts/GravityStatusUI.cs`  
**Attached to:** A runtime-created GameObject (`"Gravity Status UI"`)

---

## Purpose

A sci-fi HUD panel drawn in the **bottom-left corner** of the screen. It shows:
- The current gravity state (`READY`, `GRAVITY SHIFT`, `RECHARGE X.Xs`)
- A cooldown progress bar
- The control key bindings (Z / C / X)
- A mini character figure that rotates to represent the current gravity direction

The entire UI is **built in code** — no prefab or scene reference needed.

---

## Singleton Pattern

```csharp
GravityStatusUI.EnsureInstance()
```

Called by `GravityController.Start()`. If a `GravityStatusUI` already exists in the scene it returns it; otherwise it creates a new GameObject with `DontDestroyOnLoad`.

---

## State Methods (called by GravityController)

| Method | When called | What it does |
|---|---|---|
| `SetGravity(direction, strength)` | After a shift completes | Rotates the mini character, resets fill bar to full |
| `SetTransitionProgress(t)` | Each frame during rotation | Shows "GRAVITY SHIFT" in pink, fill bar shows rotation progress |
| `SetCooldown(remaining, duration)` | Each frame during cooldown | Shows "RECHARGE X.Xs", fill bar drains left-to-right |
| `SetReady()` | When cooldown ends | Shows "READY" in cyan, fill bar full |
| `Bind(controller)` | At setup | Stores the reference to `GravityController` |

---

## Visual Layout

```
┌──────────────────────────────────────────┐  ← accent outline
│ ▌  GRAVITY CORE              READY       │
│ ▌  [Z]  GRAVITE GAUCHE                   │
│ ▌  [C]  GRAVITE DROITE      [mini char]  │
│ ▌  [X]  GRAVITE AVANT                    │
│ ▌                                        │
│    ══════════════════════════════        │  ← cooldown bar
└──────────────────────────────────────────┘
```

### Color Palette

| Name | Color | Used for |
|---|---|---|
| `AccentColor` | Cyan `#00D9FF` | Borders, READY state, fill bar |
| `HotColor` | Pink `#FF4390` | GRAVITY SHIFT / RECHARGE states |
| `TextColor` | Near-white | Key labels |
| `MutedTextColor` | Soft teal | Key action descriptions, title |
| `PanelColor` | Very dark blue (82% opacity) | Panel background |

---

## Mini Character

A tiny stick figure made from colored `Image` rectangles (head, body, arms, legs). It rotates smoothly via `Quaternion.Lerp` in `Update` to reflect the current gravity direction:

| Gravity axis | Character rotation |
|---|---|
| Down (default) | 0° |
| Right | 90° |
| Left | -90° |
| Up (inverted) | 180° |
| Forward | 45° |
| Back | -45° |

---

## Dependencies

- `UnityEngine.UI` — `Image`, `Text`, `Canvas`, `CanvasScaler`, `Outline`
- `GravityController` — provides the state updates
- No scene assets required; font falls back to Unity's built-in `LegacyRuntime.ttf`
