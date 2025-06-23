using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridPlacer : MonoBehaviour
{
    public Tilemap mainTilemap;
    public Tilemap highlightTilemap;
    public TileBase placementTile;
    public TileBase highlightTile;

    public Vector3Int spawnCell; // Set in inspector or Start()
    private HashSet<Vector3Int> validPositions = new HashSet<Vector3Int>();

    private Vector3Int previousCell;

    void Start()
    {
        // Place the spawn tile
        mainTilemap.SetTile(spawnCell, placementTile);
        AddValidNeighbors(spawnCell);
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = mainTilemap.WorldToCell(mouseWorldPos);

        // Highlight only valid spots
        if (cellPos != previousCell)
        {
            highlightTilemap.ClearAllTiles();
            if (validPositions.Contains(cellPos))
                highlightTilemap.SetTile(cellPos, highlightTile);

            previousCell = cellPos;
        }

        // Place tile if allowed
        if (Input.GetMouseButtonDown(0) && validPositions.Contains(cellPos))
        {
            mainTilemap.SetTile(cellPos, placementTile);
            validPositions.Remove(cellPos);
            AddValidNeighbors(cellPos);
        }
    }

    // Add neighboring cells if they're not already occupied
    void AddValidNeighbors(Vector3Int origin)
    {
        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighbor = origin + dir;

            if (!mainTilemap.HasTile(neighbor) && !validPositions.Contains(neighbor))
            {
                validPositions.Add(neighbor);
            }
        }
    }
}
