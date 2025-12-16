using UnityEngine;

public class UIArmorSlot : MonoBehaviour
{
    public ArmorSlotType slotType;
    public UIInventorySlot slot;

    public bool CanAccept(BlockType t)
    {
        return slotType switch
        {
            ArmorSlotType.Helmet => t == BlockType.DiamondHelmet,
            ArmorSlotType.Chest => t == BlockType.DiamondChestplate,
            ArmorSlotType.Legs => t == BlockType.DiamondLeggings,
            ArmorSlotType.Boots => t == BlockType.DiamondBoots,
            _ => false
        };
    }
}