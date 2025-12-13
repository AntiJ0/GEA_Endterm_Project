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

    [Header("Spawn height scan")]
    public float scanStartY = 50f;
    public LayerMask blockMask = ~0;

    Bounds boundaryBounds;

    void Start()
    {
        Debug.Log("[MonsterSpawner] Start() 호출됨.");

        if (boundaryRoot == null)
        {
            Debug.LogError("[MonsterSpawner] boundaryRoot is null!");
            enabled = false;
            return;
        }

        Debug.Log("[MonsterSpawner] 경계 계산 중...");
        CalculateBounds();
        Debug.Log("[MonsterSpawner] 경계 계산 완료: " + boundaryBounds);

        Debug.Log("[MonsterSpawner] SpawnerLoop 시작.");
        StartCoroutine(SpawnerLoop());
    }

    void CalculateBounds()
    {
        var colliders = boundaryRoot.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            Debug.LogWarning("[MonsterSpawner] Collider 없음 → Transform scale 사용");
            boundaryBounds = new Bounds(boundaryRoot.position, boundaryRoot.localScale);
            return;
        }

        boundaryBounds = colliders[0].bounds;
        foreach (var c in colliders)
            boundaryBounds.Encapsulate(c.bounds);
    }

    IEnumerator SpawnerLoop()
    {
        while (true)
        {
            float wait = Random.Range(spawnIntervalMin, spawnIntervalMax);
            Debug.Log($"[MonsterSpawner] {wait:F1}초 대기 후 스폰 시도");

            yield return new WaitForSeconds(wait);

            float roll = Random.value;
            Debug.Log($"[MonsterSpawner] 확률 체크 roll={roll:F2}");

            if (roll > spawnChance)
            {
                Debug.Log("[MonsterSpawner] 스폰 실패");
                continue;
            }

            TrySpawnOne();
        }
    }

    void TrySpawnOne()
    {
        Debug.Log("[MonsterSpawner] TrySpawnOne()");

        if (spawnTypes == null || spawnTypes.Length == 0)
        {
            Debug.LogError("[MonsterSpawner] spawnTypes 비어있음");
            return;
        }

        float x = Random.Range(boundaryBounds.min.x, boundaryBounds.max.x);
        float z = Random.Range(boundaryBounds.min.z, boundaryBounds.max.z);

        Vector3 start = new Vector3(x, boundaryBounds.max.y + scanStartY, z);

        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit,
            boundaryBounds.size.y + scanStartY + 5f, blockMask))
        {
            var block = hit.collider.GetComponent<Block>();
            if (block == null || block.type == BlockType.Water)
            {
                Debug.Log("[MonsterSpawner] Block 아님 or 물 → 취소");
                return;
            }

            Vector3 spawnPos = hit.collider.transform.position + Vector3.up;
            spawnPos = new Vector3(
                Mathf.Round(spawnPos.x),
                Mathf.Round(spawnPos.y),
                Mathf.Round(spawnPos.z));

            if (!boundaryBounds.Contains(spawnPos))
            {
                Debug.Log("[MonsterSpawner] 경계 밖 → 취소");
                return;
            }

            var at = spawnTypes[Random.Range(0, spawnTypes.Length)];
            GameObject go = Instantiate(
                at.prefab,
                spawnPos,
                at.useRandomRotation
                    ? Quaternion.Euler(0, Random.Range(0f, 360f), 0)
                    : Quaternion.identity);

            var ab = go.GetComponent<AnimalBase>();
            if (ab != null)
                ab.data = at;

            Debug.Log("[MonsterSpawner] 몬스터 스폰 완료");
        }
    }
}