using UnityEngine;

public class MonsterBase : AnimalBase
{
    [Header("Monster")]
    public int attackDamage;
    public float attackCooldown;
    public float attackRange;

    [Header("Knockback")]
    public float knockbackPower = 4f;

    protected float nextAttackTime = 0f;

    MonsterSpawner spawner;

    public void SetSpawner(MonsterSpawner s)
    {
        spawner = s;
    }

    protected override void Die()
    {
        spawner?.OnMonsterDied();
        base.Die();
    }

    protected bool PlayerInRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= attackRange;
    }

    protected bool CanAttack()
    {
        return Time.time >= nextAttackTime;
    }

    protected void ResetAttackCooldown()
    {
        nextAttackTime = Time.time + attackCooldown;
    }
}