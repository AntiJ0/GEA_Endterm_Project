using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStat : MonoBehaviour
{
    public static UIPlayerStat Instance;

    [Header("HP")]
    public Slider hpBar;

    [Header("Hunger")]
    public Slider hungerBar;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RefreshHP(int cur, int max)
    {
        if (hpBar == null) return;

        hpBar.maxValue = max;
        hpBar.value = cur;
    }

    public void RefreshHunger(int cur, int max)
    {
        if (hungerBar == null) return;

        hungerBar.maxValue = max;
        hungerBar.value = cur;
    }
}