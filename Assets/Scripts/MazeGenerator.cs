using UnityEngine;

[ExecuteAlways]
public class MazeGenerator : MonoBehaviour
{
    [Header("Dimensions")]
    [Range(2, 7)] public int width = 4;
    [Range(2, 7)] public int height = 4;
    [Range(2, 7)] public int depth = 4;

    [Header("Geometry")]
    public float cellSize = 4f;
    public float wallThickness = 0.5f;

    [Header("Materials")]
    public Material wallMaterial;

    [Header("Actors")]
    public Transform player;
    public Transform goal;

    [Header("Seed (0 = aléatoire)")]
    public int seed = 0;

    private bool[,,] grid;
    private int gridW, gridH, gridD;
    private float[] posX, sizeX, posY, sizeY, posZ, sizeZ;

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

        gridW = 2 * width + 1;
        gridH = 2 * height + 1;
        gridD = 2 * depth + 1;
        grid = new bool[gridW, gridH, gridD];

        for (int x = 0; x < gridW; x++)
            for (int y = 0; y < gridH; y++)
                for (int z = 0; z < gridD; z++)
                    grid[x, y, z] = true;

        CarveDFS(1, 1, 1);
        ComputeAxes();
        BuildGeometry();
        PlaceActors();
    }

    [ContextMenu("Clear Maze")]
    public void ClearMaze()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("Maze_"))
                DestroyImmediate(child.gameObject);
        }
    }

    // DFS récursif en 6 directions (±X, ±Y, ±Z)
    void CarveDFS(int x, int y, int z)
    {
        grid[x, y, z] = false;

        int[] dx = {  2, -2,  0,  0,  0,  0 };
        int[] dy = {  0,  0,  2, -2,  0,  0 };
        int[] dz = {  0,  0,  0,  0,  2, -2 };

        // Mélange aléatoire des directions
        for (int i = 5; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (dx[i], dx[j]) = (dx[j], dx[i]);
            (dy[i], dy[j]) = (dy[j], dy[i]);
            (dz[i], dz[j]) = (dz[j], dz[i]);
        }

        for (int i = 0; i < 6; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];
            int nz = z + dz[i];

            if (InBounds(nx, ny, nz) && grid[nx, ny, nz])
            {
                // Ouvre le mur entre la cellule actuelle et le voisin
                grid[x + dx[i] / 2, y + dy[i] / 2, z + dz[i] / 2] = false;
                CarveDFS(nx, ny, nz);
            }
        }
    }

    bool InBounds(int x, int y, int z) =>
        x > 0 && x < gridW - 1 &&
        y > 0 && y < gridH - 1 &&
        z > 0 && z < gridD - 1;

    void ComputeAxes()
    {
        (posX, sizeX) = BuildAxis(gridW);
        (posY, sizeY) = BuildAxis(gridH);
        (posZ, sizeZ) = BuildAxis(gridD);
    }

    (float[] pos, float[] size) BuildAxis(int count)
    {
        var pos  = new float[count];
        var size = new float[count];
        float cursor = 0f;
        for (int i = 0; i < count; i++)
        {
            size[i] = (i % 2 == 0) ? wallThickness : cellSize;
            pos[i]  = cursor + size[i] / 2f;
            cursor += size[i];
        }
        return (pos, size);
    }

    void BuildGeometry()
    {
        for (int gx = 0; gx < gridW; gx++)
            for (int gy = 0; gy < gridH; gy++)
                for (int gz = 0; gz < gridD; gz++)
                    if (grid[gx, gy, gz])
                        CreateCube(
                            $"Maze_Wall_{gx}_{gy}_{gz}",
                            new Vector3(sizeX[gx], sizeY[gy], sizeZ[gz]),
                            new Vector3(posX[gx], posY[gy], posZ[gz]));

        // Fusionne tous les renderers en un seul draw call
        StaticBatchingUtility.Combine(gameObject);
    }

    void CreateCube(string objName, Vector3 size, Vector3 localPos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale   = size;
        if (wallMaterial != null)
            go.GetComponent<Renderer>().sharedMaterial = wallMaterial;
    }

    void PlaceActors()
    {
        // Départ : première salle (1,1,1)
        if (player != null)
            player.position = transform.TransformPoint(
                new Vector3(posX[1], posY[1], posZ[1]));

        // Arrivée : dernière salle (gridW-2, gridH-2, gridD-2)
        if (goal != null)
            goal.position = transform.TransformPoint(
                new Vector3(posX[gridW - 2], posY[gridH - 2], posZ[gridD - 2]));
    }
}
