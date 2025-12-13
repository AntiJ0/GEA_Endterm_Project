using UnityEngine;

public class Skeleton : MonsterBase
{
    public float detectRange = 15f;

    public Transform firePoint;
    public GameObject arrowPrefab;
    public GameObject arrowModel;

    [Header("Aim Offset")]
    public float aimHeightOffset = 1.2f; 

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
            SetPlanarDirection(Vector3.zero);

            float remain = nextAttackTime - Time.time;

            if (CanAttack())
            {
                Shoot();
            }
            else if (remain <= 2f)
            {
                arrowModel.SetActive(true);
            }

            return;
        }

        base.HandleBehavior();
    }

    void Shoot()
    {
        ResetAttackCooldown();
        arrowModel.SetActive(false);

        Vector3 targetPos = player.position;
        targetPos.y += aimHeightOffset;

        Vector3 dir = (targetPos - firePoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        Instantiate(arrowPrefab, firePoint.position, rot);
    }
}
