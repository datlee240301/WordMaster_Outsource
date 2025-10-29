using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeSceneUiManager : MonoBehaviour
{
    [Header("----------SettingButtonZone-----------")]
    [SerializeField] Image musicButton;
    [SerializeField] Sprite musicOnSprite;
    [SerializeField] Sprite musicOffSprite;

    [SerializeField] Image soundButton;
    [SerializeField] Sprite soundOnSprite;
    [SerializeField] Sprite soundOffSprite;

    // [SerializeField] Image vibrateButton;
    // [SerializeField] Sprite vibrateOnSprite;
    // [SerializeField] Sprite vibrateOffSprite;

    void Start()
    {
        // Lần đầu vào game → auto bật hết
        if (!PlayerPrefs.HasKey(StringManager.musicId))
            PlayerPrefs.SetInt(StringManager.musicId, 1);
        if (!PlayerPrefs.HasKey(StringManager.soundId))
            PlayerPrefs.SetInt(StringManager.soundId, 1);
        if (!PlayerPrefs.HasKey(StringManager.vibrateID))
            PlayerPrefs.SetInt(StringManager.vibrateID, 1);

        UpdateAllButtons();
    }

    void UpdateAllButtons()
    {
        UpdateMusicButton();
        UpdateSoundButton();
        //UpdateVibrateButton();
    }

    // 🔹 Hàm toggle Music
    public void ToggleMusic()
    {
        int state = PlayerPrefs.GetInt(StringManager.musicId, 1);
        state = 1 - state; // đảo 0 <-> 1
        PlayerPrefs.SetInt(StringManager.musicId, state);
        UpdateMusicButton();
    }

    void UpdateMusicButton()
    {
        bool isOn = PlayerPrefs.GetInt(StringManager.musicId, 1) == 1;
        musicButton.sprite = isOn ? musicOnSprite : musicOffSprite;
    }

    // 🔹 Hàm toggle Sound
    public void ToggleSound()
    {
        int state = PlayerPrefs.GetInt(StringManager.soundId, 1);
        state = 1 - state;
        PlayerPrefs.SetInt(StringManager.soundId, state);
        UpdateSoundButton();
    }

    void UpdateSoundButton()
    {
        bool isOn = PlayerPrefs.GetInt(StringManager.soundId, 1) == 1;
        soundButton.sprite = isOn ? soundOnSprite : soundOffSprite;
    }

    // 🔹 Hàm toggle Vibrate
    public void ToggleVibrate()
    {
        int state = PlayerPrefs.GetInt(StringManager.vibrateID, 1);
        state = 1 - state;
        PlayerPrefs.SetInt(StringManager.vibrateID, state);
        //UpdateVibrateButton();
    }

    // void UpdateVibrateButton()
    // {
    //     bool isOn = PlayerPrefs.GetInt(StringManager.vibrateID, 1) == 1;
    //     //vibrateButton.sprite = isOn ? vibrateOnSprite : vibrateOffSprite;
    // }
}
