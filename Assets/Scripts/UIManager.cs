using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image fadeOverlay;

    [Header("----------SettingButtonZone-----------")] [SerializeField]
    Image musicButton;

    [SerializeField] Sprite musicOnSprite;
    [SerializeField] Sprite musicOffSprite;

    [SerializeField] Image soundButton;
    [SerializeField] Sprite soundOnSprite;
    [SerializeField] Sprite soundOffSprite;

    // [SerializeField] Image vibrateButton;
    // [SerializeField] Sprite vibrateOnSprite;
    // [SerializeField] Sprite vibrateOffSprite;

    [SerializeField] TextMeshProUGUI ticketNumberText, shopTicketNumberText, currentLevelText;
    int ticketNumber;
    [SerializeField] UiPanelDotween noticePanel;
    int LevelId;

    [Header("Splash Loading")] [SerializeField]
    Image loadingImage; // fillAmount 0 → 1

    [SerializeField] float loadingDuration = 3f; // thời gian chạy đầy thanh loading

    // private SoundManager soundManager;
    // private MusicManager musicManager;

    void Start()
    {
        // soundManager = FindObjectOfType<SoundManager>();
        // musicManager = FindObjectOfType<MusicManager>();

        if (SceneManager.GetActiveScene().name == "Home")
        {
            LevelId = PlayerPrefs.GetInt(StringManager.currentLevelId, 1);
            PlayerPrefs.SetInt(StringManager.currentLevelId, LevelId);
            ticketNumber = PlayerPrefs.GetInt(StringManager.ticketNumber, 150);
            PlayerPrefs.SetInt(StringManager.ticketNumber, ticketNumber);
            ticketNumberText.text = ticketNumber.ToString();
            if (shopTicketNumberText != null)
                shopTicketNumberText.text = ticketNumber.ToString();

            // Lần đầu vào game → auto bật hết
            if (!PlayerPrefs.HasKey(StringManager.musicId))
                PlayerPrefs.SetInt(StringManager.musicId, 1);
            if (!PlayerPrefs.HasKey(StringManager.soundId))
                PlayerPrefs.SetInt(StringManager.soundId, 1);
            if (!PlayerPrefs.HasKey(StringManager.vibrateID))
                PlayerPrefs.SetInt(StringManager.vibrateID, 1);

            UpdateAllButtons();
        }

        if (SceneManager.GetActiveScene().name == "Main")
        {
            ticketNumber = PlayerPrefs.GetInt(StringManager.ticketNumber);
            ticketNumberText.text = ticketNumber.ToString();
            if (currentLevelText != null)
                currentLevelText.text = "Level " + PlayerPrefs.GetInt(StringManager.currentLevelId).ToString();
        }

        if (SceneManager.GetActiveScene().name == "Splash")
        {
            if (loadingImage != null)
            {
                loadingImage.fillAmount = 0f;
                StartCoroutine(FillLoadingBarAndGoHome());
            }
        }
    }

    IEnumerator FillLoadingBarAndGoHome()
    {
        float elapsed = 0f;
        while (elapsed < loadingDuration)
        {
            elapsed += Time.deltaTime;
            loadingImage.fillAmount = Mathf.Clamp01(elapsed / loadingDuration);
            yield return null;
        }

        // khi đầy 100% thì fade và load Home
        yield return FadeAndLoadScene("Home");
    }

    /// <summary>
    /// Button Zone
    /// </summary>
    public void PlayButton(string sceneName)
    {
        PlayerPrefs.SetInt(StringManager.pressPlayButton, 1);
        PlayerPrefs.SetInt(StringManager.pressLevelButton, 0);
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    public void LoadSceneButton(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    public void NextLevelButton()
    {
        // if (gameManager.currentLevelId == 15)
        //     StartCoroutine(FadeAndLoadScene("Home"));
        // else
        //     StartCoroutine(FadeAndLoadScene("Main"));
    }

    public void LoadLevelButton(int levelId)
    {
        if (levelId <= PlayerPrefs.GetInt(StringManager.currentLevelId))
        {
            PlayerPrefs.SetInt(StringManager.pressLevelButton, 1);
            PlayerPrefs.SetInt(StringManager.pressPlayButton, 0);
            StartCoroutine(FadeAndLoadScene("Main"));
        }
        else
        {
            noticePanel.PanelFadeIn();
        }
    }

    public void BuyTicket(int amount)
    {
        ticketNumber += amount;
        PlayerPrefs.SetInt(StringManager.ticketNumber, ticketNumber);
        ticketNumberText.text = ticketNumber.ToString();
        if (shopTicketNumberText != null)
            shopTicketNumberText.text = ticketNumber.ToString();
    }

    public void MinusTicket(int amount)
    {
        ticketNumber -= amount;
        PlayerPrefs.SetInt(StringManager.ticketNumber, ticketNumber);
        ticketNumberText.text = ticketNumber.ToString();
    }

    void UpdateAllButtons()
    {
        UpdateMusicButton();
        UpdateSoundButton();
        UpdateVibrateButton();
    }

    // 🔹 Toggle Music
    public void ToggleMusic()
    {
        int state = PlayerPrefs.GetInt(StringManager.musicId, 1);
        state = 1 - state;
        PlayerPrefs.SetInt(StringManager.musicId, state);
        UpdateMusicButton();
    }

    void UpdateMusicButton()
    {
        bool isOn = PlayerPrefs.GetInt(StringManager.musicId, 1) == 1;
        musicButton.sprite = isOn ? musicOnSprite : musicOffSprite;

        // chỉnh volume MusicManager
        // if (musicManager != null && musicManager.GetComponent<AudioSource>() != null)
        //     musicManager.audioSource.volume = isOn ? 1f : 0f;
    }

    // 🔹 Toggle Sound
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

        // chỉnh volume SoundManager
        // if (soundManager != null && soundManager.GetComponent<AudioSource>() != null)
        //     soundManager.audioSource.volume = isOn ? 1f : 0f;
    }

    // 🔹 Toggle Vibrate
    public void ToggleVibrate()
    {
        int state = PlayerPrefs.GetInt(StringManager.vibrateID, 1);
        state = 1 - state;
        PlayerPrefs.SetInt(StringManager.vibrateID, state);
        UpdateVibrateButton();
    }

    public void Rate()
    {
        //soundManager.PlayUIClickSound();
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.alosn.alosongngu");
    }

    void UpdateVibrateButton()
    {
        bool isOn = PlayerPrefs.GetInt(StringManager.vibrateID, 1) == 1;
        //vibrateButton.sprite = isOn ? vibrateOnSprite : vibrateOffSprite;
    }

    /// <summary>
    /// Fade & Load Scene
    /// </summary>
    IEnumerator FadeAndLoadScene(string sceneName)
    {
        Color color = fadeOverlay.color;
        color.a = 0f;
        fadeOverlay.color = color;
        fadeOverlay.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);

        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / duration);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeOverlay.color = color;
        SceneManager.LoadScene(sceneName);
    }
}