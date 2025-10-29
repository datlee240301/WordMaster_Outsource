using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public TMP_Text currentBuiltWordText; // text hiển thị từ đang nối theo ngón (ở góc hoặc follow)
    public Image currentWordBackground;
    public LineDrawer lineDrawer; // vẽ đường nối (optional)
    //public MessagePanel messagePanel;

    [Header("Level settings")]
    public int startLevelIndex = 0;

    // runtime
    private LevelItem currentLevel;
    private List<GameObject> spawnedLetterButtons = new List<GameObject>();
    private Dictionary<string, WordRowUI> wordRowMap = new Dictionary<string, WordRowUI>();
    public UnityEngine.UI.Button hintButton;
    public string Definition { get; private set; }
    // theo file GameManager của bạn
    public TMP_Text definitionDisplay; // text cố định trên UI để hiển thị definition
    private string lastHintedWord = null; // lưu từ đang được hint (để revert nếu fail)
    public VerticalLayoutGroup wordRowsLayoutGroup;

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
        hintButton.onClick.RemoveAllListeners();
        hintButton.onClick.AddListener(OnHintPressed);
        DebugCountLetterBoxesInFirstRow();
    }
    // gọi khi người nhấn nút Hint
    public void OnHintPressed()
    {
        foreach (Transform child in wordRowsParent)
        {
            var row = child.GetComponent<WordRowUI>();
            if (row == null) continue;
            if (!row.IsFullyRevealed())
            {
                // show definition ở chỗ cố định (một lần đầu)
                if (definitionDisplay != null)
                    definitionDisplay.text = row.Definition;

                int revealedIndex = row.RevealOneLetterHint(); // reveal 1 ký tự, đánh dấu isHinted
                lastHintedWord = row.word.ToUpper();

                // nếu reveal làm đầy hàng
                if (row.IsFullyRevealed())
                {
                    Debug.Log("ăn 1 hàng");
                    if (wordRowMap.ContainsKey(row.word.ToUpper()))
                        wordRowMap.Remove(row.word.ToUpper());

                    // clear definition display
                    if (definitionDisplay != null) definitionDisplay.text = "";
                    lastHintedWord = null;

                    if (wordRowMap.Count == 0) Debug.Log("win");
                }
                break; // chỉ hint 1 hàng mỗi lần bấm
            }
        }
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
            ui.Setup(w.word, w.definition);

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

        if (currentWordBackground != null)
        {
            bool hasText = !string.IsNullOrEmpty(w);
            currentWordBackground.gameObject.SetActive(hasText);

            if (hasText)
            {
                // đo chiều rộng text (tính theo số ký tự)
                float widthPerChar = currentBuiltWordText.fontSize * 0.6f; // chỉnh tỉ lệ nếu cần
                float targetWidth = Mathf.Max(100f, w.Length * widthPerChar + 40f); // padding
                var bgRT = currentWordBackground.rectTransform;
                Vector2 size = bgRT.sizeDelta;
                size.x = targetWidth;
                bgRT.sizeDelta = size;
            }
        }
    }


    public void AddLinePoint(Vector3 worldPos)
    {
        lineDrawer?.AddPoint(worldPos);
    }

    public void ClearLine()
    {
        lineDrawer?.Clear();
        currentBuiltWordText.text = "";
        if (currentWordBackground != null)
            currentWordBackground.gameObject.SetActive(false);

    }

    // Khi người dùng thả chuột/vuốt (submit word)
    // word phải đã được tạo bởi WordBuilder (chuỗi chữ)
    public void OnSubmitWord(string submittedWord, List<LetterButton> pathUsed)
{
    if (string.IsNullOrEmpty(submittedWord)) return;
    string submitted = submittedWord.ToUpper();

    // 1) Nếu submitted khớp chính xác 1 từ trong map (đủ hoặc một phần đã có do hint)
    if (wordRowMap.ContainsKey(submitted))
    {
        var row = wordRowMap[submitted];
        List<int> emptyIdx = row.GetEmptySlotIndices(); // chỉ số ô trống (left->right)
        if (emptyIdx.Count == 0)
        {
            // đã đầy (không nên xảy ra) -> nothing
            return;
        }

        // Chúng ta muốn điền theo vị trí của từ: tức là ô targetIndex sẽ lấy ký tự submitted[targetIndex]
        // và cố gắng tìm một LetterButton trong pathUsed có letter == needed để animate.
        bool[] usedPath = new bool[pathUsed.Count];

        for (int e = 0; e < emptyIdx.Count; e++)
        {
            int targetIndex = emptyIdx[e];
            if (targetIndex < 0 || targetIndex >= submitted.Length) continue;

            char needed = submitted[targetIndex];

            // tìm trong pathUsed một nút chưa dùng có letter == needed
            int foundPathIdx = -1;
            for (int p = 0; p < pathUsed.Count; p++)
            {
                if (usedPath[p]) continue;
                if (char.ToUpper(pathUsed[p].letter) == char.ToUpper(needed))
                {
                    foundPathIdx = p;
                    break;
                }
            }

            // nếu không tìm thấy nút cùng ký tự trong pathUsed, lấy nút bất kỳ chưa dùng (fallback)
            if (foundPathIdx == -1)
            {
                for (int p = 0; p < pathUsed.Count; p++)
                {
                    if (!usedPath[p])
                    {
                        foundPathIdx = p;
                        break;
                    }
                }
            }

            // nếu vẫn không tìm thấy (rất hiếm) -> bỏ qua
            if (foundPathIdx == -1) continue;

            // dùng nút tìm được để animate tới slot tương ứng
            var btn = pathUsed[foundPathIdx];
            usedPath[foundPathIdx] = true;

            // animate rồi điền ký tự (chúng ta fill bằng ký tự đúng vị trí của từ)
            StartCoroutine(MoveUILetterToSlot(btn, row.GetSlotRectAtIndex(targetIndex), 0.32f));
            row.FillIndexNonHint(targetIndex, needed);
        }

        // hoàn tất hàng
        Debug.Log("ăn 1 hàng");
        wordRowMap.Remove(submitted);
        if (definitionDisplay != null) definitionDisplay.text = "";
        if (lastHintedWord != null && lastHintedWord == submitted) lastHintedWord = null;

        if (wordRowMap.Count == 0) Debug.Log("win");
        return;
    }

    // 2) Nếu không khớp toàn bộ từ, thử khớp chuỗi thiếu của từng row (như cũ)
    bool matchedMissing = false;
    foreach (var kv in new Dictionary<string, WordRowUI>(wordRowMap))
    {
        var row = kv.Value;
        string missing = row.GetMissingSequence(); // chuỗi còn thiếu (theo thứ tự)
        if (lastHintedWord == kv.Key.ToUpper()) continue; // nếu đã hint -> bỏ qua phần thiếu

        if (missing.Length == 0) continue;
        if (submitted == missing)
        {
            // điền theo thứ tự các ô trống
            var empties = row.GetEmptySlotIndices();
            int fillCount = Mathf.Min(empties.Count, pathUsed.Count);
            for (int i = 0; i < fillCount; i++)
            {
                int targetIdx = empties[i];
                row.FillIndexNonHint(targetIdx, pathUsed[i].letter);
                StartCoroutine(MoveUILetterToSlot(pathUsed[i], row.GetSlotRectAtIndex(targetIdx), 0.32f));
            }

            if (row.IsFullyRevealed())
            {
                Debug.Log("ăn 1 hàng");
                wordRowMap.Remove(row.word.ToUpper());
                if (definitionDisplay != null) definitionDisplay.text = "";
                if (lastHintedWord != null && lastHintedWord == row.word.ToUpper()) lastHintedWord = null;

                if (wordRowMap.Count == 0) Debug.Log("win");
            }

            matchedMissing = true;
            break;
        }
    }

    if (!matchedMissing)
    {
        // Nếu có hint rồi thì KHÔNG revert hint, chỉ báo sai
        if (!string.IsNullOrEmpty(lastHintedWord) && wordRowMap.ContainsKey(lastHintedWord))
        {
            // chỉ feedback sai, KHÔNG xóa hint
            //messagePanel?.Show("Sai!", false);
            return;
        }

        // nếu không có hint thì revert như bình thường
        //messagePanel?.Show("Sai!", false);
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
    public void DebugCountLetterBoxesInFirstRow()
    {
        if (wordRowsParent == null)
        {
            Debug.LogError("`wordRowsParent` null");
            return;
        }

        if (wordRowsParent.childCount == 0)
        {
            Debug.Log("Không có hàng từ nào trong `wordRowsParent`.");
            return;
        }

        Transform firstRow = wordRowsParent.GetChild(0);
        int candidateCount = 0;
        foreach (Transform child in firstRow)
        {
            string nameLower = child.gameObject.name.ToLower();
            if (nameLower.Contains("letter") || nameLower.Contains("box") || nameLower.Contains("slot"))
                candidateCount++;
        }

        Debug.Log($"Hàng đầu tiên `{firstRow.name}` có {candidateCount} LetterBoxPrefab (tổng child: {firstRow.childCount}).");
        if(candidateCount==3)
            wordRowsLayoutGroup.padding.left = -120;
        if(candidateCount==4)
            wordRowsLayoutGroup.padding.left = -180;
        if(candidateCount==5)
            wordRowsLayoutGroup.padding.left = -240;
    }
}
