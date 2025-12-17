using System.Collections;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    public Transform boundaryRoot;
    public AnimalTypeData[] spawnTypes;
    public float spawnIntervalMin = 3f;
    public float spawnIntervalMax = 8f;
    [Range(0f, 1f)]
    public float spawnChance = 0.8f;

    public float scanStartY = 50f;
    public LayerMask blockMask = ~0;

    public int maxAliveAnimals = 10;

    Bounds boundaryBounds;
    int currentAlive;

    void Start()
    {
        if (boundaryRoot == null)
        {
            enabled = false;
            return;
        }

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
        while (true)
        {
            float wait = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(wait);

            if (currentAlive >= maxAliveAnimals)
                continue;

            if (Random.value > spawnChance)
                continue;

            TrySpawnOne();
        }
    }

    void TrySpawnOne()
    {
        float x = Random.Range(boundaryBounds.min.x + 0.5f, boundaryBounds.max.x - 0.5f);
        float z = Random.Range(boundaryBounds.min.z + 0.5f, boundaryBounds.max.z - 0.5f);
        Vector3 start = new Vector3(x, boundaryBounds.max.y + scanStartY, z);

        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit,
            boundaryBounds.size.y + scanStartY + 5f, blockMask))
        {
            var block = hit.collider.GetComponent<Block>();
            if (block == null)
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

            currentAlive++;

            go.AddComponent<AnimalSpawnCounter>().Init(this);
        }
    }

    public void NotifyAnimalDead()
    {
        currentAlive = Mathf.Max(0, currentAlive - 1);
    }
}