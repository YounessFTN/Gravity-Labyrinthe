# StartAndSensitivityUI.cs

**Location:** `Assets/Scripts/StartAndSensitivityUI.cs`  
**Attached to:** A runtime-created GameObject (`"Start And Sensitivity UI"`)

---

## Purpose

The central UI manager of the game. It handles **five distinct screens** built entirely in code:

1. **Start Screen** — shown before the game begins (with rules, background image, and a START button)
2. **Timer HUD** — displayed during gameplay (countdown + elapsed time)
3. **Sensitivity Panel** — an overlay to adjust mouse sensitivity mid-game
4. **Victory Screen** — shown when the player reaches the GoalZone
5. **Defeat Screen** — shown when the 3-minute timer reaches zero

---

## Singleton Pattern

```csharp
StartAndSensitivityUI.EnsureInstance(PlayerController p)
```

Called by `PlayerController.Start()` and `GoalZone.OnTriggerEnter()`. Returns the existing instance or creates one. `DontDestroyOnLoad` keeps it across scene reloads.

The static `Instance` property gives `GoalZone` direct access to call `ShowVictory()`.

---

## Game States

```
[Start Screen]  ──── ENTER/SPACE/click ────▶  [Playing]
                                                  │
                                         timer reaches 0
                                                  │
                                                  ▼
[Victory Screen] ◀── GoalZone reached ──  [Playing]  ──── ▶  [Defeat Screen]
       │                                                              │
       └─────────── REJOUER (RestartGame) ────────────────────────────┘
```

---

## Timer

- Duration: **180 seconds** (3 minutes), set by the `GameDuration` constant.
- Counts down in `Update` using `Time.deltaTime`.
- Display format: `MM:SS`
- At ≤ 30 seconds remaining, the timer color flashes red using `Mathf.Sin`.
- Elapsed time is also shown (`ECOULE MM:SS`).

---

## Mouse Sensitivity Panel

- Toggled by pressing **Left Ctrl** or **Right Ctrl** during gameplay.
- While open, player movement is **disabled** and the cursor is **unlocked**.
- Sensitivity can be adjusted via:
  - Mouse scroll wheel
  - `+` / `=` key (increase by 0.1)
  - `-` key (decrease by 0.1)
- A fill bar shows the normalized sensitivity between `minMouseSensitivity` and `maxMouseSensitivity`.
- State label: `LOW` (< 35%), `BALANCED` (35–72%), `HIGH` (> 72%).

---

## Start Screen Details

### Background Image

The script searches for a background image in this order:
1. `Resources/StartBackground` (works in builds)
2. `Assets/bg-start.png` (editor path)
3. Any texture named `StartBackground` in `Assets/`
4. Any 16:9 image (aspect ratio 1.65–1.90) found anywhere in `Assets/`

The image is displayed full-screen with `AspectRatioFitter` in `EnvelopeParent` mode.

### Content
- Top HUD bar: school name, student author
- Left rules panel: controls and objective
- Center panel: title, START button, keyboard hint

---

## UI Screens Summary

| Screen | Trigger | Controls blocked? | Cursor |
|---|---|---|---|
| Start | On `Awake` | Yes (`Time.timeScale = 0`) | Visible |
| Playing | ENTER/SPACE | No | Locked |
| Sensitivity Panel | Left/Right Ctrl | Yes (movement only) | Visible |
| Victory | `ShowVictory()` | Yes | Visible |
| Defeat | Timer = 0 | Yes | Visible |

---

## Key Methods

| Method | Description |
|---|---|
| `EnsureInstance(PlayerController)` | Static factory — gets or creates the UI |
| `Bind(PlayerController)` | Links the UI to a player instance (called after scene reload) |
| `ShowVictory()` | Shows the victory screen, stops timer, disables controls |
| `ShowStart()` | Resets to the start screen state |
| `StartGame()` | Transitions to gameplay |

---

## Scene Reload (`RestartGame`)

```csharp
Time.timeScale = 1f;
SceneManager.LoadScene(SceneManager.GetActiveScene().name);
```

Reloads the current scene. Because this GameObject has `DontDestroyOnLoad`, `Bind()` is called again by the new `PlayerController` instance to re-link everything.

---

## Dependencies

- **PlayerController** — controls are enabled/disabled and cursor state is managed through it.
- **UnityEngine.UI** — `Image`, `Text`, `Button`, `Canvas`, `CanvasScaler`, `Outline`
- **UnityEngine.InputSystem** — reads keyboard and mouse directly for UI navigation
- **UnityEngine.SceneManagement** — for scene reload on restart
- No prefabs or scene assets required (except the optional background image)
