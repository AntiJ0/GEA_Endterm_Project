using UnityEngine;

public class GameStartController : MonoBehaviour
{
    public PlayerController player;
    public MonoBehaviour[] systemsToDisable;

    public GameObject startText;
    public GameObject[] uiToEnableOnStart;

    bool started;

    void Start()
    {
        Time.timeScale = 0f;

        if (player != null)
            player.SetInputLocked(true);

        if (systemsToDisable != null)
            foreach (var s in systemsToDisable)
                if (s != null) s.enabled = false;

        if (startText != null)
            startText.SetActive(true);

        if (uiToEnableOnStart != null)
            foreach (var ui in uiToEnableOnStart)
                if (ui != null) ui.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (started) return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            StartGame();
        }
    }

    void StartGame()
    {
        started = true;

        Time.timeScale = 1f;

        if (player != null)
            player.SetInputLocked(false);

        if (systemsToDisable != null)
            foreach (var s in systemsToDisable)
                if (s != null) s.enabled = true;

        if (startText != null)
            startText.SetActive(false);

        if (uiToEnableOnStart != null)
            foreach (var ui in uiToEnableOnStart)
                if (ui != null) ui.SetActive(true);
    }
}