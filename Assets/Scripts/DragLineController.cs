using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragLineController : MonoBehaviour
{
    public static DragLineController I;

    [Header("Line settings")]
    public Material lineMaterial;
    public float lineWidth = 6f;
    public int maxSegments = 2; // tối đa số đoạn line (segment)
    public Canvas targetCanvas;  // gán Canvas trong Inspector (bắt buộc)

    // runtime
    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<LetterButton> path = new List<LetterButton>();
    private bool isDragging = false;

    void Awake()
    {
        if (I == null) I = this; else Destroy(gameObject);
    }

    void Start()
    {
        if (targetCanvas == null)
        {
            var c = GetComponentInParent<Canvas>();
            if (c != null) targetCanvas = c;
        }
    }

    void Update()
    {
        if (!isDragging) return;
        if (lines.Count == 0) return;

        // cập nhật điểm cuối line hiện thời theo con trỏ, dùng world point chính xác trên canvas plane
        Vector3 pointerWorld = GetPointerWorldPositionOnCanvas();
        var lr = lines[lines.Count - 1];
        lr.positionCount = 2;
        Vector3 p0 = lr.GetPosition(0);
        // giữ z cùng plane với p0
        pointerWorld.z = p0.z;
        lr.SetPosition(1, pointerWorld);
    }

    // Convert screen -> world on canvas plane reliably
    Vector3 GetPointerWorldPositionOnCanvas()
    {
        Vector2 screenPos;
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0) screenPos = Input.GetTouch(0).position;
        else screenPos = Input.mousePosition;
#else
        screenPos = Input.mousePosition;
#endif
        RectTransform canvasRT = targetCanvas.GetComponent<RectTransform>();
        Vector3 worldPoint;
        // use the canvas' transform and camera (works for Overlay/Camera)
        Camera cam = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRT, screenPos, cam, out worldPoint);
        return worldPoint;
    }

    private LineRenderer CreateNewLine(Vector3 startWorld)
    {
        GameObject go = new GameObject("DragLine");
        go.transform.SetParent(transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.widthMultiplier = lineWidth * 0.01f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.numCapVertices = 8;
        lr.numCornerVertices = 8;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.textureMode = LineTextureMode.Stretch;

        // set positions (ensure z consistent)
        lr.SetPosition(0, startWorld);
        lr.SetPosition(1, startWorld);

        // ensure ordering: larger sortingOrder -> drawn on top
        // if material supports sorting (Sprites/Default) you can set sortingLayer/order
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 0; // tăng dần để mới luôn trên

        lines.Add(lr);
        return lr;
    }

    public void BeginDragFrom(LetterButton node)
    {
        if (node == null) return;
        ClearAll();
        isDragging = true;
        path.Add(node);
        Vector3 start = node.GetWorldPositionOnCanvas(targetCanvas);
        CreateNewLine(start);
    }

    public void OnEnterNode(LetterButton node)
    {
        if (!isDragging) return;
        if (node == null) return;
        if (path.Contains(node)) return;

        // finalize last segment endpoint
        Vector3 nodePos = node.GetWorldPositionOnCanvas(targetCanvas);
        if (lines.Count == 0)
        {
            CreateNewLine(path[0].GetWorldPositionOnCanvas(targetCanvas));
        }
        var last = lines[lines.Count - 1];
        last.SetPosition(1, nodePos);

        path.Add(node);

        if (lines.Count < maxSegments)
        {
            CreateNewLine(nodePos);
        }
    }

    public void EndDragAndClear(bool keepPath = false)
    {
        ClearAll();
        isDragging = false;
        path.Clear();
    }

    private void ClearAll()
    {
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            var l = lines[i];
            if (l) Destroy(l.gameObject);
        }
        lines.Clear();
    }

    public bool IsNodeInPath(LetterButton node)
    {
        return path.Contains(node);
    }

    public List<LetterButton> GetCurrentPath()
    {
        return new List<LetterButton>(path);
    }
}
