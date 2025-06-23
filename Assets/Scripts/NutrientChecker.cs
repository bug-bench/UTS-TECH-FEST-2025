using UnityEngine;
using UnityEngine.Tilemaps;

public class NutrientChecker : MonoBehaviour
{
    public Tilemap tilemap;           // This should be your placement tilemap (mainTilemap)
    public GridPlacer gridPlacer;     // Link to GridPlacer to reset root counter

    public void CheckForNutrients(Vector3Int placedCell)
    {
        // Convert cell to center-world position
        Vector3 worldPos = tilemap.CellToWorld(placedCell) + new Vector3(0.5f, 0.5f, 0f);

        // Check for nutrient object at this tile's center
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null && hit.CompareTag("Nutrients"))
        {
            Debug.Log("Nutrient collected directly at " + placedCell);
            gridPlacer.ResetRoots();

            // Optional: remove the nutrient object so it can’t be reused
            Destroy(hit.gameObject);
        }
    }
}
