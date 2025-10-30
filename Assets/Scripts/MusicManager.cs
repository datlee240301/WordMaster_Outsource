using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    int currentMusicId;

    // Start is called before the first frame update
    void Start()
    {
        currentMusicId = PlayerPrefs.GetInt(StringManager.musicId, 1);
        PlayerPrefs.SetInt(StringManager.musicId, currentMusicId);
        if (PlayerPrefs.GetInt(StringManager.musicId) == 0)
        {
            audioSource.volume = 0;
        }
        else
        {
            audioSource.volume = .6f;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}