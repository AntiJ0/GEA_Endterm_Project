using System.Collections.Generic;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public UIInventorySlot[] slots;

    public Sprite dirtIcon;
    public Sprite grassIcon;
    public Sprite waterIcon;

    private Dictionary<BlockType, Sprite> icons = new();
    private Inventory _inventory;

    public int selectedIndex { get; private set; } = -1;

    void Awake()
    {
        icons[BlockType.Dirt] = dirtIcon;
        icons[BlockType.Grass] = grassIcon;
        icons[BlockType.Water] = waterIcon;
    }

    void Start()
    {
        _inventory = FindObjectOfType<Inventory>();
        if (_inventory != null)
        {
            _inventory.OnChanged += RefreshUI;
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        if (_inventory == null) return;

        for (int k = 0; k < slots.Length; k++)
            slots[k].Clear();

        int slotIndex = 0;

        foreach (var pair in _inventory.GetAll())
        {
            if (slotIndex >= slots.Length) break;
            BlockType type = pair.Key;
            int count = pair.Value;

            Sprite icon = icons.ContainsKey(type) ? icons[type] : null;

            slots[slotIndex].SetItem(icon, count, type);
            slotIndex++;
        }

        if (selectedIndex >= 0 && selectedIndex < slots.Length)
        {
            if (slots[selectedIndex].IsEmpty())
                Deselect();
        }

        UpdateSlotSelections();
    }

    public void ToggleSelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        if (selectedIndex == index)
            Deselect();
        else
            Select(index);
    }

    public void Select(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        if (slots[index].IsEmpty()) return;

        selectedIndex = index;
        UpdateSlotSelections();
    }

    public void Deselect()
    {
        selectedIndex = -1;
        UpdateSlotSelections();
    }

    private void UpdateSlotSelections()
    {
        for (int j = 0; j < slots.Length; j++)
            slots[j].SetSelected(j == selectedIndex);
    }

    public BlockType? GetSelectedBlockType()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Length) return null;
        return slots[selectedIndex].blockType;
    }

    public int GetSelectedCount()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Length) return 0;
        return slots[selectedIndex].count;
    }

    public bool ConsumeOneFromSelected()
    {
        if (_inventory == null) return false;

        var bt = GetSelectedBlockType();
        if (bt == null) return false;

        return _inventory.Consume(bt.Value, 1);
    }
}