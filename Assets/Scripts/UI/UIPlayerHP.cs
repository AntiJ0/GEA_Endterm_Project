using UnityEngine;
using UnityEngine.UI;

public class UIPlayerHP : MonoBehaviour
{
    public static UIPlayerHP Instance;
    public Slider hpBar;

    void Awake()
    {
        Instance = this;
    }

    public void Refresh(int cur, int max)
    {
        hpBar.maxValue = max;
        hpBar.value = cur;
    }
}