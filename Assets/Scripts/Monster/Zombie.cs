using UnityEngine;

public class Zombie : MonsterBase
{
    [Header("Detect")]
    public float detectRange = 10f;

    [Header("Combat")]
    public float stopDistance = 1.8f;   // 멈추는 거리 (공격 포함)

    protected override void HandleBehavior()
    {
        if (player == null)
        {
            base.HandleBehavior();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= detectRange)
        {
            Vector3 dir = (player.position - transform.position);
            dir.y = 0f;
            dir.Normalize();

            SetRotation(dir);

            if (dist <= stopDistance)
            {
                SetPlanarDirection(Vector3.zero);

                if (CanAttack())
                    Attack();
            }
            else
            {
                SetPlanarDirection(dir * data.fleeSpeed);
            }

            return;
        }

        base.HandleBehavior();
    }

    void Attack()
    {
        ResetAttackCooldown();

        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.TakeDamage(attackDamage, transform.position, knockbackPower);
    }
}