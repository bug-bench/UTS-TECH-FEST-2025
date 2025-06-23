using UnityEngine;
using UnityEngine.Tilemaps;

public class GridPlacer : MonoBehaviour
{
    public Tilemap mainTilemap;
    public Tilemap highlightTilemap;
    public TileBase placementTile;
    public TileBase highlightTile;

    private Vector3Int previousCellPos;

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = mainTilemap.WorldToCell(mouseWorldPos);

        // Highlight
        if (cellPos != previousCellPos)
        {
            highlightTilemap.ClearAllTiles();
            if (mainTilemap.HasTile(cellPos)) // Optional: Only highlight over existing tiles
            {
                highlightTilemap.SetTile(cellPos, highlightTile);
            }
            previousCellPos = cellPos;
        }

        // Place tile on left click
        if (Input.GetMouseButtonDown(0))
        {
            mainTilemap.SetTile(cellPos, placementTile);
        }
    }
}
