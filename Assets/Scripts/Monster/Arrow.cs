using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Arrow : MonoBehaviour
{
    public float speed = 20f;
    public float gravity = -20f;
    public int damage = 6;
    public float lifeTime = 6f;

    Vector3 velocity;

    void Start()
    {
        Destroy(gameObject, lifeTime);
        velocity = transform.forward * speed;
        GetComponent<SphereCollider>().isTrigger = true;
    }

    void Update()
    {
        velocity.y += gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        if (velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(velocity);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>()
                ?.TakeDamage(damage, transform.position);

            Destroy(gameObject);
        }
        else if (other.CompareTag("Block"))
        {
            Destroy(gameObject);
        }
    }
}