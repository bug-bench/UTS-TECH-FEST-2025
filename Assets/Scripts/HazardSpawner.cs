using UnityEngine;
using UnityEngine.Tilemaps;

public class PoisonSpawner : MonoBehaviour
{
    public Tilemap hazardTile;       // Assign PoisonGrid tilemap here
    public GameObject hazardPrefab;     // Assign your prefab with "Poison" tag

    void Start()
    {
        BoundsInt bounds = hazardTile.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (hazardTile.HasTile(cell))
                {
                    Vector3 worldPos = hazardTile.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
                    Instantiate(hazardTile, worldPos, Quaternion.identity);

                    // Optional: Clear tile if you're replacing visuals
                    hazardTile.SetTile(cell, null);
                }
            }
        }
    }
}
