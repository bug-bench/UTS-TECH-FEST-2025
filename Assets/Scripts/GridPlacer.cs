using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class GridPlacer : MonoBehaviour
{
    private List<Vector3Int> placedRootOrder = new List<Vector3Int>();

    [Header("Tilemap References")]
    public Tilemap mainTilemap;
    public Tilemap highlightTilemap;

    [Header("Tiles")]
    public TileBase potatoTile;
    public TileBase placementTile;
    public TileBase validHighlightTile;
    public TileBase invalidHighlightTile;

    [Header("Root Sprites")]
    public TileBase rootEnd;
    public TileBase rootStraight;
    public TileBase rootCorner;
    public TileBase rootT;
    public TileBase rootCross;

    [Header("UI Counters")]
    public TextMeshProUGUI totalRootText;
    public GrowthBarUIController growthBarUI;


    [Header("Root Growth Settings")]
    public Vector3Int spawnCell = Vector3Int.zero;
    public int defaultGrowthLimit = 6;
    public int rootGrowthLimit;
    private List<int> growthMilestones = new List<int> { 10, 20, 30, 45, 60, 75 };
    private int nextMilestoneIndex = 0;
    public MilestonePopupUI milestonePopup;

    [Header("Checkpoint")]
    public Transform playerTransform;
    public CameraController cameraController;
    public Vector3 spawnPoint = Vector3.zero;
    private Vector3 lastCheckpointPosition;
    private bool hasCollectedCheckpoint = false;
    private Dictionary<Vector3Int, (TileBase tile, Matrix4x4 matrix)> checkpointTiles = new Dictionary<Vector3Int, (TileBase, Matrix4x4)>();
    private int checkpointRootTileCount = 1;
    private int checkpointGrowthLimit = 6;

    [Header("Fog of War")]
    public Tilemap fogTilemap;
    public TileBase fogTile;
    public Vector2Int fogBoundsMin = new Vector2Int(-100, -100);
    public Vector2Int fogBoundsMax = new Vector2Int(100, 100);


    [Header("External Systems")]
    public NutrientChecker nutrientChecker;

    private int totalRootTiles = 1;
    private int currentGrowthCount = 0;
    private HashSet<Vector3Int> validPositions = new HashSet<Vector3Int>();
    private Vector3Int lastPlacedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    void Start()
    {
        if (TutorialManager.IsTutorialActive)
            return;

        rootGrowthLimit = defaultGrowthLimit;
        // mainTilemap.SetTile(spawnCell, potatoTile);
        // mainTilemap.SetTransformMatrix(spawnCell, Matrix4x4.identity);
        // AddValidNeighbors(spawnCell);
        // UpdateUI();
        // lastCheckpointPosition = spawnPoint;

        // InitializeFog();
        // RevealFog(spawnCell);
    }

    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = mainTilemap.WorldToCell(mouseWorldPos);

        highlightTilemap.ClearAllTiles();

        if (!mainTilemap.HasTile(cellPos) && !WallChecker.Instance.IsWall(cellPos))
        {
            int neighborCount = CountPlacedNeighbors(cellPos);
            if (validPositions.Contains(cellPos) && neighborCount == 1 && currentGrowthCount < rootGrowthLimit)
                highlightTilemap.SetTile(cellPos, validHighlightTile);
            else
                highlightTilemap.SetTile(cellPos, invalidHighlightTile);
        }

        if (Input.GetMouseButton(0))
        {
            if (cellPos != lastPlacedCell && validPositions.Contains(cellPos) && !WallChecker.Instance.IsWall(cellPos))
            {
                int neighborCount = CountPlacedNeighbors(cellPos);
                if (neighborCount == 1 && currentGrowthCount < rootGrowthLimit)
                {
                    PlaceAndRefresh(cellPos);
                    RevealFog(cellPos);
                    validPositions.Remove(cellPos);
                    AddValidNeighbors(cellPos);
                    lastPlacedCell = cellPos;
                    totalRootTiles++;
                    currentGrowthCount++;
                    CheckGrowthProgression();
                    placedRootOrder.Add(cellPos); // track the root
                    UpdateUI();
                    nutrientChecker.CheckForNutrients(cellPos);
                }
                if (HazardChecker.Instance.IsPoison(cellPos))
                {
                    StartCoroutine(DestroyPoisonedRoot(cellPos));
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            mainTilemap.ClearAllTiles();

            if (hasCollectedCheckpoint)
            {
                mainTilemap.ClearAllTiles();
                totalRootTiles = 0;

                foreach (var pair in checkpointTiles)
                {
                    mainTilemap.SetTile(pair.Key, pair.Value.tile);
                    mainTilemap.SetTransformMatrix(pair.Key, pair.Value.matrix);
                    totalRootTiles++;
                }

                currentGrowthCount = 0;
                rootGrowthLimit = checkpointGrowthLimit;
                cameraController?.SnapToPosition(lastCheckpointPosition);

                validPositions.Clear();
                foreach (var pair in checkpointTiles)
                {
                    AddValidNeighbors(pair.Key);
                }
            }
            else
            {
                mainTilemap.ClearAllTiles();
                totalRootTiles = 1;

                mainTilemap.SetTile(spawnCell, potatoTile);
                mainTilemap.SetTransformMatrix(spawnCell, Matrix4x4.identity);

                cameraController?.SnapToPosition(spawnPoint);

                validPositions.Clear();
                AddValidNeighbors(spawnCell);

                currentGrowthCount = 0;
                rootGrowthLimit = defaultGrowthLimit;
            }

            UpdateUI();
        }
    }

    public void InitializeSpawnTile()
    {
        mainTilemap.SetTile(spawnCell, potatoTile);
        mainTilemap.SetTransformMatrix(spawnCell, Matrix4x4.identity);
        AddValidNeighbors(spawnCell);

        UpdateUI();
        lastCheckpointPosition = spawnPoint;

        InitializeFog();
        RevealFog(spawnCell);
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

    void PlaceAndRefresh(Vector3Int pos)
    {
        RefreshTile(pos);

        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
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

    void UpdateUI()
    {
        if (growthBarUI != null)
            growthBarUI.SetGrowthBar(rootGrowthLimit, currentGrowthCount);

        if (totalRootText != null)
            totalRootText.text = "Roots grown: " + totalRootTiles;

        // if (rootLimitText != null)
        //     rootLimitText.text = $"Growth Used: {currentGrowthCount} / {rootGrowthLimit}";
    }

    public void ResetRoots()
    {
        currentGrowthCount = 0;
        UpdateUI();
    }

    public void SetCheckpoint(Vector3 worldPosition)
    {
        hasCollectedCheckpoint = true;
        lastCheckpointPosition = worldPosition;

        checkpointTiles.Clear();
        BoundsInt bounds = mainTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (mainTilemap.HasTile(pos))
            {
                TileBase tile = mainTilemap.GetTile(pos);
                Matrix4x4 matrix = mainTilemap.GetTransformMatrix(pos);
                checkpointTiles[pos] = (tile, matrix);
            }
        }

        checkpointRootTileCount = totalRootTiles;
        checkpointGrowthLimit = rootGrowthLimit;
    }

    public void UpdateGrowthLimit(int newLimit)
    {
        rootGrowthLimit = newLimit;
        checkpointGrowthLimit = newLimit;
        UpdateUI();
    }

    private IEnumerator DestroyPoisonedRoot(Vector3Int poisonedCell)
    {
        yield return new WaitForSeconds(1.5f);

        // ✅ Consume poison hazard
        HazardChecker.Instance.RemovePoison(poisonedCell);

        // Destroy poisoned root tile
        if (mainTilemap.HasTile(poisonedCell))
        {
            mainTilemap.SetTile(poisonedCell, null);
            totalRootTiles--;
            placedRootOrder.Remove(poisonedCell);
            validPositions.Add(poisonedCell);
            AddValidNeighbors(poisonedCell);
        }

        // Destroy the previous root tile (if it exists)
        if (placedRootOrder.Count > 0)
        {
            Vector3Int last = placedRootOrder[^1];
            if (mainTilemap.HasTile(last))
            {
                mainTilemap.SetTile(last, null);
                totalRootTiles--;
                placedRootOrder.RemoveAt(placedRootOrder.Count - 1);
                validPositions.Add(last);
                AddValidNeighbors(last);
            }
        }

        UpdateUI();
    }

    public void CutRootFrom(Vector3Int cutPoint)
    {
        int index = placedRootOrder.IndexOf(cutPoint);
        if (index != -1)
        {
            // Delete cut point and all after it
            for (int i = placedRootOrder.Count - 1; i >= index; i--)
            {
                Vector3Int cell = placedRootOrder[i];
                mainTilemap.SetTile(cell, null);
                totalRootTiles--;
                validPositions.Add(cell);
                AddValidNeighbors(cell);
                placedRootOrder.RemoveAt(i);
            }
            UpdateUI();
        }
    }
    void InitializeFog()
    {
        for (int x = fogBoundsMin.x; x <= fogBoundsMax.x; x++)
        {
            for (int y = fogBoundsMin.y; y <= fogBoundsMax.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                fogTilemap.SetTile(pos, fogTile);
            }
        }
    }

    void RevealFog(Vector3Int center)
    {
        int radius = 3; // You can change this to 4 or 5 if needed
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int tilePos = center + new Vector3Int(x, y, 0);
                if (fogTilemap.HasTile(tilePos))
                {
                    fogTilemap.SetTile(tilePos, null);
                }
            }
        }
    }

    void CheckGrowthProgression()
    {
        if (nextMilestoneIndex < growthMilestones.Count &&
            totalRootTiles >= growthMilestones[nextMilestoneIndex])
        {
            rootGrowthLimit++;
            checkpointGrowthLimit = rootGrowthLimit;
            nextMilestoneIndex++;

            if (milestonePopup != null)
                milestonePopup.ShowPopup("+1 Growth Limit!");

            UpdateUI(); // 🔁 Important!
            Debug.Log("Milestone reached! Growth Limit increased to " + rootGrowthLimit);
        }
    }
}