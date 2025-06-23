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

        // Update highlight
        if (cellPos != previousCellPos)
        {
            highlightTilemap.ClearAllTiles();
            highlightTilemap.SetTile(cellPos, highlightTile);
            previousCellPos = cellPos;
        }

        // Place tile with left click
        if (Input.GetMouseButtonDown(0))
        {
            mainTilemap.SetTile(cellPos, placementTile);
        }
    }
}
