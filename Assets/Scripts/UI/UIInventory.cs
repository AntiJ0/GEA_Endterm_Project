using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    [Header("외부 인벤토리")]
    public UIInventorySlot[] outerSlots;

    [Header("내부 인벤토리")]
    public UIInventorySlot[] innerSlots;

    [Header("방어구 슬롯")]
    public UIArmorSlot[] armorSlots;

    [Header("조합 재료 슬롯")]
    public UIInventorySlot[] craftInputSlots;

    [Header("조합 결과 슬롯")]
    public UIInventorySlot craftResultSlot;

    [Header("조합식")]
    public CraftRecipe[] recipes;

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
    public Sprite stickIcon;

    [Header("장비 아이콘")]
    public Sprite diamondHelmetIcon;
    public Sprite diamondChestplateIcon;
    public Sprite diamondLeggingsIcon;
    public Sprite diamondBootsIcon;
    public Sprite stonePickaxeIcon;
    public Sprite diamondPickaxeIcon;
    public Sprite stoneSwordIcon;
    public Sprite diamondSwordIcon;

    public UIInventorySlot[] slots => outerSlots;
    public int selectedIndex = -1;

    Dictionary<BlockType, Sprite> icons = new();

    UIInventorySlot draggingFrom;
    InventorySlotData draggingData;
    public bool isDragging = false;

    Inventory inventory;

    UIInventorySlot hoverSlot;

    bool hasPendingCraft = false;
    BlockType pendingCraftType;
    int pendingCraftCount;

    void Awake()
    {
        foreach (var s in outerSlots) s.Init(this);
        foreach (var s in innerSlots) s.Init(this);
        if (craftInputSlots != null) foreach (var s in craftInputSlots) s.Init(this);
        if (craftResultSlot != null) craftResultSlot.Init(this);
        if (armorSlots != null) foreach (var a in armorSlots) a.slot.Init(this);

        icons[BlockType.Dirt] = dirtIcon;
        icons[BlockType.Grass] = grassIcon;
        icons[BlockType.Water] = waterIcon;
        icons[BlockType.Beef] = beefIcon;
        icons[BlockType.Pork] = porkIcon;
        icons[BlockType.Stone] = stoneIcon;
        icons[BlockType.Wood] = woodIcon;
        icons[BlockType.Diamond] = diamondIcon;
        icons[BlockType.Stick] = stickIcon;

        icons[BlockType.DiamondHelmet] = diamondHelmetIcon;
        icons[BlockType.DiamondChestplate] = diamondChestplateIcon;
        icons[BlockType.DiamondLeggings] = diamondLeggingsIcon;
        icons[BlockType.DiamondBoots] = diamondBootsIcon;
        icons[BlockType.StonePickaxe] = stonePickaxeIcon;
        icons[BlockType.DiamondPickaxe] = diamondPickaxeIcon;
        icons[BlockType.StoneSword] = stoneSwordIcon;
        icons[BlockType.DiamondSword] = diamondSwordIcon;

        dragIcon.raycastTarget = false;
        dragIcon.gameObject.SetActive(false);

        ClearPendingCraft();
    }

    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
            inventory.OnChanged += RefreshAll;

        RefreshAll();
        EvaluateCrafting();
        ApplyArmorStat();
    }

    void Update()
    {
        if (dragIcon.gameObject.activeSelf)
            dragIcon.transform.position = Input.mousePosition;

        HandleNumberKeySelection();
    }

    public Sprite GetIcon(BlockType t)
        => icons.TryGetValue(t, out var s) ? s : null;

    // =========================================================
    // ★ FIX 핵심: 슬롯 재배치 제거 / 수량 동기화 방식
    // =========================================================
    public void RefreshAll()
    {
        if (isDragging) return;
        if (inventory == null) return;

        var invData = inventory.GetAll();

        // 1. 기존 슬롯 수량 동기화
        foreach (var s in outerSlots)
        {
            if (s.data.IsEmpty) continue;

            if (invData.TryGetValue(s.data.type, out int count))
            {
                s.data.count = count;
            }
            else
            {
                s.data.Clear();
            }
            s.Refresh();
        }

        foreach (var s in innerSlots)
        {
            if (s.data.IsEmpty) continue;

            if (invData.TryGetValue(s.data.type, out int count))
            {
                s.data.count = count;
            }
            else
            {
                s.data.Clear();
            }
            s.Refresh();
        }

        // 2. UI에 없는 새 아이템만 빈 슬롯에 추가
        foreach (var pair in invData)
        {
            if (HasSlotWithType(pair.Key)) continue;

            UIInventorySlot empty = GetFirstEmptySlot();
            if (empty == null) break;

            empty.data.Set(pair.Key, pair.Value);
            empty.Refresh();
        }

        // 3. 선택 슬롯 유효성 체크
        if (selectedIndex >= 0 &&
            selectedIndex < outerSlots.Length &&
            outerSlots[selectedIndex].data.IsEmpty)
        {
            Deselect();
        }
    }

    bool HasSlotWithType(BlockType t)
    {
        foreach (var s in outerSlots)
            if (!s.data.IsEmpty && s.data.type == t) return true;

        foreach (var s in innerSlots)
            if (!s.data.IsEmpty && s.data.type == t) return true;

        return false;
    }

    UIInventorySlot GetFirstEmptySlot()
    {
        foreach (var s in outerSlots)
            if (s.data.IsEmpty) return s;

        foreach (var s in innerSlots)
            if (s.data.IsEmpty) return s;

        return null;
    }

    // ================= 기존 코드 그대로 =================

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

    public void SetHoverSlot(UIInventorySlot slot) => hoverSlot = slot;
    public void ClearHoverSlot(UIInventorySlot slot)
    {
        if (hoverSlot == slot) hoverSlot = null;
    }

    bool IsCraftResultSlot(UIInventorySlot s) => craftResultSlot != null && s == craftResultSlot;
    bool IsCraftInputSlot(UIInventorySlot s)
    {
        if (craftInputSlots == null) return false;
        for (int i = 0; i < craftInputSlots.Length; i++)
            if (craftInputSlots[i] == s) return true;
        return false;
    }

    bool IsArmorSlot(UIInventorySlot s)
    {
        if (armorSlots == null) return false;
        for (int i = 0; i < armorSlots.Length; i++)
            if (armorSlots[i].slot == s) return true;
        return false;
    }

    UIArmorSlot GetArmorSlot(UIInventorySlot s)
    {
        if (armorSlots == null) return null;
        for (int i = 0; i < armorSlots.Length; i++)
            if (armorSlots[i].slot == s) return armorSlots[i];
        return null;
    }

    void BeginDragFromSlot(UIInventorySlot from)
    {
        if (from == null || from.data.IsEmpty) return;

        if (IsCraftResultSlot(from))
        {
            if (hasPendingCraft)
                ClaimPendingCraftAndStartDragging();
            return;
        }

        if (hasPendingCraft && IsCraftInputSlot(from))
            ClearPendingCraft();

        isDragging = true;
        draggingFrom = from;
        draggingData = new InventorySlotData();
        draggingData.Set(from.data.type, from.data.count);

        dragIcon.sprite = GetIcon(draggingData.type);
        dragIcon.gameObject.SetActive(true);

        from.data.Clear();
        from.Refresh();

        if (hasPendingCraft)
            ClearPendingCraft();
    }

    void CancelDragToOrigin()
    {
        if (draggingFrom == null || draggingData == null) return;

        draggingFrom.data.Set(draggingData.type, draggingData.count);
        draggingFrom.Refresh();

        draggingFrom = null;
        draggingData = null;
        isDragging = false;

        dragIcon.gameObject.SetActive(false);

        EvaluateCrafting();
        ApplyArmorStat();
    }

    void EndDragTo(UIInventorySlot to, bool placeOne)
    {
        bool fromMain = IsMainInventorySlot(draggingFrom);
        bool toMain = IsMainInventorySlot(to);
        bool fromCraft = IsCraftInputSlot(draggingFrom);
        bool toCraft = IsCraftInputSlot(to);

        int moveAmount = placeOne ? 1 : draggingData.count;

        if (toCraft && fromMain)
        {
            if (inventory == null) { CancelDragToOrigin(); return; }

            if (!inventory.Consume(draggingData.type, moveAmount))
            {
                CancelDragToOrigin();
                return;
            }
        }

        if (toMain && fromCraft)
        {
            inventory?.Add(draggingData.type, moveAmount);
        }

        if (toCraft && fromCraft)
        {
        }

        if (toMain && fromMain)
        {
        }

        if (draggingData == null || draggingFrom == null)
            return;

        if (to == null || to == draggingFrom)
        {
            CancelDragToOrigin();
            return;
        }

        if (IsCraftResultSlot(to))
        {
            CancelDragToOrigin();
            return;
        }

        if (IsArmorSlot(to))
        {
            var a = GetArmorSlot(to);
            if (a == null || !a.CanAccept(draggingData.type))
            {
                CancelDragToOrigin();
                return;
            }

            if (to.data.IsEmpty)
            {
                if (placeOne || draggingData.count == 1)
                {
                    to.data.Set(draggingData.type, 1);
                    draggingData.count -= 1;

                    to.Refresh();

                    if (draggingData.count <= 0)
                    {
                        FinishDrag();
                    }
                    else
                    {
                        dragIcon.sprite = GetIcon(draggingData.type);
                        draggingFrom.Refresh();
                    }

                    ApplyArmorStat();
                    EvaluateCrafting();
                    return;
                }

                CancelDragToOrigin();
                return;
            }

            CancelDragToOrigin();
            return;
        }

        if (to.data.IsEmpty)
        {
            if (placeOne)
            {
                to.data.Set(draggingData.type, 1);
                draggingData.count -= 1;
                to.Refresh();

                if (draggingData.count <= 0)
                    FinishDrag();
                else
                    dragIcon.sprite = GetIcon(draggingData.type);

                EvaluateCrafting();
                ApplyArmorStat();
                return;
            }

            to.data.Set(draggingData.type, draggingData.count);
            to.Refresh();
            FinishDrag();

            EvaluateCrafting();
            ApplyArmorStat();
            return;
        }

        if (to.data.type == draggingData.type)
        {
            int moved = placeOne ? 1 : draggingData.count;
            to.data.count += moved;
            draggingData.count -= moved;

            to.Refresh();

            if (draggingData.count <= 0)
                FinishDrag();
            else
                dragIcon.sprite = GetIcon(draggingData.type);

            EvaluateCrafting();
            ApplyArmorStat();
            return;
        }

        if (placeOne)
        {
            CancelDragToOrigin();
            return;
        }

        var t = to.data.type;
        var c = to.data.count;

        to.data.Set(draggingData.type, draggingData.count);
        draggingFrom.data.Set(t, c);

        to.Refresh();
        draggingFrom.Refresh();

        FinishDrag();

        EvaluateCrafting();
        ApplyArmorStat();
    }

    void FinishDrag()
    {
        draggingFrom = null;
        draggingData = null;
        isDragging = false;
        dragIcon.gameObject.SetActive(false);
    }

    public void OnSlotLeftClick(UIInventorySlot slot)
    {
        if (slot == null) return;

        if (IsCraftResultSlot(slot))
        {
            if (hasPendingCraft)
                ClaimPendingCraftAndStartDragging();
            return;
        }

        if (!isDragging)
        {
            if (hasPendingCraft && IsCraftInputSlot(slot))
                ClearPendingCraft();

            if (!slot.data.IsEmpty)
                BeginDragFromSlot(slot);
            return;
        }

        if (hasPendingCraft)
            ClearPendingCraft();

        EndDragTo(slot, placeOne: false);
    }

    public void OnSlotRightClick(UIInventorySlot slot)
    {
        if (!isDragging || draggingData == null) return;
        if (slot == null) return;

        if (hasPendingCraft)
            ClearPendingCraft();

        EndDragTo(slot, placeOne: true);
    }

    void EvaluateCrafting()
    {
        if (craftInputSlots == null || craftInputSlots.Length != 9 || craftResultSlot == null)
            return;

        if (isDragging)
        {
            ClearPendingCraft();
            return;
        }

        CraftRecipe matched = null;

        if (recipes != null)
        {
            for (int r = 0; r < recipes.Length; r++)
            {
                var rec = recipes[r];
                if (rec == null || rec.grid == null || rec.grid.Length != 9) continue;

                bool ok = true;
                for (int i = 0; i < 9; i++)
                {
                    var cell = rec.grid[i];
                    var slot = craftInputSlots[i];

                    if (!cell.hasItem)
                    {
                        if (!slot.data.IsEmpty) { ok = false; break; }
                    }
                    else
                    {
                        if (slot.data.IsEmpty) { ok = false; break; }
                        if (slot.data.type != cell.type) { ok = false; break; }
                        if (slot.data.count < 1) { ok = false; break; }
                    }
                }

                if (ok) { matched = rec; break; }
            }
        }

        if (matched == null)
        {
            ClearPendingCraft();
            return;
        }

        hasPendingCraft = true;
        pendingCraftType = matched.resultType;
        pendingCraftCount = Mathf.Max(1, matched.resultCount);

        craftResultSlot.data.Set(pendingCraftType, pendingCraftCount);
        craftResultSlot.Refresh();
    }

    void ClearPendingCraft()
    {
        hasPendingCraft = false;
        pendingCraftType = default;
        pendingCraftCount = 0;

        if (craftResultSlot != null)
        {
            craftResultSlot.data.Clear();
            craftResultSlot.Refresh();
        }
    }

    void ClaimPendingCraftAndStartDragging()
    {
        if (!hasPendingCraft) return;
        if (craftInputSlots == null || craftInputSlots.Length != 9) return;

        var typeToGive = pendingCraftType;
        var countToGive = pendingCraftCount;

        if (countToGive <= 0) return;

        for (int i = 0; i < 9; i++)
        {
            if (!craftInputSlots[i].data.IsEmpty)
            {
                craftInputSlots[i].data.count -= 1;
                if (craftInputSlots[i].data.count <= 0)
                    craftInputSlots[i].data.Clear();
                craftInputSlots[i].Refresh();
            }
        }

        ClearPendingCraft();

        isDragging = true;
        draggingFrom = craftResultSlot;
        draggingData = new InventorySlotData();
        draggingData.Set(typeToGive, countToGive);

        dragIcon.sprite = GetIcon(draggingData.type);
        dragIcon.gameObject.SetActive(true);

        inventory?.Add(typeToGive, countToGive);

        RefreshAll();
        EvaluateCrafting();
    }

    void ApplyArmorStat()
    {
        float armor = 0f;

        if (armorSlots != null)
        {
            foreach (var a in armorSlots)
            {
                if (a == null || a.slot == null) continue;
                if (!a.slot.data.IsEmpty)
                    armor += ItemStatData.GetArmorValue(a.slot.data.type);
            }
        }

        var pc = FindObjectOfType<PlayerController>();
        pc?.SetArmor(armor);
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.OnChanged -= RefreshAll;
    }

    bool IsMainInventorySlot(UIInventorySlot s)
    {
        if (s == null) return false;
        for (int i = 0; i < outerSlots.Length; i++) if (outerSlots[i] == s) return true;
        for (int i = 0; i < innerSlots.Length; i++) if (innerSlots[i] == s) return true;
        return false;
    }

    int GetReservedCountInCraft(BlockType type)
    {
        if (craftInputSlots == null) return 0;
        int sum = 0;
        for (int i = 0; i < craftInputSlots.Length; i++)
            if (!craftInputSlots[i].data.IsEmpty && craftInputSlots[i].data.type == type)
                sum += craftInputSlots[i].data.count;
        return sum;
    }

    void HandleNumberKeySelection()
    {
        if (isDragging) return;

        for (int i = 0; i < outerSlots.Length && i < 9; i++)
        {
            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(key))
            {
                ToggleSelectSlot(i);
                return;
            }
        }
    }

    public BlockType? GetSelectedTypeOnly()
    {
        if (selectedIndex < 0 || selectedIndex >= outerSlots.Length)
            return null;

        return outerSlots[selectedIndex].data.type;
    }
}