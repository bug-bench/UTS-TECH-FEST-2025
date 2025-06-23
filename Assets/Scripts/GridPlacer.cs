using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridPlacer : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap mainTilemap;
    public Tilemap highlightTilemap;

    [Header("Tiles")]
    public TileBase placementTile;
    public TileBase validHighlightTile;
    public TileBase invalidHighlightTile;

    [Header("Settings")]
    public Vector3Int spawnCell = Vector3Int.zero;

    private HashSet<Vector3Int> validPositions = new HashSet<Vector3Int>();

    void Start()
    {
        // Place the central spawn tile
        mainTilemap.SetTile(spawnCell, placementTile);
        AddValidNeighbors(spawnCell);
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = mainTilemap.WorldToCell(mouseWorldPos);

        // Always show a highlight at the hovered cell
        highlightTilemap.ClearAllTiles();

        if (mainTilemap.HasTile(cellPos) || highlightTilemap == null)
        {
            // Don't highlight if a tile already exists
            return;
        }

        if (validPositions.Contains(cellPos))
        {
            highlightTilemap.SetTile(cellPos, validHighlightTile);
        }
        else
        {
            highlightTilemap.SetTile(cellPos, invalidHighlightTile);
        }

        // Place tile only if it's in a valid spot
        if (Input.GetMouseButtonDown(0) && validPositions.Contains(cellPos))
        {
            mainTilemap.SetTile(cellPos, placementTile);
            validPositions.Remove(cellPos);
            AddValidNeighbors(cellPos);
        }
    }

    void AddValidNeighbors(Vector3Int origin)
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
            Vector3Int neighbor = origin + dir;

            if (!mainTilemap.HasTile(neighbor) && !validPositions.Contains(neighbor))
            {
                validPositions.Add(neighbor);
            }
        }
    }
}
