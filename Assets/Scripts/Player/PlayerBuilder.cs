using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerBuilder : MonoBehaviour
{
    [Header("레이/사거리")]
    public float rayDistance = 5f;
    public LayerMask hitMask = ~0;

    [Header("블록 프리팹")]
    public GameObject dirtPrefab;
    public GameObject grassPrefab;
    public GameObject waterPrefab;
    public GameObject stonePrefab;
    public GameObject woodPrefab;
    public GameObject diamondPrefab;

    [Header("Ghost")]
    public GameObject ghostPrefab;
    public Vector3 blockHalfExtents = Vector3.one * 0.45f;

    Camera cam;
    UIInventory uiInv;
    Inventory inventory;
    InventoryPanelController inventoryPanel;

    GameObject ghost;

    void Awake()
    {
        cam = Camera.main;
        uiInv = FindObjectOfType<UIInventory>();
        inventory = FindObjectOfType<Inventory>();
        inventoryPanel = FindObjectOfType<InventoryPanelController>();

        if (ghostPrefab != null)
        {
            ghost = Instantiate(ghostPrefab);
            ghost.SetActive(false);

            if (ghost.TryGetComponent(out Collider c)) Destroy(c);
            if (ghost.TryGetComponent(out Rigidbody r)) Destroy(r);
        }
    }

    void Update()
    {
        if (inventoryPanel != null && inventoryPanel.panel.activeSelf)
        {
            SetGhost(false);
            return;
        }

        UpdateGhost();

        if (Input.GetMouseButtonDown(1))
            TryPlaceBlock();
    }

    void UpdateGhost()
    {
        if (ghost == null)
            return;

        if (!TryGetPlacePosition(out Vector3 pos))
        {
            SetGhost(false);
            return;
        }

        ghost.transform.position = pos;
        SetGhost(true);
    }

    void SetGhost(bool active)
    {
        if (ghost != null)
            ghost.SetActive(active);
    }

    void TryPlaceBlock()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        var bt = uiInv.GetSelectedTypeOnly();
        if (!IsPlaceableBlock(bt))
            return;

        if (inventory.GetCount(bt.Value) <= 0)
            return;

        if (!TryGetPlacePosition(out Vector3 pos))
            return;

        if (!inventory.Consume(bt.Value, 1))
            return;

        Instantiate(GetPrefabForBlockType(bt.Value), pos, Quaternion.identity);
    }

    bool TryGetPlacePosition(out Vector3 placePos)
    {
        placePos = Vector3.zero;

        if (uiInv == null)
            uiInv = FindObjectOfType<UIInventory>();

        if (inventory == null)
            inventory = FindObjectOfType<Inventory>();

        if (uiInv == null || inventory == null)
            return false;

        var bt = uiInv.GetSelectedTypeOnly();
        if (!IsPlaceableBlock(bt))
            return false;

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
                return false;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, hitMask))
            return false;

        placePos = hit.point + hit.normal * 0.5f;
        placePos = new Vector3(
            Mathf.Round(placePos.x),
            Mathf.Round(placePos.y),
            Mathf.Round(placePos.z)
        );

        foreach (var c in Physics.OverlapBox(placePos, blockHalfExtents))
        {
            if (c.GetComponent<Block>() != null)
                return false;
        }

        return true;
    }

    bool IsPlaceableBlock(BlockType? bt)
    {
        if (bt == null) return false;

        return bt == BlockType.Dirt
            || bt == BlockType.Grass
            || bt == BlockType.Water
            || bt == BlockType.Stone
            || bt == BlockType.Wood
            || bt == BlockType.Diamond;
    }

    GameObject GetPrefabForBlockType(BlockType t)
    {
        return t switch
        {
            BlockType.Dirt => dirtPrefab,
            BlockType.Grass => grassPrefab,
            BlockType.Water => waterPrefab,
            BlockType.Stone => stonePrefab,
            BlockType.Wood => woodPrefab,
            BlockType.Diamond => diamondPrefab,
            _ => null
        };
    }
}