using UnityEngine;

public class PlayerHarvester : MonoBehaviour
{
    public float rayDistance = 5f;
    public LayerMask hitMask = ~0;
    public int toolDamage = 1;
    [Tooltip("초 단위 공격 쿨다운")]
    public float hitCooldown = 0.5f; // 0.5초 기본

    private float lastAttackTime = -999f;
    private Camera _cam;
    public Inventory inventory;

    void Awake()
    {
        _cam = Camera.main;
        if (inventory == null) inventory = GetComponent<Inventory>() ?? gameObject.AddComponent<Inventory>();
    }

    void Update()
    {
        var player = GetComponent<PlayerController>();
        if (player != null && player.IsEating) return;

        if (Input.GetMouseButton(0))
        {
            if (Time.time >= lastAttackTime + hitCooldown)
            {
                lastAttackTime = Time.time;
                TryHit();
            }
        }
    }

    void TryHit()
    {
        if (_cam == null) return;

        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out var hit, rayDistance, hitMask))
        {
            // 블록 채굴 우선
            var block = hit.collider.GetComponent<Block>();
            if (block != null)
            {
                block.Hit(toolDamage, inventory);
                Debug.Log("[PlayerHarvester] Hit Block: " + block.name);
                return;
            }

            // 동물 공격
            var animal = hit.collider.GetComponent<AnimalBase>();
            if (animal != null)
            {
                animal.TakeDamage(toolDamage, transform.position);
                Debug.Log("[PlayerHarvester] Hit Animal: " + animal.name + " dmg=" + toolDamage);
                return;
            }
        }
    }
}