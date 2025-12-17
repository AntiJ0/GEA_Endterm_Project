using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextFader : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float minAlpha = 0f;
    public float maxAlpha = 1f;

    Text uiText;
    TMP_Text tmpText;

    float timer;
    bool fadeIn = true;

    void Awake()
    {
        uiText = GetComponent<Text>();
        tmpText = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        timer = 0f;
        fadeIn = true;
        SetAlpha(minAlpha);
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(timer / fadeDuration);

        float alpha = fadeIn
            ? Mathf.Lerp(minAlpha, maxAlpha, t)
            : Mathf.Lerp(maxAlpha, minAlpha, t);

        SetAlpha(alpha);

        if (t >= 1f)
        {
            timer = 0f;
            fadeIn = !fadeIn;
        }
    }

    void SetAlpha(float a)
    {
        if (uiText != null)
        {
            var c = uiText.color;
            c.a = a;
            uiText.color = c;
        }

        if (tmpText != null)
        {
            var c = tmpText.color;
            c.a = a;
            tmpText.color = c;
        }
    }
}