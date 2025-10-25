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

    // runtime flag to prevent duplicate usage
    private bool interactable = true;

    public void Setup(char c, GameManager gm)
    {
        letter = c;
        gameManager = gm;
        letterText.text = c.ToString();
        SetInteractable(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;
        pointerDown = true;
        WordBuilder.I.BeginPath(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;
        if (Input.GetMouseButton(0) || pointerDown) // hỗ trợ touch drag
        {
            WordBuilder.I.TryAdd(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable) return;
        pointerDown = false;
        WordBuilder.I.EndPath();
    }

    public void SetHighlighted(bool on)
    {
        if (bg != null) bg.color = on ? new Color(1f, 0.85f, 0.3f) : Color.black;
    }

    public void SetInteractable(bool ok)
    {
        interactable = ok;
        var btn = GetComponent<Button>();
        if (btn) btn.interactable = ok;
        // giảm alpha nếu disabled
        //if (bg != null) bg.color = ok ? Color.white : new Color(0.7f,0.7f,0.7f,0.6f);
    }
}