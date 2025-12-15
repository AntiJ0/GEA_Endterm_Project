using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    [Header("외부 인벤토리")]
    public UIInventorySlot[] outerSlots;

    [Header("내부 인벤토리")]
    public UIInventorySlot[] innerSlots;

    [Header("드래그 아이콘")]
    public Image dragIcon;

    [Header("아이템 아이콘")]
    public Sprite dirtIcon;
    public Sprite grassIcon;
    public Sprite waterIcon;
    public Sprite beefIcon;
    public Sprite porkIcon;
    public Sprite stoneIcon;
    public Sprite woodIcon;
    public Sprite diamondIcon;

    public UIInventorySlot[] slots => outerSlots;
    public int selectedIndex = -1;

    Dictionary<BlockType, Sprite> icons = new();

    UIInventorySlot draggingFrom;
    InventorySlotData draggingData;
    bool isDragging = false;

    Inventory inventory;

    UIInventorySlot hoverSlot;

    void Awake()
    {
        foreach (var s in outerSlots) s.Init(this);
        foreach (var s in innerSlots) s.Init(this);

        icons[BlockType.Dirt] = dirtIcon;
        icons[BlockType.Grass] = grassIcon;
        icons[BlockType.Water] = waterIcon;
        icons[BlockType.Beef] = beefIcon;
        icons[BlockType.Pork] = porkIcon;
        icons[BlockType.Stone] = stoneIcon;
        icons[BlockType.Wood] = woodIcon;
        icons[BlockType.Diamond] = diamondIcon;

        dragIcon.gameObject.SetActive(false);
    }

    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
            inventory.OnChanged += RefreshAll;

        RefreshAll(); 
    }

    void Update()
    {
        if (dragIcon.gameObject.activeSelf)
            dragIcon.transform.position = Input.mousePosition;
    }

    public Sprite GetIcon(BlockType t)
        => icons.TryGetValue(t, out var s) ? s : null;

    public void RefreshAll()
    {
        if (isDragging)
            return; 

        foreach (var s in outerSlots)
        {
            s.data.Clear();
            s.Refresh();
        }

        foreach (var s in innerSlots)
        {
            s.data.Clear();
            s.Refresh();
        }

        if (inventory == null) return;

        int index = 0;

        foreach (var pair in inventory.GetAll())
        {
            UIInventorySlot slot = null;

            if (index < outerSlots.Length)
                slot = outerSlots[index];
            else
            {
                int innerIndex = index - outerSlots.Length;
                if (innerIndex < innerSlots.Length)
                    slot = innerSlots[innerIndex];
            }

            if (slot == null) break;

            slot.data.Set(pair.Key, pair.Value);
            slot.Refresh();

            index++;
        }

        if (selectedIndex >= 0 &&
            selectedIndex < outerSlots.Length &&
            outerSlots[selectedIndex].data.IsEmpty)
        {
            Deselect();
        }
    }

    public void ToggleSelectSlot(int index)
    {
        if (index < 0 || index >= outerSlots.Length) return;

        if (selectedIndex == index)
        {
            Deselect();
            return;
        }

        Deselect();
        selectedIndex = index;
        outerSlots[index].SetSelected(true);
    }

    public void Deselect()
    {
        if (selectedIndex >= 0 && selectedIndex < outerSlots.Length)
            outerSlots[selectedIndex].SetSelected(false);

        selectedIndex = -1;
    }

    public int GetSelectedCount()
    {
        if (selectedIndex < 0) return 0;
        return outerSlots[selectedIndex].data.count;
    }

    public BlockType? GetSelectedBlockType()
    {
        if (selectedIndex < 0) return null;
        return outerSlots[selectedIndex].data.type;
    }

    public bool ConsumeOneFromSelected()
    {
        if (selectedIndex < 0) return false;

        var slot = outerSlots[selectedIndex];
        if (slot.data.count <= 0) return false;

        slot.data.count--;
        slot.Refresh();

        inventory?.Consume(slot.data.type, 1);
        return true;
    }

    public void BeginDrag(UIInventorySlot from)
    {
        isDragging = true;

        draggingFrom = from;
        draggingData = new InventorySlotData();
        draggingData.Set(from.data.type, from.data.count);

        dragIcon.sprite = GetIcon(draggingData.type);
        dragIcon.gameObject.SetActive(true);

        from.data.Clear();
        from.Refresh();
    }

    public void EndDrag()
    {
        if (draggingData == null)
            return;

        dragIcon.gameObject.SetActive(false);

        UIInventorySlot to = hoverSlot;

        if (to == null || to == draggingFrom)
        {
            Restore();
            return;
        }

        if (to.data.IsEmpty)
        {
            to.data.Set(draggingData.type, draggingData.count);
        }
        else
        {
            var tempType = to.data.type;
            var tempCount = to.data.count;

            to.data.Set(draggingData.type, draggingData.count);
            draggingFrom.data.Set(tempType, tempCount);
        }

        draggingFrom.Refresh();
        to.Refresh();

        draggingFrom = null;
        draggingData = null;
        hoverSlot = null;
        isDragging = false; 
    }

    void Restore()
    {
        draggingFrom.data.Set(draggingData.type, draggingData.count);
        draggingFrom.Refresh();

        draggingFrom = null;
        draggingData = null;
        isDragging = false; 
    }

    public void CancelDrag()
    {
        if (draggingData != null)
            Restore();

        dragIcon.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.OnChanged -= RefreshAll;
    }

    public void SetHoverSlot(UIInventorySlot slot)
    {
        hoverSlot = slot;
    }

    public void ClearHoverSlot(UIInventorySlot slot)
    {
        if (hoverSlot == slot)
            hoverSlot = null;
    }
}