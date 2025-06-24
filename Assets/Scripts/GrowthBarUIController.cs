using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GrowthBarUIController : MonoBehaviour
{
    public GameObject segmentPrefab;       // straight sprout
    public GameObject endSegmentPrefab;    // end sprout
    public Transform barContainer;

    private List<GameObject> currentSegments = new List<GameObject>();

    public void SetGrowthBar(int max, int current)
    {
        foreach (var seg in currentSegments)
            Destroy(seg);
        currentSegments.Clear();

        for (int i = 0; i < max; i++)
        {
            GameObject prefab = (i == max - 1) ? endSegmentPrefab : segmentPrefab;
            GameObject seg = Instantiate(prefab, barContainer);
            seg.SetActive(i >= current); // hide segments used
            currentSegments.Add(seg);
        }
    }
}
