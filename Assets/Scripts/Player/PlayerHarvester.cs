using UnityEngine;

public class PlayerHarvester : MonoBehaviour
{
    public float rayDistance = 5f;
    public LayerMask hitMask = ~0;
    public int toolDamage = 1;
    public float hitCooldown = 0.5f;

    private bool holding;
    private float nextHitTime;

    private Camera _cam;
    public Inventory inventory;

    void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
        if (_cam == null)
            _cam = Camera.main;

        if (inventory == null)
            inventory = GetComponent<Inventory>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            holding = true;
            nextHitTime = Time.time + hitCooldown;

            TryHit(instantOnlyMonster: true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            holding = false;
        }

        if (holding && Time.time >= nextHitTime)
        {
            TryHit(instantOnlyMonster: false);
            nextHitTime = Time.time + hitCooldown;
        }
    }

    void TryHit(bool instantOnlyMonster)
    {
        if (_cam == null) return;

        Vector3 origin = _cam.transform.position;
        Vector3 dir = _cam.transform.forward;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            dir,
            rayDistance,
            hitMask,
            QueryTriggerInteraction.Collide
        );

        if (hits.Length == 0)
            return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            AnimalBase animal = hit.collider.GetComponentInParent<AnimalBase>();
            if (animal != null)
            {
                animal.TakeDamage(toolDamage, transform.position);
                Debug.Log("[Harvester] Hit Animal");
                return;
            }

            if (instantOnlyMonster)
                continue;

            if (hit.collider.TryGetComponent(out Block block))
            {
                block.Hit(toolDamage, inventory);
                Debug.Log("[Harvester] Hit Block");
                return;
            }
        }
    }
}