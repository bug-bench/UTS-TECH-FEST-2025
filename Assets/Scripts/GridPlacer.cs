using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using System.Collections.Generic;

public class GridPlacer : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap mainTilemap;         // The tilemap where root tiles are placed
    public Tilemap highlightTilemap;    // The tilemap used for showing highlight feedback

    [Header("Tiles")]
    public TileBase potatoTile;
    public TileBase placementTile;         // The tile placed by the player (root)
    public TileBase validHighlightTile;    // Highlight tile shown when placement is valid
    public TileBase invalidHighlightTile;  // Highlight tile shown when placement is invalid

    [Header("Root Sprites")]
    public TileBase rootEnd;
    public TileBase rootStraight;
    public TileBase rootCorner;
    public TileBase rootT;
    public TileBase rootCross;


    [Header("UI Counters")]
    public TextMeshProUGUI totalRootText;     // Displays total number of placed tiles
    public TextMeshProUGUI rootLimitText;     // Displays max number of allowed placements

    [Header("Placement Settings")]
    public Vector3Int spawnCell = Vector3Int.zero;  // Starting point of the root system
    public int rootGrowthLimit = 4;                 // Max number of tiles player can grow

    // Internal tracking variables
    private int totalRootTiles = 1; // Starts at 1 because we place the spawn tile immediately
    private HashSet<Vector3Int> validPositions = new HashSet<Vector3Int>(); // Positions eligible for placement
    private Vector3Int lastPlacedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue); // Tracks last placed tile to avoid drag-spam

    void Start()
    {
        // Place the initial root tile and add its valid neighbors
        mainTilemap.SetTile(spawnCell, potatoTile);
        mainTilemap.SetTransformMatrix(spawnCell, Matrix4x4.identity);

        AddValidNeighbors(spawnCell);
        UpdateUI();
    }

    void Update()
    {
        // Convert mouse position to grid cell
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = mainTilemap.WorldToCell(mouseWorldPos);

        // Always clear highlight tiles before updating
        highlightTilemap.ClearAllTiles();

        // Only show highlight if the cell doesn't already have a tile
        if (!mainTilemap.HasTile(cellPos))
        {
            int neighborCount = CountPlacedNeighbors(cellPos);

            // Show valid highlight if position is allowed and only connects to one tile (Scrabble rule)
            if (validPositions.Contains(cellPos) && neighborCount == 1 && totalRootTiles < rootGrowthLimit + 1)
                highlightTilemap.SetTile(cellPos, validHighlightTile);
            else
                highlightTilemap.SetTile(cellPos, invalidHighlightTile);
        }

        // Handle drag placement when mouse is held down
        if (Input.GetMouseButton(0))
        {
            // Only place if cell is different from last and is valid
            if (cellPos != lastPlacedCell && validPositions.Contains(cellPos))
            {
                int neighborCount = CountPlacedNeighbors(cellPos);

                // Allow placement only if one neighbor and still under limit
                if (neighborCount == 1 && totalRootTiles < rootGrowthLimit + 1)
                {
                    PlaceAndRefresh(cellPos);
                    validPositions.Remove(cellPos);
                    AddValidNeighbors(cellPos);
                    lastPlacedCell = cellPos;
                    totalRootTiles++;
                    UpdateUI();
                }
            }
        }

        // Reset drag placement when mouse is released
        if (Input.GetMouseButtonUp(0))
        {
            lastPlacedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        }
    }

    // Adds all valid adjacent cells to a given tile (up/down/left/right)
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

            // Only add neighbors that aren't already occupied or already marked valid
            if (!mainTilemap.HasTile(neighbor) && !validPositions.Contains(neighbor))
            {
                validPositions.Add(neighbor);
            }
        }
    }

    // Counts how many neighboring tiles are already placed around a given cell
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

    void PlaceAndRefresh(Vector3Int pos)
    {
        RefreshTile(pos);

        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up, // left
            Vector3Int.down, // right
            Vector3Int.left, // down
            Vector3Int.right // up
        };

        foreach (var dir in directions)
        {
            Vector3Int neighbor = pos + dir;
            if (mainTilemap.HasTile(neighbor) && neighbor != spawnCell)
            {
                RefreshTile(neighbor);
            }
        }
    }

    void RefreshTile(Vector3Int pos)
    {
        bool up = mainTilemap.HasTile(pos + Vector3Int.up);
        bool down = mainTilemap.HasTile(pos + Vector3Int.down);
        bool left = mainTilemap.HasTile(pos + Vector3Int.left);
        bool right = mainTilemap.HasTile(pos + Vector3Int.right);

        int connections = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

        TileBase newTile = null;
        float rotZ = 0f;

        if (connections == 1)
        {
            newTile = rootEnd;
            if (up) rotZ = 0;
            else if (right) rotZ = 270;
            else if (down) rotZ = 180;
            else if (left) rotZ = 90;
        }
        else if (connections == 2)
        {
            if ((up && down) || (left && right))
            {
                newTile = rootStraight;
                rotZ = (up && down) ? 0 : 90;
            }
            else
            {
                newTile = rootCorner;
                if (right && down) rotZ = 0f;          
                else if (down && left) rotZ = 270f;
                else if (left && up) rotZ = 180f;
                else if (up && right) rotZ = 90f;
            }
        }
        else if (connections == 3)
        {
            newTile = rootT;
            if (!left) rotZ = 0f;             
            else if (!up) rotZ = 270f;
            else if (!right) rotZ = 180f;
            else if (!down) rotZ = 90f;
        }
        else if (connections == 4)
        {
            newTile = rootCross;
            rotZ = 0;
        }

        if (newTile != null)
        {
            mainTilemap.SetTile(pos, newTile);
            mainTilemap.SetTransformMatrix(pos, Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotZ)));
        }
    }

    // Updates the UI counters for tile placement
    void UpdateUI()
    {
        if (totalRootText != null)
            totalRootText.text = "Total Root Tiles: " + totalRootTiles;

        if (rootLimitText != null)
            rootLimitText.text = "Root Growth Limit: " + rootGrowthLimit;
    }
}