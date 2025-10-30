using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource audioSource;
    [SerializeField] AudioClip foundWordSound;
    [SerializeField] AudioClip getCoinSound;
    [SerializeField] AudioClip uiClickSound;
    [SerializeField] AudioClip winSound;
    int currentSoundId;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentSoundId = PlayerPrefs.GetInt(StringManager.soundId, 1);
        PlayerPrefs.SetInt(StringManager.soundId, currentSoundId);
        if (PlayerPrefs.GetInt(StringManager.soundId) == 0)
        {
            audioSource.volume = 0;
        }
        else
        {
            audioSource.volume = 1f;
        }
    }
    public void PlayFoundWordSound()
    {
        audioSource.PlayOneShot(foundWordSound);
    }
    public void PlayGetCoinSound()
    {
        audioSource.PlayOneShot(getCoinSound);
    }
    public void PlayUIClickSound()
    {
        audioSource.PlayOneShot(uiClickSound);
    }
    public void PlayWinSound()
    {
        audioSource.PlayOneShot(winSound);
    }
}