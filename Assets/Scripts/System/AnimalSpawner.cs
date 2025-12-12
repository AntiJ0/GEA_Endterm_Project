using System.Collections;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
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
        Debug.Log("[AnimalSpawner] Start() 호출됨.");

        if (boundaryRoot == null)
        {
            Debug.LogError("[AnimalSpawner] boundaryRoot is null! Boundary 오브젝트가 할당 안됨.");
            enabled = false;
            return;
        }

        Debug.Log("[AnimalSpawner] 경계 계산 중...");
        CalculateBounds();
        Debug.Log("[AnimalSpawner] 경계 계산 완료: " + boundaryBounds);

        Debug.Log("[AnimalSpawner] SpawnerLoop 시작.");
        StartCoroutine(SpawnerLoop());
    }

    void CalculateBounds()
    {
        var colliders = boundaryRoot.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            Debug.LogWarning("[AnimalSpawner] 경계를 계산할 Collider가 없음. Transform scale 로 Bounds 생성.");
            var t = boundaryRoot;
            var s = t.localScale;
            boundaryBounds = new Bounds(t.position, s);
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
            Debug.Log($"[AnimalSpawner] {wait:F1}초 대기 후 스폰 시도 예정.");

            yield return new WaitForSeconds(wait);

            float roll = Random.value;
            Debug.Log($"[AnimalSpawner] 스폰 확률 체크: roll={roll:F2}, 필요<={spawnChance:F2}");

            if (roll > spawnChance)
            {
                Debug.Log("[AnimalSpawner] 스폰 실패: 확률 미달.");
                continue;
            }

            TrySpawnOne();
        }
    }

    void TrySpawnOne()
    {
        Debug.Log("[AnimalSpawner] TrySpawnOne() 호출됨.");

        if (spawnTypes == null || spawnTypes.Length == 0)
        {
            Debug.LogError("[AnimalSpawner] spawnTypes 배열이 비어 있음!");
            return;
        }

        float x = Random.Range(boundaryBounds.min.x + 0.5f, boundaryBounds.max.x - 0.5f);
        float z = Random.Range(boundaryBounds.min.z + 0.5f, boundaryBounds.max.z - 0.5f);
        Vector3 start = new Vector3(x, boundaryBounds.max.y + scanStartY, z);

        Debug.Log($"[AnimalSpawner] Raycast 시작점: {start}");

        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, boundaryBounds.size.y + scanStartY + 5f, blockMask))
        {
            Debug.Log("[AnimalSpawner] Raycast 히트! hit=" + hit.collider.name);

            var block = hit.collider.GetComponent<Block>();
            if (block == null)
            {
                Debug.LogWarning("[AnimalSpawner] Raycast는 맞았지만 Block 컴포넌트가 없음 → 스폰 취소");
                return;
            }

            Vector3 spawnPos = hit.collider.transform.position + Vector3.up * 1f;
            spawnPos = new Vector3(Mathf.Round(spawnPos.x), Mathf.Round(spawnPos.y), Mathf.Round(spawnPos.z));

            Debug.Log("[AnimalSpawner] 계산된 스폰 위치: " + spawnPos);

            if (!boundaryBounds.Contains(spawnPos))
            {
                Debug.LogWarning("[AnimalSpawner] 스폰 위치가 경계 밖임 → 스폰 취소");
                return;
            }

            var at = spawnTypes[Random.Range(0, spawnTypes.Length)];
            if (at == null)
            {
                Debug.LogError("[AnimalSpawner] 선택된 AnimalTypeData 가 null!");
                return;
            }
            if (at.prefab == null)
            {
                Debug.LogError("[AnimalSpawner] AnimalTypeData 의 prefab 이 null!");
                return;
            }

            Debug.Log($"[AnimalSpawner] 프리팹 {at.prefab.name} 스폰!");

            GameObject go = Instantiate(
                at.prefab,
                spawnPos,
                at.useRandomRotation ? Quaternion.Euler(0, Random.Range(0f, 360f), 0) : Quaternion.identity);

            var ab = go.GetComponent<AnimalBase>();
            if (ab == null)
            {
                Debug.LogWarning("[AnimalSpawner] 스폰된 오브젝트에 AnimalBase 없음.");
            }
            else
            {
                ab.data = at;
                Debug.Log("[AnimalSpawner] AnimalBase 데이터 설정 완료.");
            }
        }
        else
        {
            Debug.LogWarning("[AnimalSpawner] Raycast가 아무것도 맞추지 못함 → 지형 없음?");
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (boundaryRoot == null) return;

        Gizmos.color = Color.green;
        var colliders = boundaryRoot.GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
            Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
    }
#endif
}