using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class WallChecker : MonoBehaviour
{
    public Tilemap wallTilemap;  // Assign WallGrid in the inspector

    private HashSet<Vector3Int> wallPositions = new HashSet<Vector3Int>();

    public static WallChecker Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        CacheWallPositions();
    }

    private void CacheWallPositions()
    {
        BoundsInt bounds = wallTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (wallTilemap.HasTile(pos))
            {
                wallPositions.Add(pos);
            }
        }
    }

    public bool IsWall(Vector3Int cellPos)
    {
        return wallPositions.Contains(cellPos);
    }
}
