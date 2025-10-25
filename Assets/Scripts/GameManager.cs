using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("Prefabs & refs")]
    public GameObject letterButtonPrefab; // prefab: Button/Image + Text + LetterButton script
    public Transform circleParent; // RectTransform (empty) làm tâm vòng
    public float circleRadius = 200f; // bán kính (ui units)
    public GameObject wordRowPrefab; // prefab chứa WordRowUI
    public Transform wordRowsParent; // nơi chứa các hàng từ
    public Text currentBuiltWordText; // text hiển thị từ đang nối theo ngón (ở góc hoặc follow)
    public LineDrawer lineDrawer; // vẽ đường nối (optional)
    public MessagePanel messagePanel;

    [Header("Level settings")]
    public int startLevelIndex = 0;

    // runtime
    private LevelItem currentLevel;
    private List<GameObject> spawnedLetterButtons = new List<GameObject>();
    private Dictionary<string, WordRowUI> wordRowMap = new Dictionary<string, WordRowUI>();

    void Awake()
    {
        if (I == null) I = this; else Destroy(gameObject);
    }

    void Start()
    {
        if (LevelLoader.AllLevels == null || LevelLoader.AllLevels.Count == 0)
        {
            var loader = FindObjectOfType<LevelLoader>();
            if (loader == null) { Debug.LogError("Cần LevelLoader trong scene"); return; }
            loader.Load();
        }
        LoadLevel(startLevelIndex);
    }

    public void LoadLevel(int index)
    {
        ClearLevel();

        if (index < 0 || index >= LevelLoader.AllLevels.Count)
        {
            Debug.LogError("Index level ngoài phạm vi");
            return;
        }

        currentLevel = LevelLoader.AllLevels[index];

        // 1) Spawn letters on circle
        for (int i = 0; i < currentLevel.letters.Count; i++)
        {
            string s = currentLevel.letters[i];
            GameObject go = Instantiate(letterButtonPrefab, circleParent);
            go.name = "Letter_" + s + "_" + i;
            var lb = go.GetComponent<LetterButton>();
            lb.Setup(s[0], this); // truyền char và tham chiếu manager
            spawnedLetterButtons.Add(go);
        }
        ArrangeLettersInCircle();

        // 2) Spawn word rows (mỗi word 1 hàng, tự sinh đủ ô trong hàng)
        foreach (var w in currentLevel.words)
        {
            GameObject r = Instantiate(wordRowPrefab, wordRowsParent);
            var ui = r.GetComponent<WordRowUI>();
            ui.Setup(w.word); // tạo đủ ô
            wordRowMap[w.word.ToUpper()] = ui;
        }

        // clear UI helpers
        currentBuiltWordText.text = "";
        lineDrawer?.Clear();
    }

    void ClearLevel()
    {
        foreach (var g in spawnedLetterButtons) if (g) Destroy(g);
        spawnedLetterButtons.Clear();
        foreach (Transform t in wordRowsParent) Destroy(t.gameObject);
        wordRowMap.Clear();
        lineDrawer?.Clear();
    }

    void ArrangeLettersInCircle()
    {
        int count = spawnedLetterButtons.Count;
        if (count == 0) return;
        float angleStep = 360f / count;
        float startAngle = 90f; // 90 = chữ đầu trên đỉnh; chỉnh nếu muốn
        RectTransform centerRT = circleParent.GetComponent<RectTransform>();

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle - i * angleStep;
            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * circleRadius;
            float y = Mathf.Sin(rad) * circleRadius;

            RectTransform rt = spawnedLetterButtons[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, y);
            rt.localRotation = Quaternion.identity;
        }
    }

    // Giao diện dành cho LetterButton / WordBuilder
    public void ShowCurrentBuiltWord(string w)
    {
        currentBuiltWordText.text = w;
    }

    public void AddLinePoint(Vector3 worldPos)
    {
        lineDrawer?.AddPoint(worldPos);
    }

    public void ClearLine()
    {
        lineDrawer?.Clear();
    }

    // Khi người dùng thả chuột/vuốt (submit word)
    // word phải đã được tạo bởi WordBuilder (chuỗi chữ)
    public void OnSubmitWord(string word, List<LetterButton> pathUsed)
    {
        if (string.IsNullOrEmpty(word)) return;
        string W = word.ToUpper();

        if (wordRowMap.ContainsKey(W))
        {
            // đúng: animate từng chữ bay vào hàng đúng
            StartCoroutine(AnimateWordToRow(W, pathUsed));
            messagePanel?.Show("Đúng!", true);
        }
        else
        {
            // sai
            messagePanel?.Show("Sai!", false);
            // feedback: shake / flash
        }
    }

    IEnumerator AnimateWordToRow(string word, List<LetterButton> pathUsed)
    {
        if (!wordRowMap.ContainsKey(word)) yield break;
        var row = wordRowMap[word];

        // Lấy danh sách index của các ô trống theo thứ tự
        List<int> emptyIndexes = row.GetEmptySlotIndices();
        if (emptyIndexes.Count == 0) yield break;

        // nếu số chữ lớn hơn số ô trống, ta chỉ xử lý tối đa bằng số ô trống
        int countToFill = Mathf.Min(pathUsed.Count, emptyIndexes.Count);

        for (int i = 0; i < countToFill; i++)
        {
            var lb = pathUsed[i];
            int targetIndex = emptyIndexes[i];
            RectTransform slotRect = row.GetSlotRectAtIndex(targetIndex);
            if (slotRect == null)
            {
                // fallback: skip
                continue;
            }

            // Animate letter visual từ lb -> slotRect
            yield return StartCoroutine(MoveUILetterToSlot(lb, slotRect, 0.32f));

            // Sau animation: fill đúng ô targetIndex
            row.FillSlotAtIndex(targetIndex, lb.letter);

            // **KHÔNG** disable nút gốc: cho phép dùng tiếp
            // Nếu bạn muốn ẩn nút sau khi dùng, uncomment dòng dưới (tùy gameplay)
            // lb.SetInteractable(false);
        }
        // --- CHỖ CHÈN DEBUG: đã ăn 1 hàng
        Debug.Log("ăn 1 hàng");

        // loại bỏ word khỏi map để không cho người chơi làm lại (tuỳ bạn muốn)
        wordRowMap.Remove(word);
        // --- CHỖ CHÈN DEBUG: nếu hết hàng => win
        if (wordRowMap.Count == 0)
        {
            Debug.Log("win");
        }
        yield return null;
    }


    IEnumerator MoveUILetterToSlot(LetterButton lb, RectTransform target, float duration)
    {
        // tạo một copy visual của letter (để animation mượt, giữ original button disabled)
        var copy = new GameObject("AnimLetter");
        copy.transform.SetParent(lb.transform.root, false); // parent to canvas
        var rtCopy = copy.AddComponent<RectTransform>();
        var img = copy.AddComponent<UnityEngine.UI.Image>();
        // copy appearance
        var origImg = lb.GetComponent<UnityEngine.UI.Image>();
        if (origImg) img.sprite = origImg.sprite;
        img.raycastTarget = false;
        var txt = new GameObject("T", typeof(RectTransform)).AddComponent<UnityEngine.UI.Text>();
        txt.transform.SetParent(copy.transform, false);
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = lb.letter.ToString();
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;
        txt.rectTransform.sizeDelta = new Vector2(100, 100);

        // set start pos = world pos of lb
        RectTransform lbRT = lb.GetComponent<RectTransform>();
        Vector3 startWorld;
        Vector2 localPos;
        startWorld = lbRT.TransformPoint(lbRT.rect.center);
        // convert to canvas local by inverting parent
        rtCopy.sizeDelta = lbRT.sizeDelta;
        // world -> anchored pos in canvas
        Canvas canvas = lbRT.GetComponentInParent<Canvas>();
        Vector2 startAnchored;
        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        // convert
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, RectTransformUtility.WorldToScreenPoint(null, startWorld), canvas.worldCamera, out startAnchored);
        rtCopy.SetParent(canvasRT, false);
        rtCopy.anchoredPosition = startAnchored;

        // target anchored position
        Vector3 targetWorld = target.TransformPoint(target.rect.center);
        Vector2 targetAnchored;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, RectTransformUtility.WorldToScreenPoint(null, targetWorld), canvas.worldCamera, out targetAnchored);

        // animate
        float t = 0f;
        Vector2 from = rtCopy.anchoredPosition;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / duration);
            rtCopy.anchoredPosition = Vector2.Lerp(from, targetAnchored, p);
            yield return null;
        }
        rtCopy.anchoredPosition = targetAnchored;

        // destroy copy
        Destroy(copy);
        yield return null;
    }
}
