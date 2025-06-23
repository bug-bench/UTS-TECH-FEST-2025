using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class HazardChecker : MonoBehaviour
{
    public Tilemap poisonTilemap;  // Drag PoisonGrid here

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
    }

    void Start()
    {
        CachePoisonPositions();
    }

    void CachePoisonPositions()
    {
        poisonPositions.Clear(); // Clear any old data in case this runs again
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

    public void RemovePoison(Vector3Int cellPos)
    {
        poisonPositions.Remove(cellPos);
        poisonTilemap.SetTile(cellPos, null); // This is safe even if already removed
    }
}
