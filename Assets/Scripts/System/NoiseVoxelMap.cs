using System.Collections.Generic;
using UnityEngine;

public class NoiseVoxelMap : MonoBehaviour
{
    [Header("Block Prefabs")]
    public GameObject blockPrefabDirt;
    public GameObject blockPrefabGrass;
    public GameObject blockPrefabWater;
    public GameObject blockPrefabBedrock;
    public GameObject blockPrefabStone;
    public GameObject blockPrefabWood;
    public GameObject blockPrefabLeaves;
    public GameObject blockPrefabDiamond;

    [Header("Map Settings")]
    public int width = 20;
    public int depth = 20;
    public int maxHeight = 16;
    public int waterLevel = 4;
    [SerializeField] float noiseScale = 20f;

    const int DIAMOND_COUNT = 50;

    List<Vector3Int> stonePositions = new();
    List<Vector2Int> treePositions = new();

    void Start()
    {
        float offsetX = Random.Range(-9999f, 9999f);
        float offsetZ = Random.Range(-9999f, 9999f);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                PlaceBedrock(x, 0, z);

                float nx = (x + offsetX) / noiseScale;
                float nz = (z + offsetZ) / noiseScale;
                float noise = Mathf.PerlinNoise(nx, nz);
                int h = Mathf.FloorToInt(noise * maxHeight);
                if (h < 1) h = 1;

                for (int y = 1; y < h; y++)
                {
                    if (y <= h - 2)
                    {
                        stonePositions.Add(new Vector3Int(x, y, z));
                        PlaceStone(x, y, z);
                    }
                    else
                    {
                        PlaceDirt(x, y, z);
                    }
                }

                PlaceGrass(x, h, z);

                for (int y = h + 1; y <= waterLevel; y++)
                    PlaceWater(x, y, z);

                TrySpawnTree(x, h + 1, z);
            }
        }

        SpawnDiamonds();
    }

    void PlaceBedrock(int x, int y, int z)
    {
        var go = Instantiate(blockPrefabBedrock, new Vector3(x, y, z), Quaternion.identity, transform);
        var b = go.GetComponent<Block>();
        b.type = BlockType.Bedrock;
        b.mineable = false;
        b.dropCount = 0;
    }

    void PlaceStone(int x, int y, int z)
    {
        var go = Instantiate(blockPrefabStone, new Vector3(x, y, z), Quaternion.identity, transform);
        go.GetComponent<Block>().type = BlockType.Stone;
    }

    void PlaceDirt(int x, int y, int z)
    {
        var go = Instantiate(blockPrefabDirt, new Vector3(x, y, z), Quaternion.identity, transform);
        go.GetComponent<Block>().type = BlockType.Dirt;
    }

    void PlaceGrass(int x, int y, int z)
    {
        var go = Instantiate(blockPrefabGrass, new Vector3(x, y, z), Quaternion.identity, transform);
        go.GetComponent<Block>().type = BlockType.Grass;
    }

    void PlaceWater(int x, int y, int z)
    {
        var go = Instantiate(blockPrefabWater, new Vector3(x, y, z), Quaternion.identity, transform);
        var b = go.GetComponent<Block>();
        b.type = BlockType.Water;
        b.mineable = false;
    }

    void TrySpawnTree(int x, int y, int z)
    {
        if (y - 1 <= waterLevel) return;

        RaycastHit hit;
        if (Physics.Raycast(
            new Vector3(x, y, z),
            Vector3.down,
            out hit,
            1.1f))
        {
            var block = hit.collider.GetComponent<Block>();
            if (block == null || block.type != BlockType.Grass)
                return;
        }
        else return;

        if (Random.value > 0.015f) return;

        Vector2Int pos = new(x, z);
        foreach (var p in treePositions)
        {
            if (Vector2Int.Distance(p, pos) < 5)
                return;
        }

        SpawnTree(x, y, z);
        treePositions.Add(pos);
    }

    void SpawnTree(int x, int y, int z)
    {
        int height = Random.Range(3, 5);

        for (int i = 0; i < height; i++)
            PlaceWood(x, y + i, z);

        for (int i = height - 2; i < height; i++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0 && i != height - 1) continue;
                    PlaceLeaves(x + dx, y + i, z + dz);
                }
            }
        }
    }

    void PlaceWood(int x, int y, int z)
    {
        var go = Instantiate(blockPrefabWood, new Vector3(x, y, z), Quaternion.identity, transform);
        go.GetComponent<Block>().type = BlockType.Wood;
    }

    void PlaceLeaves(int x, int y, int z)
    {
        var go = Instantiate(blockPrefabLeaves, new Vector3(x, y, z), Quaternion.identity, transform);
        var b = go.GetComponent<Block>();
        b.type = BlockType.Leaves;
        b.dropCount = 0;
    }

    void SpawnDiamonds()
    {
        List<Vector3Int> candidates = new(stonePositions);

        for (int i = 0; i < DIAMOND_COUNT && candidates.Count > 0; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            var pos = candidates[idx];
            candidates.RemoveAt(idx);

            PlaceDiamond(pos.x, pos.y, pos.z);
        }
    }

    void PlaceDiamond(int x, int y, int z)
    {
        var go = Instantiate(blockPrefabDiamond, new Vector3(x, y, z), Quaternion.identity, transform);
        go.GetComponent<Block>().type = BlockType.Diamond;
    }
}