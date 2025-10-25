using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public static List<LevelItem> AllLevels;

    void Awake()
    {
        Load();
    }

    public void Load()
    {
        TextAsset ta = Resources.Load<TextAsset>("WordLevels");
        if (ta == null)
        {
            Debug.LogError("Không tìm thấy Resources/WordLevels.json");
            AllLevels = new List<LevelItem>();
            return;
        }
        AllLevels = JsonHelper.FromJson<LevelItem>(ta.text);
    }
}