# GoalZone.cs

**Emplacement :** `Assets/Scripts/GoalZone.cs`  
**Attaché à :** Le GameObject `GoalZoneTrigger` (créé par `MazeGenerator` à la sortie du labyrinthe)

---

## Rôle

Détecte quand le joueur entre dans la zone de sortie et déclenche l'**écran de victoire**. C'est la condition de victoire du jeu.

---

## Fonctionnement

### `Awake` — Vérification du collider

Au démarrage, le script s'assure qu'un **collider trigger** existe sur le GameObject :

1. Si aucun collider → ajoute un `SphereCollider` de rayon 2.
2. Si un collider existe mais que `isTrigger` est false → le force à true.
3. Si un trigger collider existe déjà → ne fait rien.

Cela évite le bug Unity classique où `OnTriggerEnter` ne se déclenche jamais parce que le collider n'est pas en mode trigger.

### `OnTriggerEnter` — Détection du joueur

Quand un collider entre dans la zone, le script vérifie s'il appartient au joueur via **deux méthodes indépendantes** :

```csharp
bool isPlayer = other.CompareTag("Player")                               // vérification par tag
             || other.GetComponent<PlayerController>() != null           // vérification par composant
             || other.GetComponentInParent<PlayerController>() != null;  // vérification sur le parent
```

Utiliser à la fois le tag et le composant rend la détection robuste même si le tag joueur n'est pas configuré dans l'Inspecteur.

Si le joueur est détecté :
1. Déclenche `onPlayerReached` (un `UnityEvent` public — peut être câblé dans l'Inspecteur).
2. Appelle `StartAndSensitivityUI.Instance.ShowVictory()`.
3. Si l'instance UI est null, la crée à la volée via `EnsureInstance`.

---

## Champs publics (Inspecteur)

| Champ | Description |
|---|---|
| `onPlayerReached` | `UnityEvent` — callbacks supplémentaires câblables dans l'Inspecteur (optionnel) |

---

## Visualisation éditeur (`OnDrawGizmos`)

Dessine un **cube vert semi-transparent** dans la vue Scène à la position de la zone de sortie pour la repérer facilement en développement.

```
Remplissage : vert à 40% d'opacité
Contour filaire : vert à 100% d'opacité
```

---

## Dépendances

- **PlayerController** — utilisé pour identifier le joueur.
- **StartAndSensitivityUI** — appelé pour afficher l'écran de victoire.
- **MazeGenerator** — crée et place ce composant à la dernière cellule du labyrinthe.

---

## Configuration (automatique)

`MazeGenerator.BuildGoalRoom()` crée le GameObject `GoalZoneTrigger` et ajoute `GoalZone` automatiquement. Aucune configuration manuelle dans l'Inspecteur n'est nécessaire.
