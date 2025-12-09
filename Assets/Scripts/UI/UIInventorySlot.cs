using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventorySlot : MonoBehaviour
{
    [Header("UI ¿ä¼Ò")]
    public Image slotBackground; 
    public Image iconImage;
    public TMP_Text countText;

    [HideInInspector] public BlockType blockType;
    [HideInInspector] public int count;

    private readonly Color backgroundNormal = Color.white;
    private readonly Color backgroundSelected = new Color(1f, 0.85f, 0.0f, 1f); 

    void Reset()
    {
    }

    public void SetItem(Sprite icon, int count, BlockType type = BlockType.Dirt)
    {
        this.blockType = type;
        this.count = count;

        if (iconImage != null)
        {
            iconImage.enabled = (icon != null);
            iconImage.sprite = icon;
        }

        if (slotBackground != null)
            slotBackground.color = backgroundNormal;

        countText.text = count > 1 ? count.ToString() : "";
    }

    public void Clear()
    {
        blockType = default;
        count = 0;

        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }
        if (slotBackground != null)
            slotBackground.color = backgroundNormal;

        countText.text = "";
    }

    public void SetSelected(bool sel)
    {
        if (slotBackground != null)
            slotBackground.color = sel ? backgroundSelected : backgroundNormal;
        else
        {
            if (iconImage != null)
                iconImage.color = sel ? Color.yellow : Color.white;
        }
    }

    public bool IsEmpty()
    {
        return count <= 0;
    }
}