using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Craft Recipe")]
public class CraftRecipe : ScriptableObject
{
    [System.Serializable]
    public struct Cell
    {
        public bool hasItem;
        public BlockType type;
    }

    public Cell[] grid = new Cell[9];

    public BlockType resultType;
    public int resultCount = 1;
}