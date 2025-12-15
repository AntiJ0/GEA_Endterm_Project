using UnityEngine;

public class InventoryPanelController : MonoBehaviour
{
    public GameObject panel;
    public UIInventory uiInventory;
    public PlayerController player;

    bool opened = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (opened) Close();
            else Open();
        }
    }

    void Open()
    {
        opened = true;
        panel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        player.enabled = false;
    }

    void Close()
    {
        opened = false;
        panel.SetActive(false);

        uiInventory.CancelDrag();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        player.enabled = true;
    }
}