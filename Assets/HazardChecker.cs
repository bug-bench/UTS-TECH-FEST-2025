using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class HazardChecker : MonoBehaviour
{
    public Tilemap poisonTilemap;  // Assign the poison tilemap in inspector

    private HashSet<Vector3Int> poisonPositions = new HashSet<Vector3Int>();

    public static HazardChecker Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        CachePoisonPositions();
    }

    void CachePoisonPositions()
    {
        BoundsInt bounds = poisonTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (poisonTilemap.HasTile(pos))
            {
                poisonPositions.Add(pos);
            }
        }
    }

    public bool IsPoison(Vector3Int cellPos)
    {
        return poisonPositions.Contains(cellPos);
    }
}
