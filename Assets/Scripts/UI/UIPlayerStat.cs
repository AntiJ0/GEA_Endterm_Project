using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStat : MonoBehaviour
{
    public static UIPlayerStat Instance;

    public Slider hpBar;

    public Slider hungerBar;

    public Slider armorBar;

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

    public void RefreshArmor(float value)
    {
        if (armorBar == null) return;

        armorBar.maxValue = 50f;
        armorBar.value = value;
    }
}