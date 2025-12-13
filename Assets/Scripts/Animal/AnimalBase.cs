using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AnimalBase : MonoBehaviour
{
    public AnimalTypeData data;

    public float minWanderTime = 1.5f;
    public float maxWanderTime = 4f;
    public float idleChance = 0.3f;

    public float jumpPower = 5f;
    public float obstacleCheckDistance = 0.7f;
    public float obstacleMaxHeight = 1.0f;
    public float obstacleCastRadius = 0.25f;

    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    public float stepOffset = 0.3f;

    public float scaredDuration = 10f;

    public float knockbackDecay = 6f;
    public float knockbackHorizontalForce = 4f;
    public float knockbackUpForce = 1.2f;

    int hp;
    CharacterController controller;
    protected Transform player;

    Vector3 planarDir = Vector3.zero;
    float verticalVelocity = 0f;
    Vector3 currentKnockback = Vector3.zero;

    Vector3 wanderDir;
    float wanderTimer = 0f;

    float scaredEndTime = 0f;
    bool isScared = false;
    bool isInWater = false;

    Quaternion targetRotation;
    Renderer[] renderers;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        renderers = GetComponentsInChildren<Renderer>();
        controller.stepOffset = stepOffset;
    }

    void Start()
    {
        if (data == null)
        {
            Debug.LogError("[AnimalBase] data is null");
            enabled = false;
            return;
        }

        hp = data.maxHp;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        PickRandomDirection();
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        UpdateScaredState();
        HandleBehavior();
        ApplyRotation(Time.deltaTime);
        ApplyGravity(Time.deltaTime);

        Vector3 finalPlanar = planarDir;
        finalPlanar.x += currentKnockback.x;
        finalPlanar.z += currentKnockback.z;

        float finalVertical = verticalVelocity + currentKnockback.y;

        Vector3 displacement = (finalPlanar * Time.deltaTime) + Vector3.up * (finalVertical * Time.deltaTime);
        controller.Move(displacement);

        currentKnockback = Vector3.MoveTowards(currentKnockback, Vector3.zero, knockbackDecay * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
    }

    protected virtual void HandleBehavior()
    {
        if (isScared && player != null)
        {
            Vector3 dir = (transform.position - player.position);
            dir.y = 0f;
            dir.Normalize();
            SetPlanarDirection(dir * data.fleeSpeed);
            SetRotation(dir);
            return;
        }

        if (player != null && data.fleeWhenPlayerNear)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= data.fleeDistance)
            {
                Vector3 dir = (transform.position - player.position);
                dir.y = 0f;
                dir.Normalize();
                SetPlanarDirection(dir * data.fleeSpeed);
                SetRotation(dir);
                return;
            }
        }

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
            PickRandomDirection();

        if (wanderDir.sqrMagnitude > 0.001f)
            SetPlanarDirection(wanderDir * data.walkSpeed);
        else
            SetPlanarDirection(Vector3.zero);

        if (wanderDir != Vector3.zero)
            SetRotation(wanderDir);
    }

    void PickRandomDirection()
    {
        wanderTimer = Random.Range(minWanderTime, maxWanderTime);
        if (Random.value < idleChance)
        {
            wanderDir = Vector3.zero;
            return;
        }

        Vector2 r = Random.insideUnitCircle.normalized;
        wanderDir = new Vector3(r.x, 0f, r.y);
    }

    protected void SetPlanarDirection(Vector3 desiredVelocity)
    {
        if (desiredVelocity.sqrMagnitude > 0.0001f)
        {
            Vector3 dir = desiredVelocity.normalized;
            Vector3 origin = transform.position + Vector3.up * 0.2f;

            RaycastHit hit;
            bool collided = Physics.SphereCast(origin, obstacleCastRadius, dir, out hit, obstacleCheckDistance);

            if (!collided)
            {
                Vector3 right = transform.right;
                float sideOffset = Mathf.Clamp(controller.radius * 0.8f, 0.1f, 0.5f);
                Vector3 o1 = origin + right * sideOffset;
                Vector3 o2 = origin - right * sideOffset;
                if (Physics.SphereCast(o1, obstacleCastRadius, dir, out hit, obstacleCheckDistance))
                    collided = true;
                else if (Physics.SphereCast(o2, obstacleCastRadius, dir, out hit, obstacleCheckDistance))
                    collided = true;
            }

            if (collided)
            {
                float topY = hit.collider.bounds.max.y;
                float heightDiff = topY - transform.position.y;
                if (heightDiff <= obstacleMaxHeight && controller.isGrounded)
                {
                    verticalVelocity = Mathf.Sqrt(jumpPower * -2f * gravity);
                }
                else
                {
                    desiredVelocity = Vector3.zero;
                }
            }
        }

        if (isInWater)
            desiredVelocity *= 0.5f;

        planarDir = desiredVelocity;
    }

    protected void SetRotation(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        targetRotation = Quaternion.LookRotation(dir.normalized);
    }

    protected void ApplyRotation(float dt)
    {
        Quaternion smoothed = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * dt);
        Vector3 e = smoothed.eulerAngles;
        e.x = 0f;
        e.z = 0f;
        transform.rotation = Quaternion.Euler(e);
    }

    protected void ApplyGravity(float dt)
    {
        if (isInWater)
        {
            verticalVelocity = Mathf.Lerp(verticalVelocity, 1.5f, dt * 3f);
            return;
        }

        verticalVelocity += gravity * dt;
    }

    public void TakeDamage(int dmg, Vector3 attackerPos)
    {
        if (dmg <= 0) return;

        hp -= dmg;
        StartCoroutine(FlashRed());

        Vector3 dir = (transform.position - attackerPos);
        dir.y = 0f;
        dir.Normalize();

        currentKnockback = new Vector3(dir.x * knockbackHorizontalForce, knockbackUpForce, dir.z * knockbackHorizontalForce);

        if (data.fleeWhenAttacked)
        {
            isScared = true;
            scaredEndTime = Time.time + scaredDuration;
        }

        if (hp <= 0) Die();
    }

    protected void UpdateScaredState()
    {
        if (isScared && Time.time >= scaredEndTime) isScared = false;
    }

    IEnumerator FlashRed()
    {
        if (renderers == null) yield break;
        foreach (var r in renderers)
            foreach (var m in r.materials)
                m.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        foreach (var r in renderers)
            foreach (var m in r.materials)
                m.color = Color.white;
    }

    void Die()
    {
        if (Random.value <= data.dropChance && data.dropType != BlockType.Bedrock)
        {
            int amount = Random.Range(data.dropMin, data.dropMax + 1);
            if (amount > 0)
            {
                var inv = FindObjectOfType<Inventory>();
                inv?.Add(data.dropType, amount);
            }
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        var block = other.GetComponent<Block>();
        if (block != null && block.type == BlockType.Water) isInWater = true;
    }

    void OnTriggerExit(Collider other)
    {
        var block = other.GetComponent<Block>();
        if (block != null && block.type == BlockType.Water) isInWater = false;
    }
}