[System.Serializable]
public class InventorySlotData
{
    public BlockType type;
    public int count;

    public bool IsEmpty => count <= 0;

    public void Clear()
    {
        type = default;
        count = 0;
    }

    public void Set(BlockType t, int c)
    {
        type = t;
        count = c;
    }
}