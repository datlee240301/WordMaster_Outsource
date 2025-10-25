using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WordRowUI : MonoBehaviour
{
    public string word { get; private set; }
    public GameObject letterBoxPrefab; // prefab cho 1 ô (Image + Text)
    private List<Text> boxTexts = new List<Text>();
    private List<RectTransform> boxRects = new List<RectTransform>();

    public void Setup(string targetWord)
    {
        word = targetWord.ToUpper();
        foreach (Transform t in transform) Destroy(t.gameObject);
        boxTexts.Clear();
        boxRects.Clear();

        for (int i = 0; i < word.Length; i++)
        {
            GameObject b = Instantiate(letterBoxPrefab, transform);
            b.name = "Box_" + i;
            var txt = b.GetComponentInChildren<Text>();
            txt.text = "_";
            boxTexts.Add(txt);
            boxRects.Add(b.GetComponent<RectTransform>());
        }
    }

    // Trả về danh sách các index các ô còn trống, theo thứ tự từ trái sang phải
    public List<int> GetEmptySlotIndices()
    {
        var list = new List<int>();
        for (int i = 0; i < boxTexts.Count; i++)
        {
            if (string.IsNullOrEmpty(boxTexts[i].text) || boxTexts[i].text == "_")
                list.Add(i);
        }
        return list;
    }

    // Trả về RectTransform ô tại index
    public RectTransform GetSlotRectAtIndex(int index)
    {
        if (index < 0 || index >= boxRects.Count) return null;
        return boxRects[index];
    }

    // fill tại index (thứ tự) (giả sử index hợp lệ)
    public void FillSlotAtIndex(int index, char c)
    {
        if (index < 0 || index >= boxTexts.Count) return;
        boxTexts[index].text = c.ToString();
    }
}