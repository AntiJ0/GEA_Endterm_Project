using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Dictionary<BlockType, int> items = new();

    public event Action OnChanged;

    public void Add(BlockType type, int amount = 1)
    {
        if (!items.ContainsKey(type))
            items[type] = 0;

        items[type] += amount;

        OnChanged?.Invoke();
    }

    public bool Consume(BlockType type, int amount = 1)
    {
        if (!items.ContainsKey(type)) return false;
        if (items[type] < amount) return false;

        items[type] -= amount;
        if (items[type] <= 0)
            items.Remove(type);

        OnChanged?.Invoke();
        return true;
    }

    public int GetCount(BlockType type)
    {
        return items.ContainsKey(type) ? items[type] : 0;
    }

    public Dictionary<BlockType, int> GetAll()
    {
        return items;
    }
}