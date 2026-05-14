using UnityEngine;

[ExecuteAlways]
public class MazeGenerator : MonoBehaviour
{
    [Header("Dimensions")]
    public int width = 8;
    public int height = 8;

    [Header("Geometry")]
    public float cellSize = 4f;
    public float wallHeight = 4f;
    public float wallThickness = 0.5f;

    [Header("Materials")]
    public Material wallMaterial;
    public Material floorMaterial;

    [Header("Actors")]
    public Transform player;
    public Transform goal;

    private bool[,] grid;
    private int gridW, gridH;

    void Start()
    {
        if (Application.isPlaying)
            Generate();
    }

    [ContextMenu("Generate Maze")]
    public void Generate()
    {
        ClearMaze();

        gridW = 2 * width + 1;
        gridH = 2 * height + 1;
        grid = new bool[gridW, gridH];

        for (int x = 0; x < gridW; x++)
            for (int z = 0; z < gridH; z++)
                grid[x, z] = true;

        CarveDFS(1, 1);
        BuildGeometry();
        PlaceActors();
    }

    void ClearMaze()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("Maze_"))
                DestroyImmediate(child.gameObject);
        }
    }

    void CarveDFS(int x, int z)
    {
        grid[x, z] = false;

        int[] dx = { 0, 0, -2, 2 };
        int[] dz = { 2, -2, 0, 0 };

        // Shuffle directions for random maze
        for (int i = 3; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (dx[i], dx[j]) = (dx[j], dx[i]);
            (dz[i], dz[j]) = (dz[j], dz[i]);
        }

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int nz = z + dz[i];

            if (nx > 0 && nx < gridW - 1 && nz > 0 && nz < gridH - 1 && grid[nx, nz])
            {
                grid[x + dx[i] / 2, z + dz[i] / 2] = false;
                CarveDFS(nx, nz);
            }
        }
    }

    void BuildGeometry()
    {
        // Precompute world position and size for each grid column/row
        float[] posX = new float[gridW];
        float[] sizeX = new float[gridW];
        float[] posZ = new float[gridH];
        float[] sizeZ = new float[gridH];

        float cx = 0f;
        for (int gx = 0; gx < gridW; gx++)
        {
            sizeX[gx] = (gx % 2 == 0) ? wallThickness : cellSize;
            posX[gx] = cx + sizeX[gx] / 2f;
            cx += sizeX[gx];
        }

        float cz = 0f;
        for (int gz = 0; gz < gridH; gz++)
        {
            sizeZ[gz] = (gz % 2 == 0) ? wallThickness : cellSize;
            posZ[gz] = cz + sizeZ[gz] / 2f;
            cz += sizeZ[gz];
        }

        float totalX = cx;
        float totalZ = cz;

        CreateCube("Maze_Floor",
            new Vector3(totalX, wallThickness, totalZ),
            new Vector3(totalX / 2f, -wallThickness / 2f, totalZ / 2f),
            floorMaterial);

        CreateCube("Maze_Ceiling",
            new Vector3(totalX, wallThickness, totalZ),
            new Vector3(totalX / 2f, wallHeight + wallThickness / 2f, totalZ / 2f),
            floorMaterial);

        for (int gx = 0; gx < gridW; gx++)
        {
            for (int gz = 0; gz < gridH; gz++)
            {
                if (!grid[gx, gz]) continue;

                CreateCube($"Maze_Wall_{gx}_{gz}",
                    new Vector3(sizeX[gx], wallHeight, sizeZ[gz]),
                    new Vector3(posX[gx], wallHeight / 2f, posZ[gz]),
                    wallMaterial);
            }
        }
    }

    GameObject CreateCube(string objName, Vector3 size, Vector3 localPos, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = size;
        if (mat != null)
            go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    void PlaceActors()
    {
        // Start: room at grid (1,1)
        float startX = wallThickness + cellSize / 2f;
        float startZ = wallThickness + cellSize / 2f;

        if (player != null)
            player.position = transform.TransformPoint(new Vector3(startX, 1f, startZ));

        // Goal: room at grid (gridW-2, gridH-2)
        float endX = width * wallThickness + (width - 0.5f) * cellSize;
        float endZ = height * wallThickness + (height - 0.5f) * cellSize;

        if (goal != null)
            goal.position = transform.TransformPoint(new Vector3(endX, 1f, endZ));
    }
}
