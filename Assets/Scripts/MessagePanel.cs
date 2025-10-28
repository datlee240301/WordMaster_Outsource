using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MessagePanel : MonoBehaviour
{
    public TMP_Text text;
    public Image bg;
    public float showTime = 0.9f;
    Coroutine running;

    public void Show(string msg, bool positive)
    {
        if (running != null) StopCoroutine(running);
        bg.color = positive ? new Color(0.2f,0.8f,0.2f,0.9f) : new Color(0.8f,0.2f,0.2f,0.9f);
        text.text = msg;
        gameObject.SetActive(true);
        running = StartCoroutine(HideAfter());
    }

    IEnumerator HideAfter()
    {
        yield return new WaitForSeconds(showTime);
        gameObject.SetActive(false);
    }
}