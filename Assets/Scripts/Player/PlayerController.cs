using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float waterMoveSpeed = 2f;
    public float jumpPower = 5f;

    [Header("HP")]
    public int maxHp = 20;
    public int curHp;

    [Header("Gravity Settings")]
    public float normalGravity = -9.81f;
    public float waterGravity = -2f;
    public float floatUpSpeed = 2f;

    [Header("Water Exit Slow Settings")]
    public float waterExitSlowDuration = 0.5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 3f;

    [Header("Eating Settings")]
    public float eatingMoveSpeedMultiplier = 0.4f;

    [Header("Knockback")]
    public float knockbackDuration = 0.2f;

    [Header("Hunger")]
    public int maxHunger = 16;
    public int curHunger = 16;
    public float starvationDamageInterval = 5f;

    public float hungerDecreaseInterval = 10f;
    public float regenHpInterval = 1f;

    float starvationTimer = 0f;
    float hungerTimer = 0f;
    float hpRegenTimer = 0f;
    int regenHpStack = 0;

    float xRotation = 0f;

    CharacterController controller;
    Transform cam;

    Vector3 velocity;
    Vector3 knockbackVelocity;
    float knockbackTimer;

    bool isGrounded;

    int waterContactCount = 0;
    float currentGravity;
    float waterSlowTimer = 0f;

    public bool IsEating { get; private set; } = false;

    Inventory inventory;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>().transform;
    }

    void Start()
    {
        curHp = maxHp;
        curHunger = maxHunger;

        UIPlayerStat.Instance?.RefreshHP(curHp, maxHp);
        UIPlayerStat.Instance?.RefreshHunger(curHunger, maxHunger);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentGravity = normalGravity;
        inventory = FindObjectOfType<Inventory>();
    }

    void Update()
    {
        HandleMove();
        HandleLook();

        if (waterSlowTimer > 0)
            waterSlowTimer -= Time.deltaTime;

        HandleInventoryEatInput(); 
        HandleHungerAndRegen();
    }

    void HandleMove()
    {
        if (knockbackTimer > 0)
        {
            controller.Move(knockbackVelocity * Time.deltaTime);
            knockbackTimer -= Time.deltaTime;
            return;
        }

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveDir = transform.right * h + transform.forward * v;

        float finalSpeed = moveSpeed;

        if (waterContactCount > 0 || waterSlowTimer > 0)
            finalSpeed = waterMoveSpeed;

        if (IsEating)
            finalSpeed *= eatingMoveSpeedMultiplier;

        controller.Move(moveDir * finalSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpPower * -2f * currentGravity);

        if (waterContactCount > 0 && Input.GetButton("Jump"))
            velocity.y = floatUpSpeed;
        else
            velocity.y += currentGravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        Block block = other.GetComponent<Block>();
        if (block != null && block.type == BlockType.Water)
        {
            waterContactCount++;
            currentGravity = waterGravity;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Block block = other.GetComponent<Block>();
        if (block != null && block.type == BlockType.Water)
        {
            waterContactCount--;
            if (waterContactCount <= 0)
            {
                waterContactCount = 0;
                waterSlowTimer = waterExitSlowDuration;
                currentGravity = normalGravity;
            }
        }
    }

    void HandleInventoryEatInput()
    {
        if (IsEating) return;

        var uiInv = FindObjectOfType<UIInventory>();
        if (uiInv == null) return;

        if (Input.GetMouseButtonDown(1)) 
        {
            var selectedType = uiInv.GetSelectedBlockType();
            if (selectedType == null) return;

            if (selectedType == BlockType.Beef || selectedType == BlockType.Pork)
            {
                StartEating(selectedType.Value, 2f); 
            }
        }
    }

    public bool StartEating(BlockType itemType, float duration)
    {
        if (IsEating) return false;
        if (inventory == null) return false;

        StartCoroutine(EatingCoroutine(itemType, duration));
        return true;
    }

    IEnumerator EatingCoroutine(BlockType itemType, float duration)
    {
        IsEating = true;

        yield return new WaitForSeconds(duration); 

        if (inventory.Consume(itemType, 1))
        {
            curHunger = Mathf.Min(maxHunger, curHunger + 8);
            UIPlayerStat.Instance?.RefreshHunger(curHunger, maxHunger);
        }

        IsEating = false;
    }

    public void TakeDamage(int dmg, Vector3 attackerPos, float knockbackForce = 4f)
    {
        curHp -= dmg;
        if (curHp < 0) curHp = 0;

        UIPlayerStat.Instance?.RefreshHP(curHp, maxHp);

        Vector3 dir = (transform.position - attackerPos);
        dir.y = 0.3f;
        dir.Normalize();

        knockbackVelocity = dir * knockbackForce;
        knockbackTimer = knockbackDuration;

        if (curHp <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("Player Dead");

        Time.timeScale = 0f;

        GameObject gameOverPanel = GameObject.Find("GameOverPanel");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    void HandleHungerAndRegen()
    {
        bool canRegen = curHunger >= 14 && curHp < maxHp;

        if (canRegen)
        {
            hpRegenTimer += Time.deltaTime;

            if (hpRegenTimer >= regenHpInterval)
            {
                hpRegenTimer -= regenHpInterval;

                curHp++;
                if (curHp > maxHp) curHp = maxHp;

                regenHpStack++;

                if (regenHpStack >= 3)
                {
                    regenHpStack = 0;
                    curHunger = Mathf.Max(0, curHunger - 1);
                    UIPlayerStat.Instance?.RefreshHunger(curHunger, maxHunger);
                }

                UIPlayerStat.Instance?.RefreshHP(curHp, maxHp);
            }

            starvationTimer = 0f;
            hungerTimer = 0f;
        }
        else
        {
            hpRegenTimer = 0f;
            regenHpStack = 0;

            if (curHunger <= 0)
            {
                starvationTimer += Time.deltaTime;

                if (starvationTimer >= starvationDamageInterval)
                {
                    starvationTimer -= starvationDamageInterval;

                    curHp--;
                    if (curHp < 0) curHp = 0;

                    UIPlayerStat.Instance?.RefreshHP(curHp, maxHp);

                    if (curHp <= 0)
                        Die();
                }

                hungerTimer = 0f;
            }
            else
            {
                hungerTimer += Time.deltaTime;

                if (hungerTimer >= hungerDecreaseInterval)
                {
                    hungerTimer -= hungerDecreaseInterval;
                    curHunger = Mathf.Max(0, curHunger - 1);
                    UIPlayerStat.Instance?.RefreshHunger(curHunger, maxHunger);
                }

                starvationTimer = 0f;
            }
        }
    }
}
