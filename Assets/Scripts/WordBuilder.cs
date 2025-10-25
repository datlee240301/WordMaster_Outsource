using System.Collections.Generic;
using UnityEngine;

public class WordBuilder : MonoBehaviour
{
    public static WordBuilder I;
    private List<LetterButton> path = new List<LetterButton>();
    private string current = "";

    void Awake() { I = this; }

    public void BeginPath(LetterButton first)
    {
        path.Clear();
        current = "";
        AddInternal(first);
    }

    public void TryAdd(LetterButton btn)
    {
        if (path.Contains(btn)) return; // tránh thêm lại
        AddInternal(btn);
    }

    void AddInternal(LetterButton btn)
    {
        path.Add(btn);
        current += btn.letter;
        btn.SetHighlighted(true);
        // show text & line
        GameManager.I.ShowCurrentBuiltWord(current);
        GameManager.I.AddLinePoint(btn.GetComponent<RectTransform>().TransformPoint(btn.GetComponent<RectTransform>().rect.center));
    }

    public void EndPath()
    {
        // submit
        GameManager.I.OnSubmitWord(current, new List<LetterButton>(path));
        // reset visuals
        foreach (var b in path) b.SetHighlighted(false);
        path.Clear();
        current = "";
        GameManager.I.ShowCurrentBuiltWord("");
        GameManager.I.ClearLine();
    }
    
}