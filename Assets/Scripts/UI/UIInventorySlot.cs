using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIInventorySlot : MonoBehaviour,
    IPointerDownHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Image slotBackground;
    public Image iconImage;
    public TMP_Text countText;

    public InventorySlotData data;
    private UIInventory owner;

    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    public void Init(UIInventory inv)
    {
        owner = inv;
        data ??= new InventorySlotData();
        SetSelected(false);
        Refresh();
    }

    public void Refresh()
    {
        if (data.IsEmpty)
        {
            iconImage.enabled = false;
            countText.text = "";
        }
        else
        {
            iconImage.enabled = true;
            iconImage.sprite = owner.GetIcon(data.type);
            countText.text = data.count > 1 ? data.count.ToString() : "";
        }
    }

    public void SetSelected(bool selected)
    {
        if (slotBackground != null)
            slotBackground.color = selected ? selectedColor : normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            owner.OnSlotLeftClick(this);
        else if (eventData.button == PointerEventData.InputButton.Right)
            owner.OnSlotRightClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        owner.SetHoverSlot(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        owner.ClearHoverSlot(this);
    }
}