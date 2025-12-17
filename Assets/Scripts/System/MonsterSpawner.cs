using UnityEngine;
using System.Collections;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Boundary")]
    public Transform boundaryRoot;

    [Header("Spawn settings")]
    public AnimalTypeData[] spawnTypes;
    public float spawnIntervalMin = 3f;
    public float spawnIntervalMax = 8f;
    [Range(0f, 1f)]
    public float spawnChance = 0.8f;

    [Header("Spawn limit")]
    public int maxTotalMonsters = 50;

    [Header("UI")]
    public GameObject gameClearPanel;

    [Header("Spawn height scan")]
    public float scanStartY = 50f;
    public LayerMask blockMask = ~0;

    Bounds boundaryBounds;

    int totalSpawned = 0;
    int aliveMonsters = 0;
    bool cleared = false;

    void Start()
    {
        if (boundaryRoot == null)
        {
            Debug.LogError("[MonsterSpawner] boundaryRoot is null!");
            enabled = false;
            return;
        }

        if (gameClearPanel != null)
            gameClearPanel.SetActive(false);

        CalculateBounds();
        StartCoroutine(SpawnerLoop());
    }

    void CalculateBounds()
    {
        var colliders = boundaryRoot.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            boundaryBounds = new Bounds(boundaryRoot.position, boundaryRoot.localScale);
            return;
        }

        boundaryBounds = colliders[0].bounds;
        foreach (var c in colliders)
            boundaryBounds.Encapsulate(c.bounds);
    }

    IEnumerator SpawnerLoop()
    {
        while (!cleared)
        {
            if (totalSpawned >= maxTotalMonsters)
                yield break;

            float wait = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(wait);

            if (Random.value > spawnChance)
                continue;

            TrySpawnOne();
        }
    }

    void TrySpawnOne()
    {
        if (totalSpawned >= maxTotalMonsters)
            return;

        if (spawnTypes == null || spawnTypes.Length == 0)
            return;

        float x = Random.Range(boundaryBounds.min.x, boundaryBounds.max.x);
        float z = Random.Range(boundaryBounds.min.z, boundaryBounds.max.z);

        Vector3 start = new Vector3(x, boundaryBounds.max.y + scanStartY, z);

        if (!Physics.Raycast(start, Vector3.down, out RaycastHit hit,
            boundaryBounds.size.y + scanStartY + 5f, blockMask))
            return;

        var block = hit.collider.GetComponent<Block>();
        if (block == null || block.type == BlockType.Water)
            return;

        Vector3 spawnPos = hit.collider.transform.position + Vector3.up;
        spawnPos = new Vector3(
            Mathf.Round(spawnPos.x),
            Mathf.Round(spawnPos.y),
            Mathf.Round(spawnPos.z));

        if (!boundaryBounds.Contains(spawnPos))
            return;

        var at = spawnTypes[Random.Range(0, spawnTypes.Length)];
        GameObject go = Instantiate(
            at.prefab,
            spawnPos,
            at.useRandomRotation
                ? Quaternion.Euler(0, Random.Range(0f, 360f), 0)
                : Quaternion.identity);

        var monster = go.GetComponent<MonsterBase>();
        if (monster != null)
        {
            monster.data = at;
            monster.SetSpawner(this);

            totalSpawned++;
            aliveMonsters++;
        }
    }

    public void OnMonsterDied()
    {
        aliveMonsters = Mathf.Max(0, aliveMonsters - 1);

        if (!cleared &&
            totalSpawned >= maxTotalMonsters &&
            aliveMonsters == 0)
        {
            ClearGame();
        }
    }

    void ClearGame()
    {
        cleared = true;

        Time.timeScale = 0f;

        if (gameClearPanel != null)
            gameClearPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[MonsterSpawner] GAME CLEAR!");
    }
}
