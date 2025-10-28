using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordRowUI : MonoBehaviour
{
    public string word { get; private set; }
    public GameObject letterBoxPrefab;

    private List<TMP_Text> boxTexts = new List<TMP_Text>();
    private List<RectTransform> boxRects = new List<RectTransform>();
    private bool[] isRevealed;   // đã reveal (bằng hint hoặc người chơi)
    private bool[] isHinted;     // reveal do hint (để có thể revert)
    private int revealedCount = 0;
    public string Definition { get; private set; }

    public void Setup(string targetWord, string definition)
    {
        word = targetWord.ToUpper();
        Definition = definition;
        // clear
        foreach (Transform t in transform) Destroy(t.gameObject);
        boxTexts.Clear();
        boxRects.Clear();

        int n = word.Length;
        isRevealed = new bool[n];
        isHinted = new bool[n];
        revealedCount = 0;

        for (int i = 0; i < n; i++)
        {
            GameObject b = Instantiate(letterBoxPrefab, transform);
            b.name = "Box_" + i;
            var txt = b.GetComponentInChildren<TMP_Text>();
            txt.text = "";
            boxTexts.Add(txt);
            boxRects.Add(b.GetComponent<RectTransform>());
            isRevealed[i] = false;
            isHinted[i] = false;
        }
    }

    // trả về danh sách index các ô trống (left -> right)
    public List<int> GetEmptySlotIndices()
    {
        var list = new List<int>();
        for (int i = 0; i < boxTexts.Count; i++)
            if (!isRevealed[i])
                list.Add(i);
        return list;
    }

    public RectTransform GetSlotRectAtIndex(int index)
    {
        if (index < 0 || index >= boxRects.Count) return null;
        return boxRects[index];
    }

    // fill ô tại index, isHint = true nếu là do hint
    public void FillSlotAtIndex(int index, char c, bool isHint = false)
    {
        if (index < 0 || index >= boxTexts.Count) return;
        if (!isRevealed[index])
        {
            revealedCount++;
            isRevealed[index] = true;
            isHinted[index] = isHint;
        }
        boxTexts[index].text = c.ToString();

        // nếu đầy rồi, clear any definition handled externally
    }

    // reveal 1 ký tự do hint; trả về index vừa reveal, hoặc -1 nếu không còn
    public int RevealOneLetterHint()
    {
        var empties = GetEmptySlotIndices();
        if (empties.Count == 0) return -1;
        int idx = empties[0]; // reveal left->right
        FillSlotAtIndex(idx, word[idx], true);
        return idx;
    }

    public bool IsFullyRevealed()
    {
        return revealedCount >= word.Length;
    }

    // trả về chuỗi ký tự cần điền (theo thứ tự) cho các ô trống
    public string GetMissingSequence()
    {
        var empties = GetEmptySlotIndices();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (int i in empties) sb.Append(word[i]);
        return sb.ToString();
    }

    // revert tất cả chữ do hint (chỉ những ô có isHinted = true)
    public void RevertHintedLetters()
    {
        for (int i = 0; i < boxTexts.Count; i++)
        {
            if (isHinted[i])
            {
                isHinted[i] = false;
                isRevealed[i] = false;
                boxTexts[i].text = "";
                revealedCount = Mathf.Max(0, revealedCount - 1);
            }
        }
    }

    // helper: fill specific index with given char (non-hint)
    public void FillIndexNonHint(int index, char c)
    {
        FillSlotAtIndex(index, c, false);
    }
}
