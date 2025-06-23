using UnityEngine;
using UnityEngine.Tilemaps;
// using UnityEngine.UI;
using TMPro;
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

    [Header("UI Counters")]
    public TextMeshProUGUI  totalRootText;      // UI Text reference (or TextMeshProUGUI)
    public TextMeshProUGUI  rootLimitText;

    [Header("Placement Settings")]
    public Vector3Int spawnCell = Vector3Int.zero;
    public int rootGrowthLimit = 4; // How many tiles the player can place

    private int totalRootTiles = 1; // Starts with 1 (spawn tile)
    private HashSet<Vector3Int> validPositions = new HashSet<Vector3Int>();
    private Vector3Int lastPlacedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    void Start()
    {
        mainTilemap.SetTile(spawnCell, placementTile);
        AddValidNeighbors(spawnCell);
        UpdateUI();
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = mainTilemap.WorldToCell(mouseWorldPos);

        highlightTilemap.ClearAllTiles();

        if (!mainTilemap.HasTile(cellPos))
        {
            int neighborCount = CountPlacedNeighbors(cellPos);

            if (validPositions.Contains(cellPos) && neighborCount == 1 && totalRootTiles < rootGrowthLimit + 1)
                highlightTilemap.SetTile(cellPos, validHighlightTile);
            else
                highlightTilemap.SetTile(cellPos, invalidHighlightTile);
        }

        // Handle drag placement
        if (Input.GetMouseButton(0))
        {
            if (cellPos != lastPlacedCell && validPositions.Contains(cellPos))
            {
                int neighborCount = CountPlacedNeighbors(cellPos);

                if (neighborCount == 1 && totalRootTiles < rootGrowthLimit + 1)
                {
                    mainTilemap.SetTile(cellPos, placementTile);
                    validPositions.Remove(cellPos);
                    AddValidNeighbors(cellPos);
                    lastPlacedCell = cellPos;
                    totalRootTiles++;
                    UpdateUI();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            lastPlacedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
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

    void UpdateUI()
    {
        if (totalRootText != null)
            totalRootText.text = "Total Root Tiles: " + totalRootTiles;

        if (rootLimitText != null)
            rootLimitText.text = "Root Growth Limit: " + rootGrowthLimit;
    }
}
