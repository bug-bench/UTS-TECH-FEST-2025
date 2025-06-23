using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PathManager : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Tilemap rootTilemap;
    public Tilemap pathTilemap;       // ← Assign RatPathGrid here
    public GridPlacer gridPlacer;
    public TileBase pathTile;         // ← The tile used to mark path

    private List<Vector3Int> ratPath = new();
    private int currentTargetIndex = 0;

    void Start()
    {
        GeneratePathFromTilemap();

        if (ratPath.Count > 0)
        {
            Vector3 startPos = rootTilemap.CellToWorld(ratPath[0]) + new Vector3(0.5f, 0.5f, 0f);
            transform.position = startPos;
            StartCoroutine(MoveAlongPath());
        }
    }

    void GeneratePathFromTilemap()
    {
        BoundsInt bounds = pathTilemap.cellBounds;
        List<Vector3Int> pathPoints = new();

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (pathTilemap.GetTile(pos) == pathTile)
                pathPoints.Add(pos);
        }

        // Optional: sort based on proximity (good for non-random paths)
        Vector3Int start = pathPoints[0];
        ratPath = SortByDistance(start, pathPoints);
    }

    List<Vector3Int> SortByDistance(Vector3Int start, List<Vector3Int> unsorted)
    {
        List<Vector3Int> sorted = new() { start };
        HashSet<Vector3Int> visited = new() { start };

        while (sorted.Count < unsorted.Count)
        {
            Vector3Int last = sorted.Last();
            Vector3Int next = unsorted
                .Where(p => !visited.Contains(p))
                .OrderBy(p => Vector3Int.Distance(p, last))
                .First();

            sorted.Add(next);
            visited.Add(next);
        }

        return sorted;
    }

    IEnumerator MoveAlongPath()
    {
        while (true)
        {
            Vector3 nextTarget = ratPath[(currentTargetIndex + 1) % ratPath.Count];
            Vector3 direction = (nextTarget - ratPath[currentTargetIndex]).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Vector3Int targetCell = ratPath[currentTargetIndex];
            Vector3 targetWorld = rootTilemap.CellToWorld(targetCell) + new Vector3(0.5f, 0.5f, 0f);

            while ((transform.position - targetWorld).sqrMagnitude > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);
                yield return null;
            }

            if (rootTilemap.HasTile(targetCell))
            {
                Debug.Log("Rat cut root at " + targetCell);
                gridPlacer.CutRootFrom(targetCell);
            }

            currentTargetIndex = (currentTargetIndex + 1) % ratPath.Count;
            yield return null;
        }
    }
}
