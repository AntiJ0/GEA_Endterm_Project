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
        var panel = FindObjectOfType<InventoryPanelController>();
        if (panel != null && panel.panel.activeSelf)
            return;

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

        int blockDamage = toolDamage;
        int entityDamage = toolDamage;

        var uiInv = FindObjectOfType<UIInventory>();
        var bt = uiInv?.GetSelectedBlockType();

        if (bt != null)
        {
            if (ItemStatData.IsPickaxe(bt.Value))
                blockDamage = ItemStatData.GetBlockDamage(bt.Value);

            if (ItemStatData.IsSword(bt.Value))
                entityDamage = ItemStatData.GetEntityDamage(bt.Value);
        }

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (!Physics.Raycast(ray, out var hit, rayDistance, hitMask)) return;

        var animal = hit.collider.GetComponentInParent<AnimalBase>();
        if (animal != null)
        {
            animal.TakeDamage(entityDamage, transform.position);
            return;
        }

        if (!instantOnlyMonster && hit.collider.TryGetComponent(out Block block))
        {
            block.Hit(blockDamage, inventory);
        }
    }
}