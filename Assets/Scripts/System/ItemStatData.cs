using UnityEngine;

public static class ItemStatData
{
    public static float GetArmorValue(BlockType t)
    {
        return t switch
        {
            BlockType.DiamondHelmet => 10f,
            BlockType.DiamondChestplate => 18f,
            BlockType.DiamondLeggings => 14f,
            BlockType.DiamondBoots => 8f,
            _ => 0f
        };
    }

    public static int GetBlockDamage(BlockType t)
    {
        return t switch
        {
            BlockType.StonePickaxe => 4,
            BlockType.DiamondPickaxe => 8,
            _ => 1
        };
    }

    public static int GetEntityDamage(BlockType t)
    {
        return t switch
        {
            BlockType.StoneSword => 5,
            BlockType.DiamondSword => 8,
            _ => 1
        };
    }

    public static bool IsArmor(BlockType t)
    {
        return GetArmorValue(t) > 0f;
    }

    public static bool IsPickaxe(BlockType t)
    {
        return t == BlockType.StonePickaxe || t == BlockType.DiamondPickaxe;
    }

    public static bool IsSword(BlockType t)
    {
        return t == BlockType.StoneSword || t == BlockType.DiamondSword;
    }
}