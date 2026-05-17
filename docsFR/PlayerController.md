# PlayerController.cs

**Emplacement :** `Assets/Scripts/PlayerController.cs`  
**Attaché à :** Le GameObject Joueur (avec `Rigidbody` + `CapsuleCollider`)

---

## Rôle

Contrôle tout ce qui concerne le joueur : déplacement, rotation de la caméra, saut et verrouillage du curseur. Il reçoit les entrées via le **Input System** d'Unity et expose des méthodes pour que d'autres scripts puissent activer ou désactiver les contrôles.

---

## Champs publics (Inspecteur)

| Champ | Défaut | Description |
|---|---|---|
| `moveSpeed` | 5 | Vitesse de déplacement en unités/seconde |
| `mouseSensitivity` | 2 | Vitesse de rotation de la caméra |
| `minMouseSensitivity` | 0.2 | Sensibilité minimale (limitée par l'UI) |
| `maxMouseSensitivity` | 8 | Sensibilité maximale (limitée par l'UI) |
| `jumpForce` | 5 | Impulsion appliquée lors d'un saut |
| `groundCheckDistance` | 0.1 | Rayon supplémentaire pour détecter le sol |
| `controlsEnabled` | true | Si false, le joueur ne peut ni bouger ni regarder |
| `autoCreateStartUI` | true | Si true, crée l'écran de démarrage automatiquement au Start |
| `cameraTransform` | — | Le Transform de la caméra enfant (pour la rotation verticale) |

---

## Fonctionnement

### Déplacement (`FixedUpdate`)

Le déplacement est appliqué dans `FixedUpdate` pour rester indépendant du framerate.

```
vitesse = (avant * inputY + droite * inputX) * moveSpeed
        + vitesseDeChuteCouranteSelonGravité
```

La vitesse de chute est conservée dans la direction de la gravité actuelle — c'est ce qui permet au mécanisme de changement de gravité de fonctionner correctement (le joueur continue de tomber dans la nouvelle direction après un changement).

### Caméra (`Update`)

- La rotation **horizontale** tourne tout le corps du joueur autour de `Vector3.up`.
- La rotation **verticale** incline uniquement la caméra, bloquée entre -90° et 90° pour éviter le basculement.

### Détection du sol (`IsGrounded`)

Utilise `Physics.CheckSphere` sous les pieds du joueur (calculé à partir des dimensions du `CapsuleCollider`). La direction de la gravité est utilisée pour que la détection du sol pointe toujours vers le bas relatif à la gravité courante.

```csharp
Vector3 feetCenter = transform.position + gravityDirection * (halfHeight - radius);
Physics.CheckSphere(feetCenter, radius + groundCheckDistance, ~0, QueryTriggerInteraction.Ignore);
```

Les triggers sont exclus pour que la zone de sortie (GoalZone) ne soit pas comptée comme sol.

---

## Méthodes principales

| Méthode | Appelée par | Description |
|---|---|---|
| `OnMove(InputValue)` | Input System | Stocke l'entrée WASD/stick |
| `OnLook(InputValue)` | Input System | Stocke le delta souris/stick |
| `OnJump(InputValue)` | Input System | Demande un saut si au sol |
| `SetControlsEnabled(bool)` | `StartAndSensitivityUI` | Bloque ou restaure les entrées |
| `SetMouseSensitivity(float)` | `StartAndSensitivityUI` | Ajuste la sensibilité en jeu |
| `LockCursor(bool)` | `StartAndSensitivityUI` | Verrouille/déverrouille le curseur |

---

## Dépendances

- **Input System** — nécessite un `InputActionAsset` configuré avec les actions `Move`, `Look`, `Jump`, `GravityLeft/Right/Front`.
- **StartAndSensitivityUI** — créé automatiquement au `Start` si `autoCreateStartUI` est true.
- **GravityController** — partage le même GameObject ; `PlayerController.OnGravityLeft()` est un stub (la logique réelle est dans `GravityController`).
