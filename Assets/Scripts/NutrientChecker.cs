using UnityEngine;
using UnityEngine.Tilemaps;

public class NutrientChecker : MonoBehaviour
{
    public Tilemap tilemap;
    public GridPlacer gridPlacer;

    public void CheckForNutrients(Vector3Int cellPos)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int checkCell = cellPos + dir;
            Vector3 worldPos = tilemap.CellToWorld(checkCell) + new Vector3(0.5f, 0.5f, 0);

            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.CompareTag("Nutrients"))
            {
                Debug.Log("Nutrient found at " + worldPos);
                gridPlacer.ResetRoots();

                // Optional: disable or destroy the nutrient
                Destroy(hit.gameObject);
                return;
            }
        }
    }
}