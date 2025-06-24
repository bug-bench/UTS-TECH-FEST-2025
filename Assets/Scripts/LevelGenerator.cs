using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    public Tilemap backgroundGrid;
    public Tilemap nutriGrid;
    public Tilemap wallGrid;
    public Tilemap soilGrid;
    public TileBase backgroundTile;
    public TileBase nutrientTile;
    public TileBase poisonTile;
    public TileBase wallTile;
    public TileBase soilTile;

    public int width = 40;
    public int height = 40;
    public int minPathLength = 30;

    private HashSet<Vector3Int> usedPositions = new HashSet<Vector3Int>();
    private Vector3Int currentPos = Vector3Int.zero;
    private Vector3Int soilGoalPosition;

    public GridPlacer gridPlacer; // Reference to GridPlacer

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        // Fill background
        for (int x = -width / 2; x < width / 2; x++)
        {
            for (int y = -height / 2; y < height / 2; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                backgroundGrid.SetTile(pos, backgroundTile);
            }
        }

        currentPos = Vector3Int.zero;
        usedPositions.Clear();
        usedPositions.Add(currentPos);

        List<Vector3Int> path = new List<Vector3Int> { currentPos };
        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        // Build random path
        for (int i = 0; i < minPathLength; i++)
        {
            List<Vector3Int> shuffled = new List<Vector3Int>(directions);
            Shuffle(shuffled);

            foreach (var dir in shuffled)
            {
                Vector3Int next = currentPos + dir;
                if (!usedPositions.Contains(next) && Mathf.Abs(next.x) < width / 2 && Mathf.Abs(next.y) < height / 2)
                {
                    currentPos = next;
                    usedPositions.Add(currentPos);
                    path.Add(currentPos);
                    break;
                }
            }
        }

        // Place nutrients and hazards
        int guaranteedNutrients = 3;
        int placed = 0;

        foreach (var pos in path)
        {
            if (Vector3Int.Distance(pos, Vector3Int.zero) <= 5f && placed < guaranteedNutrients)
            {
                nutriGrid.SetTile(pos, nutrientTile);
                placed++;
            }
        }

        // Additional random nutrients and hazards
        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3Int pos = path[i];

            // Skip if already has nutrient
            if (nutriGrid.HasTile(pos)) continue;

            if (Random.value < 0.1f)
            {
                nutriGrid.SetTile(pos, nutrientTile);
            }
            else if (Random.value < 0.1f)
            {
                wallGrid.SetTile(pos, poisonTile);
            }
}

        // Place soil goal tile at end
        soilGoalPosition = path[^1];
        soilGrid.SetTile(soilGoalPosition, soilTile);

        // Generate random walls
        for (int x = -width / 2; x < width / 2; x++)
        {
            for (int y = -height / 2; y < height / 2; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (!usedPositions.Contains(pos) && Random.value < 0.08f)
                {
                    wallGrid.SetTile(pos, wallTile);
                }
            }
        }
    }

    void Shuffle(List<Vector3Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector3Int temp = list[i];
            int randIndex = Random.Range(i, list.Count);
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }
}
