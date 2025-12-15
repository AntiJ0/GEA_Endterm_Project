using UnityEngine;

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

    private Camera _cam;
    private UIInventory _uiInv;
    private Inventory _inventory;
    private InventoryPanelController _inventoryPanel;

    private GameObject _ghostInstance;

    void Awake()
    {
        _cam = Camera.main;
        _uiInv = FindObjectOfType<UIInventory>();
        _inventory = FindObjectOfType<Inventory>();
        _inventoryPanel = FindObjectOfType<InventoryPanelController>();

        if (ghostPrefab != null)
        {
            _ghostInstance = Instantiate(ghostPrefab);
            _ghostInstance.SetActive(false);

            if (_ghostInstance.TryGetComponent(out Collider c)) Destroy(c);
            if (_ghostInstance.TryGetComponent(out Rigidbody r)) Destroy(r);
        }
    }

    void Update()
    {
        if (_inventoryPanel != null && _inventoryPanel.panel.activeSelf)
        {
            SetGhost(false);
            return;
        }

        HandleHotkeys();
        UpdateGhost();

        if (Input.GetMouseButtonDown(1))
        {
            TryPlaceBlock();
        }
    }

    void HandleHotkeys()
    {
        if (_uiInv == null) return;

        for (int i = 0; i < _uiInv.slots.Length && i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                _uiInv.ToggleSelectSlot(i);
                break;
            }
        }
    }

    void UpdateGhost()
    {
        if (!TryGetPlacePosition(out Vector3 pos))
        {
            SetGhost(false);
            return;
        }

        _ghostInstance.transform.position = pos;
        SetGhost(true);
    }

    void SetGhost(bool active)
    {
        if (_ghostInstance != null)
            _ghostInstance.SetActive(active);
    }

    void TryPlaceBlock()
    {
        Debug.Log("TryPlaceBlock CALLED");

        if (!TryGetPlacePosition(out Vector3 pos))
        {
            Debug.Log("Place failed: no valid position");
            return;
        }

        var bt = _uiInv.GetSelectedBlockType();
        if (!IsPlaceableBlock(bt)) return;

        if (!_inventory.Consume(bt.Value, 1))
        {
            Debug.Log("Inventory consume failed");
            return;
        }

        GameObject prefab = GetPrefabForBlockType(bt.Value);
        Instantiate(prefab, pos, Quaternion.identity);

        _uiInv.RefreshAll();
        Debug.Log("Block placed");
    }

    bool TryGetPlacePosition(out Vector3 placePos)
    {
        placePos = Vector3.zero;

        if (_uiInv == null || _inventory == null) return false;
        if (_uiInv.selectedIndex < 0 || _uiInv.GetSelectedCount() <= 0) return false;

        var bt = _uiInv.GetSelectedBlockType();
        if (!IsPlaceableBlock(bt)) return false;

        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!Physics.Raycast(ray, out var hit, rayDistance, hitMask))
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
