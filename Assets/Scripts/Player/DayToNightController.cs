using UnityEngine;

public class DayToNightController : MonoBehaviour
{
    public Material skyboxMaterial;

    public float startExposure = 3f;
    public float nightExposure = 0.3f;

    public Color startAmbientColor = Color.white;
    public Color nightAmbientColor = new Color(0.15f, 0.18f, 0.3f);

    public float nightStartDelay = 10f;
    public float transitionDuration = 20f;

    public AnimalSpawner animalSpawner;
    public MonsterSpawner monsterSpawner;

    float timer;
    bool transitioning;
    bool isNight;

    void Start()
    {
        RenderSettings.skybox = skyboxMaterial;
        skyboxMaterial.SetFloat("_Exposure", startExposure);
        RenderSettings.ambientLight = startAmbientColor;

        SetDayState();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!transitioning && timer >= nightStartDelay)
        {
            transitioning = true;
            timer = 0f;
        }

        if (transitioning && !isNight)
        {
            float t = Mathf.Clamp01(timer / transitionDuration);

            float exposure = Mathf.Lerp(startExposure, nightExposure, t);
            Color ambient = Color.Lerp(startAmbientColor, nightAmbientColor, t);

            skyboxMaterial.SetFloat("_Exposure", exposure);
            RenderSettings.ambientLight = ambient;

            if (t >= 1f)
                SetNightState();
        }
    }

    void SetDayState()
    {
        isNight = false;
        if (animalSpawner != null) animalSpawner.enabled = true;
        if (monsterSpawner != null) monsterSpawner.enabled = false;
    }

    void SetNightState()
    {
        isNight = true;
        if (animalSpawner != null) animalSpawner.enabled = false;
        if (monsterSpawner != null) monsterSpawner.enabled = true;
    }
}