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

        // Always show highlight at mouse position
        highlightTilemap.ClearAllTiles();

        if (!mainTilemap.HasTile(cellPos))
        {
            int neighborCount = CountPlacedNeighbors(cellPos);

            if (validPositions.Contains(cellPos) && neighborCount == 1)
            {
                highlightTilemap.SetTile(cellPos, validHighlightTile);
            }
            else
            {
                highlightTilemap.SetTile(cellPos, invalidHighlightTile);
            }
        }

        // Place tile only if it's in a valid position and has exactly one neighbor
        if (Input.GetMouseButtonDown(0) && validPositions.Contains(cellPos))
        {
            int neighborCount = CountPlacedNeighbors(cellPos);

            if (neighborCount == 1)
            {
                mainTilemap.SetTile(cellPos, placementTile);
                validPositions.Remove(cellPos);
                AddValidNeighbors(cellPos);
            }
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

    int CountPlacedNeighbors(Vector3Int cellPos)
    {
        int count = 0;
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighbor = cellPos + dir;
            if (mainTilemap.HasTile(neighbor))
            {
                count++;
            }
        }

        return count;
    }
}
