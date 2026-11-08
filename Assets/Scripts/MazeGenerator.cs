using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MazeGenerator : MonoBehaviour
{
    private const int MinimumSize = 5;
    private const float CellSize = 1.5f;

    [SerializeField, Min(MinimumSize)] private int sizeX = MinimumSize;
    [SerializeField, Min(MinimumSize)] private int sizeZ = MinimumSize;
    [SerializeField] private Transform wallPrefab;
    [SerializeField] private GameObject player;
    [SerializeField] private float wallYPos = 0.75f;
    [SerializeField] private Transform startMarker;
    [SerializeField] private Transform finishMarker;
    [SerializeField] private MeshRenderer floorRenderer;
    
    [SerializeField] private Material northWallMaterial;
    [SerializeField] private Material southWallMaterial;
    [SerializeField] private Material eastWallMaterial;
    [SerializeField] private Material westWallMaterial;

    [Flags]
    private enum Cell
    {
        North = 1,
        East = 2,
        South = 4,
        West = 8,
        Visited = 16
    }

    private static readonly (Vector2Int offset, Cell passage, Cell opposite)[] Directions =
    {
        (Vector2Int.up, Cell.North, Cell.South),
        (Vector2Int.right, Cell.East, Cell.West),
        (Vector2Int.down, Cell.South, Cell.North),
        (Vector2Int.left, Cell.West, Cell.East)
    };

    public Vector3 StartPosition { get; private set; }
    public Vector3 FinishPosition { get; private set; }
    public bool IsGenerated { get; private set; }
    private Cell[,] cells;
    private Material floorMaterialInstance;
    public int SizeX => sizeX;
    public int SizeZ => sizeZ;

    //world position of the centre of a cell
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return transform.TransformPoint(CellCenter(cell.x, cell.y));
    }

    //cells you can walk to from this cell (no wall in between)
    public List<Vector2Int> GetOpenNeighbours(Vector2Int cell)
    {
        var result = new List<Vector2Int>(4);
        foreach (var direction in Directions)
        {
            if ((cells[cell.x, cell.y] & direction.passage) != 0)
                result.Add(cell + direction.offset);
        }
        return result;
    }
    
    //which cell a world position is in (clamped, so positions outside the maze use the nearest edge cell)
    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition);
        int x = Mathf.RoundToInt(local.x / CellSize + (sizeX - 1) * 0.5f);
        int z = Mathf.RoundToInt(local.z / CellSize + (sizeZ - 1) * 0.5f);
        return new Vector2Int(Mathf.Clamp(x, 0, sizeX - 1), Mathf.Clamp(z, 0, sizeZ - 1));
    }

    private void Awake()
    {
        if (wallPrefab == null || startMarker == null || finishMarker == null)
        {
            Debug.LogError("Assign the wall prefab, start marker, and finish marker on Maze Generator.", this);
            return;
        }

        if (northWallMaterial == null || southWallMaterial == null || eastWallMaterial == null ||
            westWallMaterial == null)
        {
            Debug.LogError("Assign the north, south, east, and west wall materials on Maze Generator.", this);
            return;
        }

        sizeX = Mathf.Max(sizeX, MinimumSize);
        sizeZ = Mathf.Max(sizeZ, MinimumSize);
        SizeFloor();
        cells = CarvePassages();
        DrawWalls(cells);

        StartPosition = transform.TransformPoint(CellCenter(0, 0));
        FinishPosition = transform.TransformPoint(CellCenter(sizeX - 1, sizeZ - 1));
        PlaceMarker(startMarker, StartPosition);
        PlaceMarker(finishMarker, FinishPosition);
        Instantiate(player, startMarker.position, startMarker.rotation);
        IsGenerated = true;
    }

    private void SizeFloor()
    {
        if (floorRenderer == null)
        {
            Debug.LogError("Assign the floor quad renderer on Maze Generator.", this);
            return;
        }

        Transform floor = floorRenderer.transform;
        floor.position = transform.position;
        floor.rotation = transform.rotation * Quaternion.Euler(90f, 0f, 0f);
        floor.localScale = new Vector3(sizeX * CellSize, sizeZ * CellSize, floor.localScale.z);

        // Floor.png contains four checker squares along each axis.
        floorMaterialInstance = floorRenderer.material;
        Vector2 tiling = new Vector2(sizeX / 4f, sizeZ / 4f);
        floorMaterialInstance.SetTextureScale("_BaseMap", tiling);
        floorMaterialInstance.SetTextureScale("_MainTex", tiling);
    }

    private void OnDestroy()
    {
        if (floorMaterialInstance != null)
            Destroy(floorMaterialInstance);
    }

    private Cell[,] CarvePassages()
    {
        var cells = new Cell[sizeX, sizeZ];
        var path = new Stack<Vector2Int>();
        var candidates = new int[Directions.Length];

        path.Push(Vector2Int.zero);
        cells[0, 0] = Cell.Visited;

        while (path.Count > 0)
        {
            Vector2Int current = path.Peek();
            int candidateCount = 0;

            for (int i = 0; i < Directions.Length; i++)
            {
                Vector2Int neighbor = current + Directions[i].offset;
                if (neighbor.x >= 0 && neighbor.x < sizeX && neighbor.y >= 0 && neighbor.y < sizeZ &&
                    (cells[neighbor.x, neighbor.y] & Cell.Visited) == 0)
                {
                    candidates[candidateCount++] = i;
                }
            }

            if (candidateCount == 0)
            {
                path.Pop();
                continue;
            }

            var direction = Directions[candidates[UnityEngine.Random.Range(0, candidateCount)]];
            Vector2Int next = current + direction.offset;
            cells[current.x, current.y] |= direction.passage;
            cells[next.x, next.y] = direction.opposite | Cell.Visited;
            path.Push(next);
        }

        return cells;
    }

    private void DrawWalls(Cell[,] cells)
    {
        float halfCell = CellSize * 0.5f;

        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                Vector3 center = CellCenter(x, z);
                Cell cell = cells[x, z];

                // Each shared edge is drawn by just one of its cells.
                if ((cell & Cell.East) == 0)
                    PlaceWall(center + Vector3.right * halfCell, 0f, westWallMaterial, eastWallMaterial);
                if ((cell & Cell.North) == 0)
                    PlaceWall(center + Vector3.forward * halfCell, 90f, northWallMaterial, southWallMaterial);
                if (x == 0)
                    PlaceWall(center - Vector3.right * halfCell, 0f, westWallMaterial, eastWallMaterial);
                if (z == 0)
                    PlaceWall(center - Vector3.forward * halfCell, 90f, northWallMaterial, southWallMaterial);
            }
        }
    }

    private Vector3 CellCenter(int x, int z)
    {
        return new Vector3((x - (sizeX - 1) * 0.5f) * CellSize, 0f,
                           (z - (sizeZ - 1) * 0.5f) * CellSize);
    }

    private void PlaceWall(Vector3 position, float yaw, Material sideAMat, Material sideBMat)
    {
        position.y = wallYPos;
        Transform wall = Instantiate(wallPrefab, transform, false);
        wall.SetLocalPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
        
        wall.Find("SideA").GetComponent<MeshRenderer>().sharedMaterial = sideAMat;
        wall.Find("SideB").GetComponent<MeshRenderer>().sharedMaterial = sideBMat;
    }

    private static void PlaceMarker(Transform marker, Vector3 position)
    {
        position.y = marker.position.y;
        marker.position = position;
    }
}
