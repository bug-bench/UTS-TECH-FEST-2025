using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class SoilGoalChecker : MonoBehaviour
{
    public static SoilGoalChecker Instance { get; private set; }
    public Tilemap soilTilemap;

    void Awake() { Instance = this; }

    public bool IsGoal(Vector3Int cell) => soilTilemap.HasTile(cell);
}
