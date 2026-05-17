# GravityController.cs

**Emplacement :** `Assets/Scripts/GravityController.cs`  
**Attaché à :** Le GameObject Joueur (même que `PlayerController`)

---

## Rôle

Permet au joueur de modifier la direction de la gravité en temps réel. Lorsqu'une touche de gravité est pressée, le script fait pivoter le joueur en douceur vers la nouvelle direction "haut", applique le nouveau vecteur de gravité via `Physics.gravity`, puis impose un cooldown avant le prochain changement.

---

## Champs publics (Inspecteur)

| Champ | Défaut | Description |
|---|---|---|
| `gravityStrength` | 9.81 | Intensité de la gravité |
| `rotationDuration` | 0.5 | Durée en secondes de la rotation du joueur |
| `gravityCooldown` | 3 | Secondes à attendre avant le prochain changement |
| `gravityStatusUI` | — | Référence au panneau HUD (créé automatiquement si null) |
| `autoCreateGravityStatusUI` | true | Crée l'UI automatiquement si non assigné |

---

## Contrôles

| Touche | Action |
|---|---|
| `Z` | Gravité vers la **gauche** |
| `C` | Gravité vers la **droite** |
| `X` | Gravité vers l'**avant** |

Ces touches sont mappées via le Input System d'Unity (`OnGravityLeft`, `OnGravityRight`, `OnGravityFront`).

---

## Fonctionnement

### Coroutine de changement de gravité

Quand une touche est pressée et que le cooldown est terminé :

1. **Gel** — met `Physics.gravity = Vector3.zero` et annule la vélocité du `Rigidbody` pour que le joueur flotte pendant la transition.
2. **Rotation** — utilise `Quaternion.Slerp` sur `rotationDuration` secondes pour faire pivoter le joueur vers la nouvelle orientation.
3. **Application de la gravité** — définit `Physics.gravity` dans la nouvelle direction × `gravityStrength`.
4. **Cooldown** — attend `gravityCooldown` secondes en mettant à jour la barre UI à chaque frame.
5. **Prêt** — réactive le changement de gravité et notifie l'UI.

```
pression d'une touche
  └─ TryChangeGravity()
       └─ GetNearestAxis(direction)   ← aligne sur l'axe cardinal le plus proche
            └─ StartCoroutine(ChangeGravity)
                 1. gel de la physique
                 2. rotation Slerp  ← met à jour la barre de progression UI
                 3. application de Physics.gravity
                 4. boucle de cooldown  ← met à jour la barre de cooldown UI
                 5. déverrouillage du changement de gravité
```

### `GetNearestAxis`

Convertit un vecteur direction quelconque en l'axe cardinal le plus proche (±X, ±Y, ±Z). Cela empêche une gravité diagonale et rend le jeu jouable.

---

## Dépendances

- **Rigidbody** — doit être sur le même GameObject.
- **GravityStatusUI** — mis à jour à chaque frame pendant les transitions et le cooldown.
- **Input System** — noms d'actions : `GravityLeft`, `GravityRight`, `GravityFront`.
- `Physics.gravity` — paramètre **global** d'Unity ; le modifier affecte tous les Rigidbody de la scène.

---

## Note importante

`Physics.gravity` est un paramètre global. Le modifier en cours de partie affecte tous les objets physiques (décorations, etc.), ce qui est voulu — tout le monde du labyrinthe obéit à la nouvelle direction de gravité.
