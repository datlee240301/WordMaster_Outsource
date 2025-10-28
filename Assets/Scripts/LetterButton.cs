using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LetterButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    public char letter;
    public Text letterText;
    public Image bg;
    private GameManager gameManager;
    private bool pointerDown = false;
    private bool interactable = true;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void Setup(char c, GameManager gm)
    {
        letter = c;
        gameManager = gm;
        if (letterText != null) letterText.text = c.ToString();
        SetInteractable(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;
        pointerDown = true;
        WordBuilder.I.BeginPath(this);

        // Start drag-line visual
        if (DragLineController.I != null) DragLineController.I.BeginDragFrom(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;
        // only act if dragging (mouse button held or pointerDown true)
        bool dragging = Input.GetMouseButton(0) || pointerDown;
        if (dragging)
        {
            WordBuilder.I.TryAdd(this);

            // visual: enter node
            if (DragLineController.I != null)
                DragLineController.I.OnEnterNode(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;
        pointerDown = false;
        WordBuilder.I.EndPath();

        // end drag: clear lines
        if (DragLineController.I != null)
            DragLineController.I.EndDragAndClear();
    }

    public void SetHighlighted(bool on)
    {
        if (bg != null) bg.color = on ? Color.white : new Color(0f, 0f, 0f, 0f);
    }

    // trong LetterButton.cs
    public Vector3 GetWorldPositionOnCanvas(Canvas canvas)
    {
        RectTransform rt = transform as RectTransform;
        Vector3 world;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(),
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.TransformPoint(rt.rect.center)),
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out world);
        return world;
    }


    public void SetInteractable(bool ok)
    {
        interactable = ok;
        var btn = GetComponent<Button>();
        // if (btn) btn.interactable = ok;
        // if (bg != null) bg.color = ok ? Color.white : new Color(0.7f,0.7f,0.7f,0.6f);
    }
}
