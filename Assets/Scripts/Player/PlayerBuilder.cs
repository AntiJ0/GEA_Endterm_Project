using UnityEngine;

public class PlayerBuilder : MonoBehaviour
{
    [Header("레이/사거리")]
    public float rayDistance = 5f;
    public LayerMask hitMask = ~0;

    [Header("블록 프리팹 (각 BlockType에 맞게 연결)")]
    public GameObject dirtPrefab;
    public GameObject grassPrefab;
    public GameObject waterPrefab;

    [Header("희미한 블록(ghost) 프리팹 - 동일한 크기여야 함")]
    public GameObject ghostPrefab;

    [Header("블록 체크용 (반 사이즈)")]
    public Vector3 blockHalfExtents = Vector3.one * 0.45f; 

    private const float GhostPositionEpsilon = 0.001f;

    private Camera _cam;
    private UIInventory _uiInv;
    private Inventory _inventory;

    private GameObject _ghostInstance;
    private Vector3 _currentGhostPos;
    private bool _ghostActive = false;

    private bool _lastGhostActive = false;
    private Vector3 _lastGhostPos;

    void Awake()
    {
        _cam = Camera.main;
        _uiInv = FindObjectOfType<UIInventory>();
        _inventory = FindObjectOfType<Inventory>();

        if (ghostPrefab != null)
        {
            _ghostInstance = Instantiate(ghostPrefab);
            _ghostInstance.SetActive(false);
            var coll = _ghostInstance.GetComponent<Collider>();
            if (coll != null) Destroy(coll);
            var rb = _ghostInstance.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);
        }

        _lastGhostPos = Vector3.positiveInfinity;
    }

    void Update()
    {
        HandleHotkeys();
        UpdateGhost();

        if (Input.GetMouseButtonDown(1) && _ghostActive)
        {
            TryPlaceBlock();
        }
    }

    void HandleHotkeys()
    {
        if (_uiInv == null) return;
        int slotCount = _uiInv.slots.Length;

        for (int i = 0; i < slotCount && i < 10; i++)
        {
            KeyCode kc;
            if (i == 9) kc = KeyCode.Alpha0;
            else kc = KeyCode.Alpha1 + i;

            if (Input.GetKeyDown(kc))
            {
                _uiInv.ToggleSelectSlot(i);
                break;
            }
        }
    }

    void UpdateGhost()
    {
        bool shouldBeActive = false;
        Vector3 computedPos = Vector3.zero;

        if (_uiInv == null || _inventory == null || _ghostInstance == null)
        {
            SetGhostActive(false);
            return;
        }

        int selIndex = _uiInv.selectedIndex;
        if (selIndex < 0 || _uiInv.GetSelectedCount() <= 0)
        {
            SetGhostActive(false);
            return;
        }

        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out var hit, rayDistance, hitMask))
        {
            var hitBlock = hit.collider.GetComponent<Block>();
            if (hitBlock == null)
            {
                SetGhostActive(false);
                return;
            }

            Vector3 placePos = hit.point + hit.normal * 0.5f;
            placePos = new Vector3(Mathf.Round(placePos.x), Mathf.Round(placePos.y), Mathf.Round(placePos.z));

            if (Vector3.Distance(_cam.transform.position, placePos) > rayDistance + 0.01f)
            {
                SetGhostActive(false);
                return;
            }

            Vector3 smallHalf = blockHalfExtents * 0.9f;
            Collider[] overlaps = Physics.OverlapBox(placePos, smallHalf, Quaternion.identity, hitMask);
            bool occupied = false;
            foreach (var c in overlaps)
            {
                if (c == null) continue;
                if (c.GetComponent<Block>() != null)
                {
                    occupied = true;
                    break;
                }
            }
            if (occupied)
            {
                SetGhostActive(false);
                return;
            }

            computedPos = placePos;
            shouldBeActive = true;
        }
        else
        {
            shouldBeActive = false;
        }

        if (shouldBeActive)
        {
            if (_lastGhostPos == Vector3.positiveInfinity || Vector3.Distance(_lastGhostPos, computedPos) > GhostPositionEpsilon)
            {
                _ghostInstance.transform.position = computedPos;
                _lastGhostPos = computedPos;
            }
        }

        if (_lastGhostActive != shouldBeActive)
        {
            _ghostInstance.SetActive(shouldBeActive);
            _lastGhostActive = shouldBeActive;
        }

        _ghostActive = shouldBeActive;
    }

    void SetGhostActive(bool active)
    {
        if (_ghostInstance == null) return;
        if (_lastGhostActive == active) return;
        _ghostInstance.SetActive(active);
        _lastGhostActive = active;
        if (!active)
            _lastGhostPos = Vector3.positiveInfinity;
    }

    void TryPlaceBlock()
    {
        if (_uiInv == null || _inventory == null) return;
        var bt = _uiInv.GetSelectedBlockType();
        if (bt == null) return;

        Vector3 smallHalf = blockHalfExtents * 0.9f;
        Collider[] overlaps = Physics.OverlapBox(_lastGhostPos, smallHalf, Quaternion.identity, hitMask);
        foreach (var c in overlaps)
        {
            if (c == null) continue;
            if (c.GetComponent<Block>() != null)
            {
                Debug.Log("[Builder] 설치 위치가 이미 차있음.");
                return;
            }
        }

        GameObject prefab = GetPrefabForBlockType(bt.Value);
        if (prefab == null)
        {
            Debug.LogError("[Builder] 해당 블록 타입에 연결된 프리팹이 없습니다: " + bt.Value);
            return;
        }

        bool consumed = _uiInv.ConsumeOneFromSelected();
        if (!consumed)
        {
            Debug.Log("[Builder] 인벤토리에서 소비 실패.");
            return;
        }

        Vector3 place = new Vector3(Mathf.Round(_lastGhostPos.x), Mathf.Round(_lastGhostPos.y), Mathf.Round(_lastGhostPos.z));
        GameObject go = Instantiate(prefab, place, Quaternion.identity);

        var blockComp = go.GetComponent<Block>();
        if (blockComp == null)
        {
            blockComp = go.AddComponent<Block>();
        }

        if (go.GetComponent<Collider>() == null)
        {
            var bc = go.AddComponent<BoxCollider>();
            bc.size = Vector3.one;
        }

        go.tag = "Block";

        if (_uiInv.GetSelectedCount() <= 0)
        {
            _uiInv.Deselect();
            SetGhostActive(false);
        }
    }

    GameObject GetPrefabForBlockType(BlockType t)
    {
        return t switch
        {
            BlockType.Dirt => dirtPrefab,
            BlockType.Grass => grassPrefab,
            BlockType.Water => waterPrefab,
            _ => null
        };
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(_lastGhostPos, blockHalfExtents * 2f);
    }
}