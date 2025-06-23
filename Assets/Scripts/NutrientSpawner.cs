using UnityEngine;
using UnityEngine.Tilemaps;

public class NutrientSpawner : MonoBehaviour
{
    public Tilemap nutrientTilemap;      // Reference to NutriGrid Tilemap
    public GameObject nutrientPrefab;    // Nutrient prefab to spawn (with collider and "Nutrients" tag)

    void Start()
    {
        BoundsInt bounds = nutrientTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (nutrientTilemap.HasTile(cell))
                {
                    Vector3 worldPos = nutrientTilemap.CellToWorld(cell) + new Vector3(0.5f, 0.5f, 0f);
                    Instantiate(nutrientPrefab, worldPos, Quaternion.identity);

                    // Optional: remove the tile after placing
                    // nutrientTilemap.SetTile(cell, null);
                }
            }
        }
    }
}
