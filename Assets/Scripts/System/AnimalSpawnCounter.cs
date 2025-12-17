using UnityEngine;

public class AnimalSpawnCounter : MonoBehaviour
{
    AnimalSpawner spawner;

    public void Init(AnimalSpawner s)
    {
        spawner = s;
    }

    void OnDestroy()
    {
        if (spawner != null)
            spawner.NotifyAnimalDead();
    }
}