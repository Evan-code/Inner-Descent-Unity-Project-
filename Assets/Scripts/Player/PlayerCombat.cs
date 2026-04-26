using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    public Transform meleePoint;
    public float attackRadius = 1.2f;
    public int damage = 1;
    public float attackCooldown = 0.35f;
    public float attackDuration = 0.55f;
    public float hitDelay = 0.1f;
    public LayerMask enemyMask;

    [Header("Movement While Attacking")]
    public float attackSlowDuration = 0.55f;
    public float attackMoveMultiplier = 0.35f;

    [Header("Animation")]
    public Animator animator;

    private float nextAttackTime = 0f;
    private float attackSlowTimer = 0f;
    private bool isAttacking = false;

    public bool IsAttackSlowed => attackSlowTimer > 0f;
    public float AttackMoveMultiplier => attackMoveMultiplier;
    public bool IsAttacking => isAttacking;

    void Update()
    {
        if (attackSlowTimer > 0f)
        {
            attackSlowTimer -= Time.deltaTime;
        }

        // Keep facing the mouse during the entire attack animation
        if (isAttacking)
        {
            FaceMouse();
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    void FaceMouse()
    {
        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPosition = ray.GetPoint(distance);

            Vector3 direction = mouseWorldPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.forward = direction.normalized;
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        attackSlowTimer = attackSlowDuration;

        FaceMouse();

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(hitDelay);

        Attack();

        float remainingAttackTime = attackDuration - hitDelay;

        if (remainingAttackTime > 0f)
        {
            yield return new WaitForSeconds(remainingAttackTime);
        }

        isAttacking = false;
    }

    void Attack()
    {
        if (meleePoint == null)
            return;

        Collider[] hits = Physics.OverlapSphere(meleePoint.position, attackRadius, enemyMask);

        HashSet<EnemyReceiveDamage> damagedEnemies = new HashSet<EnemyReceiveDamage>();

        foreach (Collider hit in hits)
        {
            EnemyReceiveDamage enemyDamage = hit.GetComponentInParent<EnemyReceiveDamage>();

            if (enemyDamage != null && !damagedEnemies.Contains(enemyDamage))
            {
                enemyDamage.Hit(damage);
                damagedEnemies.Add(enemyDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (meleePoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleePoint.position, attackRadius);
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public int GetDamage()
    {
        return damage;
    }

    public void SetAttackCooldown(float newCooldown)
    {
        attackCooldown = newCooldown;
    }

    public float GetAttackCooldown()
    {
        return attackCooldown;
    }
}