using UnityEngine;

// Configuration d'une surface du labyrinthe (mur / sol / toit)
[System.Serializable]
public class SurfaceStyle
{
    [Tooltip("Matériaux disponibles — un est choisi aléatoirement pour chaque cube. Laisser vide = couleur automatique.")]
    public Material[] materiaux;

    [Tooltip("Couleur utilisée si aucun matériau n'est assigné.")]
    public Color couleurAuto = Color.gray;
}

[ExecuteAlways]
public class MazeGenerator : MonoBehaviour
{
    [Header("Dimensions")]
    [Range(2, 7)] public int width  = 4;
    [Range(2, 7)] public int height = 4;
    [Range(2, 7)] public int depth  = 4;

    [Header("Géométrie")]
    public float cellSize      = 4f;
    public float wallThickness = 0.5f;

    [Header("Style des surfaces")]
    public SurfaceStyle murs  = new() { couleurAuto = new Color(0.22f, 0.36f, 0.58f) };
    public SurfaceStyle sol   = new() { couleurAuto = new Color(0.76f, 0.70f, 0.60f) };
    public SurfaceStyle toit  = new() { couleurAuto = new Color(0.13f, 0.13f, 0.20f) };

    [Header("Zone de sortie")]
    public Material goalPlatformMaterial;

    [Header("Acteurs")]
    public Transform player;

    [Header("Seed (0 = aléatoire)")]
    public int seed = 0;

    private bool[,,] grid;
    private int gridW, gridH, gridD;
    private float[] posX, sizeX, posY, sizeY, posZ, sizeZ;

    // Matériaux de secours créés une seule fois quand les tableaux sont vides
    private Material _autoWall, _autoFloor, _autoCeiling;

    void Start()
    {
        if (Application.isPlaying)
            Generate();
    }

    [ContextMenu("Generate Maze")]
    public void Generate()
    {
        if (seed != 0)
            Random.InitState(seed);

        ClearMaze();
        PrepareAutoMaterials();

        gridW = 2 * width  + 1;
        gridH = 2 * height + 1;
        gridD = 2 * depth  + 1;
        grid  = new bool[gridW, gridH, gridD];

        for (int x = 0; x < gridW; x++)
            for (int y = 0; y < gridH; y++)
                for (int z = 0; z < gridD; z++)
                    grid[x, y, z] = true;

        CarveDFS(1, 1, 1);
        ComputeAxes();
        BuildGeometry();
        BuildGoalRoom();
        StaticBatchingUtility.Combine(gameObject);
        PlaceActors();
    }

    // Crée un matériau opaque unique quand le tableau d'une surface est vide
    void PrepareAutoMaterials()
    {
        _autoFloor   = HasMats(sol)  ? null : MakeMat(sol.couleurAuto);
        _autoCeiling = HasMats(toit) ? null : MakeMat(toit.couleurAuto);
        _autoWall    = HasMats(murs) ? null : MakeMat(murs.couleurAuto);
    }

    static bool HasMats(SurfaceStyle s) =>
        s.materiaux != null && System.Array.Exists(s.materiaux, m => m != null);

    static Shader LitShader()
    {
        return Shader.Find("Universal Render Pipeline/Lit")  // URP
            ?? Shader.Find("HDRP/Lit")                       // HDRP
            ?? Shader.Find("Standard");                       // Built-in
    }

    static Material MakeMat(Color color) =>
        new Material(LitShader()) { color = color };

    // Choisit un matériau au hasard dans le tableau, ou retourne le matériau auto
    static Material Pick(SurfaceStyle style, Material autoMat)
    {
        if (autoMat != null)
            return autoMat;

        // Filtre les slots vides et pioche au hasard
        int count = 0;
        foreach (var m in style.materiaux)
            if (m != null) count++;

        if (count == 0)
            return MakeMat(style.couleurAuto);

        int pick = Random.Range(0, count);
        int idx  = 0;
        foreach (var m in style.materiaux)
        {
            if (m == null) continue;
            if (idx == pick) return m;
            idx++;
        }
        return null;
    }

    [ContextMenu("Clear Maze")]
    public void ClearMaze()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("Maze_") || child.name == "GoalRoom")
                DestroyImmediate(child.gameObject);
        }
    }

    // ── DFS récursif en 6 directions (±X, ±Y, ±Z) ──────────────────────────────

    void CarveDFS(int x, int y, int z)
    {
        grid[x, y, z] = false;

        int[] dx = {  2, -2,  0,  0,  0,  0 };
        int[] dy = {  0,  0,  2, -2,  0,  0 };
        int[] dz = {  0,  0,  0,  0,  2, -2 };

        for (int i = 5; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (dx[i], dx[j]) = (dx[j], dx[i]);
            (dy[i], dy[j]) = (dy[j], dy[i]);
            (dz[i], dz[j]) = (dz[j], dz[i]);
        }

        for (int i = 0; i < 6; i++)
        {
            int nx = x + dx[i], ny = y + dy[i], nz = z + dz[i];
            if (InBounds(nx, ny, nz) && grid[nx, ny, nz])
            {
                grid[x + dx[i] / 2, y + dy[i] / 2, z + dz[i] / 2] = false;
                CarveDFS(nx, ny, nz);
            }
        }
    }

    bool InBounds(int x, int y, int z) =>
        x > 0 && x < gridW - 1 &&
        y > 0 && y < gridH - 1 &&
        z > 0 && z < gridD - 1;

    // ── Axes ────────────────────────────────────────────────────────────────────

    void ComputeAxes()
    {
        (posX, sizeX) = BuildAxis(gridW);
        (posY, sizeY) = BuildAxis(gridH);
        (posZ, sizeZ) = BuildAxis(gridD);
    }

    (float[] pos, float[] size) BuildAxis(int count)
    {
        var pos   = new float[count];
        var size  = new float[count];
        float cur = 0f;
        for (int i = 0; i < count; i++)
        {
            size[i] = (i % 2 == 0) ? wallThickness : cellSize;
            pos[i]  = cur + size[i] / 2f;
            cur    += size[i];
        }
        return (pos, size);
    }

    // ── Sélection du matériau ────────────────────────────────────────────────────
    //   gy pair  = dalle horizontale (sol bas, toit sommet)
    //   gy impair = mur vertical

    Material PickMaterial(int gy)
    {
        if (gy % 2 == 0)
            return gy == gridH - 1
                ? Pick(toit, _autoCeiling)
                : Pick(sol,  _autoFloor);
        return Pick(murs, _autoWall);
    }

    // ── Construction des cubes ──────────────────────────────────────────────────

    void BuildGeometry()
    {
        for (int gx = 0; gx < gridW; gx++)
            for (int gy = 0; gy < gridH; gy++)
                for (int gz = 0; gz < gridD; gz++)
                    if (grid[gx, gy, gz])
                        CreateCube(
                            $"Maze_Wall_{gx}_{gy}_{gz}",
                            new Vector3(sizeX[gx], sizeY[gy], sizeZ[gz]),
                            new Vector3(posX[gx], posY[gy], posZ[gz]),
                            PickMaterial(gy));
    }

    void CreateCube(string objName, Vector3 size, Vector3 localPos, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = size;
        if (mat != null)
            go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    // ── Zone de sortie ──────────────────────────────────────────────────────────

    void BuildGoalRoom()
    {
        Vector3 localCenter = new Vector3(posX[gridW - 2], posY[gridH - 2], posZ[gridD - 2]);
        Vector3 worldCenter = transform.TransformPoint(localCenter);

        var root = new GameObject("GoalRoom");
        root.transform.SetParent(transform, false);

        float platW   = cellSize * 0.9f;
        var platform  = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "GoalPlatform";
        platform.transform.SetParent(root.transform, true);
        platform.transform.position   = worldCenter - new Vector3(0f, cellSize * 0.45f, 0f);
        platform.transform.localScale = new Vector3(platW, wallThickness * 3f, platW);
        DestroyImmediate(platform.GetComponent<Collider>());

        var rend = platform.GetComponent<Renderer>();
        if (goalPlatformMaterial != null)
        {
            rend.sharedMaterial = goalPlatformMaterial;
        }
        else
        {
            var platMat = new Material(LitShader()) { color = new Color(0.1f, 0.9f, 0.4f) };
            platMat.SetColor("_EmissionColor", new Color(0f, 0.4f, 0.15f));
            platMat.EnableKeyword("_EMISSION");
            rend.material = platMat;
        }

        var triggerGO = new GameObject("GoalZoneTrigger");
        triggerGO.transform.SetParent(root.transform, true);
        triggerGO.transform.position = worldCenter;

        var col       = triggerGO.AddComponent<SphereCollider>();
        col.radius    = cellSize * 0.48f;
        col.isTrigger = true;

        triggerGO.AddComponent<GoalZone>();

        Debug.Log($"[MazeGenerator] GoalZone créée à {worldCenter}, rayon trigger = {col.radius:F2}");
    }

    // ── Placement des acteurs ────────────────────────────────────────────────────

    void PlaceActors()
    {
        if (player)
            player.position = transform.TransformPoint(
                new Vector3(posX[1], posY[1], posZ[1]));
    }
}
