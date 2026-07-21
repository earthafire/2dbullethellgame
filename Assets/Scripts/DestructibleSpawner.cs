using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Scatters destructible barrels/crates across the map after each generation.
// Hooks LevelGenerator.OnGenerated (fired once tiles are rendered onto the
// tilemap) rather than the ring-around-the-player spawn gameSupervisorController
// uses for enemies, since these should be spread across the whole level up front.
public class DestructibleSpawner : MonoBehaviour
{
    [Tooltip("Barrel/crate prefabs to scatter across the map - one is picked at random per spawn")]
    public GameObject[] destructiblePrefabs;

    [SerializeField] private int minCount = 6;
    [SerializeField] private int maxCount = 14;

    // How many random cells to try before giving up on placing a given destructible.
    [SerializeField] private int maxPlacementAttempts = 60;

    private readonly List<GameObject> _spawned = new();
    private readonly HashSet<Vector3Int> _usedCells = new();

    private static readonly Vector3Int[] Neighbors =
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
    };

    private void Start()
    {
        GlobalReferences.levelGenerator.OnGenerated.AddListener(SpawnDestructibles);

        // OnGenerated may already have fired before this listener was attached
        // (Start() order across scripts isn't guaranteed) - this catches that case.
        // If the map regenerates again afterwards, the listener above re-runs this
        // and clears/replaces this best-effort batch anyway.
        SpawnDestructibles();
    }

    private void SpawnDestructibles()
    {
        ClearSpawned();

        if (destructiblePrefabs == null || destructiblePrefabs.Length == 0)
        {
            return;
        }

        LevelGenerator levelGenerator = GlobalReferences.levelGenerator;
        Tilemap tilemap = levelGenerator.tilemap;
        int count = GlobalReferences.GetRandomNumber(minCount, maxCount + 1);

        for (int i = 0; i < count; i++)
        {
            if (!TryFindWallAdjacentFloorCell(tilemap, levelGenerator.width, levelGenerator.height, out Vector3Int cell))
            {
                continue;
            }

            _usedCells.Add(cell);

            // Cell center in world space - accounts for the grid's actual cell
            // size/offset rather than assuming 1 world unit per cell (the map's
            // Grid uses a 0.24 cell size, so raw array indices are NOT world units).
            Vector3 position = tilemap.GetCellCenterWorld(cell);

            GameObject prefab = destructiblePrefabs[GlobalReferences.GetRandomNumber(0, destructiblePrefabs.Length)];
            GameObject spawned = ObjectPoolManager.SpawnObject(prefab, position, Quaternion.identity);
            _spawned.Add(spawned);
        }
    }

    private bool TryFindWallAdjacentFloorCell(Tilemap tilemap, int width, int height, out Vector3Int cell)
    {
        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            int x = GlobalReferences.GetRandomNumber(1, width - 1);
            int y = GlobalReferences.GetRandomNumber(1, height - 1);
            Vector3Int candidate = new Vector3Int(x, y, 0);

            if (_usedCells.Contains(candidate))
            {
                continue;
            }

            if (IsWallAdjacentFloor(tilemap, candidate))
            {
                cell = candidate;
                return true;
            }
        }

        cell = default;
        return false;
    }

    // Walls are the only thing ever rendered onto this tilemap (see
    // MapFunctions.RenderMap - a tile means map[x,y] == 1, i.e. wall), so an empty
    // cell is floor by definition; no tile means floor.
    private bool IsWallAdjacentFloor(Tilemap tilemap, Vector3Int cell)
    {
        if (tilemap.GetTile(cell) != null)
        {
            return false;
        }

        foreach (var offset in Neighbors)
        {
            if (tilemap.GetTile(cell + offset) != null)
            {
                return true;
            }
        }
        return false;
    }

    private void ClearSpawned()
    {
        foreach (var obj in _spawned)
        {
            if (obj != null)
            {
                ObjectPoolManager.ReturnObjectToPool(obj);
            }
        }
        _spawned.Clear();
        _usedCells.Clear();
    }
}
