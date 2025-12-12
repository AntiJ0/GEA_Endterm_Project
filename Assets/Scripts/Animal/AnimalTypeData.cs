using UnityEngine;

[CreateAssetMenu(menuName = "Animal/AnimalTypeData")]
public class AnimalTypeData : ScriptableObject
{
    public string animalName;
    public GameObject prefab;

    [Header("Stats")]
    public int maxHp = 5;
    public float walkSpeed = 2f;
    public float fleeSpeed = 4f;

    [Header("Behavior")]
    public bool fleeWhenAttacked = true;
    public bool fleeWhenPlayerNear = false;
    public float fleeDistance = 6f;

    [Header("Spawn")]
    public bool useRandomRotation = true;

    [Header("Drops (uses BlockType enum)")]
    public BlockType dropType = BlockType.Dirt; 
    public int dropMin = 0;
    public int dropMax = 2;
    [Range(0f, 1f)]
    public float dropChance = 0.75f;
}