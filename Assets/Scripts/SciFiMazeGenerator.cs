using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Générateur de labyrinthe Sci-Fi — ajoute ce composant sur un GameObject vide,
/// tous les prefabs s'assignent automatiquement.
/// Clic droit → "Generate Maze" pour construire.
/// </summary>
public class SciFiMazeGenerator : MonoBehaviour
{
    const int N = 0, E = 1, S = 2, W = 3;

    const string ROOT = "Assets/Sci-Fi Styled Modular Pack/Prefabs";

    // ── Prefabs (auto-assignés via Reset) ─────────────────────────────────────

    [Header("Corridor Prefabs")]
    public GameObject prefabDeadEnd;   // Corridor_1
    public GameObject prefabStraight;  // Corridor_I
    public GameObject prefabCorner;    // Corridor_L
    public GameObject prefabT;         // Corridor_T
    public GameObject prefabCross;     // Corridor_X

    [Header("Décor")]
    public GameObject prefabFloor;     // floor_1
    public GameObject prefabWall;      // blank_wall_A
    public GameObject prefabLight;     // light_celing_1

    [Header("Style des murs")]
    [Tooltip("Image appliquee sur tous les murs generes. Glisse ici une image importee dans Assets pour changer le style.")]
    public Texture2D wallTexture;
    [Tooltip("Materiau optionnel utilise comme base. S'il est vide, un materiau URP/Lit est cree automatiquement.")]
    public Material wallMaterial;
    public Color wallTint = Color.white;
    public Vector2 wallTextureTiling = Vector2.one;
    public Vector2 wallTextureOffset = Vector2.zero;

    [Header("Acteurs")]
    public Transform player;
    public Transform goal;

    [Header("Paramètres")]
    [Range(2, 20)] public int cols = 8;
    [Range(2, 20)] public int rows = 8;
    public float cellSize = 4f;
    [Range(0f, 1f)] public float lightDensity = 0.3f;
    [Tooltip("0 = aléatoire")]
    public int seed = 0;
    [Tooltip("Désactivé par défaut : les prefabs Corridor_* du pack occupent plusieurs cases et se chevauchent si on les pose à chaque cellule.")]
    public bool useCorridorPrefabs = false;

    [Header("Ajustement orientation (si pièces mal tournées)")]
    public float baseRotStraight = 0f;
    public float baseRotCorner   = 0f;
    public float baseRotT        = 0f;
    public Vector3 corridorOffset = Vector3.zero;
    public float baseRotWall = 0f;
    public Vector3 wallOffset = Vector3.zero;

    // ── Données internes ───────────────────────────────────────────────────────

    private bool[,,] open;
    private bool[,]  visited;
    private Material runtimeWallMaterial;

    // ── Auto-setup ────────────────────────────────────────────────────────────

    // Appelé automatiquement quand le composant est ajouté dans l'éditeur
    void Reset()
    {
        AutoAssignPrefabs();
        AutoAssignActors();
    }

    void OnValidate()
    {
        EnsureSetup();
    }

    void AutoAssignPrefabs()
    {
#if UNITY_EDITOR
        prefabDeadEnd  = Load($"{ROOT}/Corridors/Corridor_1.prefab");
        prefabStraight = Load($"{ROOT}/Corridors/Corridor_I.prefab");
        prefabCorner   = Load($"{ROOT}/Corridors/Corridor_L.prefab");
        prefabT        = Load($"{ROOT}/Corridors/Corridor_T.prefab");
        prefabCross    = Load($"{ROOT}/Corridors/Corridor_X.prefab");
        prefabFloor    = Load($"{ROOT}/Floors/floor_1.prefab");
        prefabWall     = Load($"{ROOT}/Walls/Simple/blank_wall_A.prefab");
        prefabLight    = Load($"{ROOT}/Lights/light_celing_1.prefab");
        wallTexture    = wallTexture ? wallTexture : LoadTexture("Assets/img (1).jpg");
#endif
    }

    void EnsureSetup()
    {
        cols = Mathf.Max(2, cols);
        rows = Mathf.Max(2, rows);
        cellSize = Mathf.Max(0.1f, cellSize);

#if UNITY_EDITOR
        prefabDeadEnd  = prefabDeadEnd  ? prefabDeadEnd  : Load($"{ROOT}/Corridors/Corridor_1.prefab");
        prefabStraight = prefabStraight ? prefabStraight : Load($"{ROOT}/Corridors/Corridor_I.prefab");
        prefabCorner   = prefabCorner   ? prefabCorner   : Load($"{ROOT}/Corridors/Corridor_L.prefab");
        prefabT        = prefabT        ? prefabT        : Load($"{ROOT}/Corridors/Corridor_T.prefab");
        prefabCross    = prefabCross    ? prefabCross    : Load($"{ROOT}/Corridors/Corridor_X.prefab");
        prefabFloor    = prefabFloor    ? prefabFloor    : Load($"{ROOT}/Floors/floor_1.prefab");
        prefabWall     = prefabWall     ? prefabWall     : Load($"{ROOT}/Walls/Simple/blank_wall_A.prefab");
        prefabLight    = prefabLight    ? prefabLight    : Load($"{ROOT}/Lights/light_celing_1.prefab");
        wallTexture    = wallTexture    ? wallTexture    : LoadTexture("Assets/img (1).jpg");
#endif
    }

    void AutoAssignActors()
    {
        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;

        var gz = FindAnyObjectByType<GoalZone>();
        if (gz != null) goal = gz.transform;
    }

#if UNITY_EDITOR
    static GameObject Load(string path) =>
        AssetDatabase.LoadAssetAtPath<GameObject>(path);

    static Texture2D LoadTexture(string path) =>
        AssetDatabase.LoadAssetAtPath<Texture2D>(path);
#endif

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        if (Application.isPlaying)
            Generate();
    }

    // ── Commandes ─────────────────────────────────────────────────────────────

    [ContextMenu("Generate Maze")]
    public void Generate()
    {
        EnsureSetup();

        if (seed != 0) Random.InitState(seed);

        ClearMaze();

        open    = new bool[cols, rows, 4];
        visited = new bool[cols, rows];

        CarveDFS(0, 0);
        BuildCells();
        PlaceActors();
    }

    [ContextMenu("Clear Maze")]
    public void ClearMaze()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c.name.StartsWith("Cell_"))
                DestroyImmediate(c.gameObject);
        }
    }

    [ContextMenu("Apply Wall Style To Existing Walls")]
    public void ApplyWallStyleToExistingWalls()
    {
        foreach (Transform cell in transform)
        {
            if (!cell.name.StartsWith("Cell_"))
                continue;

            foreach (Transform child in cell)
            {
                if (child.name.StartsWith("Generated_Wall") || child.name.Contains("wall"))
                    ApplyWallStyle(child.gameObject);
            }
        }
    }

    // ── Génération DFS ────────────────────────────────────────────────────────

    void CarveDFS(int x, int z)
    {
        visited[x, z] = true;
        int[] dirs = { N, E, S, W };

        for (int i = 3; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (dirs[i], dirs[j]) = (dirs[j], dirs[i]);
        }

        foreach (int d in dirs)
        {
            int nx = x + DX(d), nz = z + DZ(d);
            if (InBounds(nx, nz) && !visited[nx, nz])
            {
                open[x,  z,  d]      = true;
                open[nx, nz, Opp(d)] = true;
                CarveDFS(nx, nz);
            }
        }
    }

    // ── Construction ──────────────────────────────────────────────────────────

    void BuildCells()
    {
        for (int x = 0; x < cols; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                bool n = open[x, z, N];
                bool e = open[x, z, E];
                bool s = open[x, z, S];
                bool w = open[x, z, W];

                var cell = new GameObject($"Cell_{x}_{z}");
                cell.transform.SetParent(transform, false);
                cell.transform.localPosition = new Vector3(x * cellSize, 0f, z * cellSize);

                if (prefabFloor)
                    Spawn(prefabFloor, cell.transform, Vector3.zero, 0f);

                if (prefabLight && Random.value < lightDensity)
                    Spawn(prefabLight, cell.transform, Vector3.zero, 0f);

                if (useCorridorPrefabs)
                {
                    int c = (n ? 1 : 0) + (e ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0);
                    var (prefab, rot) = PickPiece(n, e, s, w, c);
                    if (prefab)
                        Spawn(prefab, cell.transform, corridorOffset, rot);
                }
                else
                {
                    BuildCellWalls(cell.transform, x, z, n, e, s, w);
                }
            }
        }
    }

    void BuildCellWalls(Transform cell, int x, int z, bool n, bool e, bool s, bool w)
    {
        float half = cellSize * 0.5f;

        if (z == 0 && !n)
            SpawnWall(cell, new Vector3(0f, 0f, -half), 0f);

        if (x == 0 && !w)
            SpawnWall(cell, new Vector3(-half, 0f, 0f), 90f);

        if (!e)
            SpawnWall(cell, new Vector3(half, 0f, 0f), 90f);

        if (!s)
            SpawnWall(cell, new Vector3(0f, 0f, half), 0f);
    }

    void SpawnWall(Transform parent, Vector3 localPos, float yRot)
    {
        if (prefabWall)
        {
            var wallInstance = Spawn(prefabWall, parent, localPos + wallOffset, baseRotWall + yRot);
            wallInstance.name = "Generated_Wall";
            ApplyWallStyle(wallInstance);
            return;
        }

        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Fallback_Wall";
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = localPos + wallOffset + new Vector3(0f, 2f, 0f);
        wall.transform.localRotation = Quaternion.Euler(0f, baseRotWall + yRot, 0f);
        wall.transform.localScale = new Vector3(cellSize, 4f, 0.2f);
        ApplyWallStyle(wall);
    }

    // ── Sélection du prefab ───────────────────────────────────────────────────

    (GameObject, float) PickPiece(bool n, bool e, bool s, bool w, int count)
    {
        return count switch
        {
            1 => (prefabDeadEnd ?? prefabStraight, baseRotStraight + DeadEndRot(n, e, s, w)),
            2 when n && s => (prefabStraight, baseRotStraight + 0f),
            2 when e && w => (prefabStraight, baseRotStraight + 90f),
            2             => (prefabCorner,   baseRotCorner   + CornerRot(n, e, s, w)),
            3             => (prefabT,        baseRotT        + TRot(n, e, s, w)),
            _             => (prefabCross,    0f),
        };
    }

    float DeadEndRot(bool n, bool e, bool s, bool w)
    {
        if (n) return 0f;
        if (e) return 90f;
        if (s) return 180f;
        return 270f;
    }

    float CornerRot(bool n, bool e, bool s, bool w)
    {
        if (n && e) return 0f;
        if (e && s) return 90f;
        if (s && w) return 180f;
        return 270f;
    }

    float TRot(bool n, bool e, bool s, bool _)
    {
        if (!n) return 90f;
        if (!e) return 180f;
        if (!s) return 270f;
        return 0f;
    }

    // ── Placement des acteurs ─────────────────────────────────────────────────

    void PlaceActors()
    {
        if (player != null)
            player.position = transform.TransformPoint(new Vector3(0f, 1f, 0f));

        if (goal != null)
            goal.position = transform.TransformPoint(
                new Vector3((cols - 1) * cellSize, 1f, (rows - 1) * cellSize));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    GameObject Spawn(GameObject prefab, Transform parent, Vector3 localPos, float yRot)
    {
        var go = Instantiate(prefab, parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);
        return go;
    }

    void ApplyWallStyle(GameObject wall)
    {
        Material mat = GetWallMaterial();
        if (mat == null)
            return;

        foreach (var renderer in wall.GetComponentsInChildren<Renderer>())
            renderer.sharedMaterial = mat;
    }

    Material GetWallMaterial()
    {
        if (wallMaterial == null && wallTexture == null)
            return null;

        if (runtimeWallMaterial == null)
        {
            runtimeWallMaterial = wallMaterial != null
                ? new Material(wallMaterial)
                : new Material(LitShader());

            runtimeWallMaterial.name = "Generated Wall Material";
        }

        Color tint = wallTexture != null ? Color.white : wallTint;
        runtimeWallMaterial.color = tint;
        SetColor(runtimeWallMaterial, "_BaseColor", tint);
        SetColor(runtimeWallMaterial, "_Color", tint);

        if (wallTexture != null)
        {
            SetTexture(runtimeWallMaterial, "_BaseMap", wallTexture);
            SetTexture(runtimeWallMaterial, "_MainTex", wallTexture);
            runtimeWallMaterial.mainTextureScale = wallTextureTiling;
            runtimeWallMaterial.mainTextureOffset = wallTextureOffset;
        }

        return runtimeWallMaterial;
    }

    static Shader LitShader()
    {
        return Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("HDRP/Lit")
            ?? Shader.Find("Standard");
    }

    static void SetTexture(Material mat, string propertyName, Texture texture)
    {
        if (mat.HasProperty(propertyName))
            mat.SetTexture(propertyName, texture);
    }

    static void SetColor(Material mat, string propertyName, Color color)
    {
        if (mat.HasProperty(propertyName))
            mat.SetColor(propertyName, color);
    }

    int  DX(int d)      => d == E ? 1 : d == W ? -1 : 0;
    int  DZ(int d)      => d == S ? 1 : d == N ? -1 : 0;
    int  Opp(int d)     => (d + 2) % 4;
    bool InBounds(int x, int z) => x >= 0 && x < cols && z >= 0 && z < rows;
}
