using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public RectTransform container; // rect transform của canvas
    public GameObject linePointPrefab; // optional small image prefab to show line (or use UI.LineRenderer lib)
    private List<Vector3> points = new List<Vector3>();

    // For simplicity: we'll store screen points and draw by instantiating small images between points.
    private List<GameObject> visualPoints = new List<GameObject>();

    public void AddPoint(Vector3 worldPos)
    {
        // convert world -> canvas local
        Canvas canvas = container.GetComponentInParent<Canvas>();
        Vector2 anchored;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(container, RectTransformUtility.WorldToScreenPoint(null, worldPos), canvas.worldCamera, out anchored);

        points.Add(anchored);
        RefreshVisual();
    }

    public void Clear()
    {
        points.Clear();
        foreach (var g in visualPoints) if (g) Destroy(g);
        visualPoints.Clear();
    }

    void RefreshVisual()
    {
        // destroy old visuals (simple approach)
        foreach (var g in visualPoints) if (g) Destroy(g);
        visualPoints.Clear();

        for (int i = 0; i < points.Count; i++)
        {
            GameObject dot = new GameObject("dot", typeof(RectTransform));
            dot.transform.SetParent(container, false);
            var rt = dot.GetComponent<RectTransform>();
            rt.anchoredPosition = points[i];
            rt.sizeDelta = new Vector2(8, 8);
            var img = dot.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.yellow;
            visualPoints.Add(dot);
        }

        // Optionally draw lines between points by creating stretched images — omitted for brevity
    }
}