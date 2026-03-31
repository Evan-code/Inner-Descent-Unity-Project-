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

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        attackSlowTimer = attackSlowDuration;

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

     // Lets other scripts change the player's attack damage
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    // Lets other scripts read the player's current damage
    public int GetDamage()
    {
        return damage;
    }

    // Lets other scripts change attack cooldown
    public void SetAttackCooldown(float newCooldown)
    {
        attackCooldown = newCooldown;
    }

    // Lets other scripts read attack cooldown
    public float GetAttackCooldown()
    {
        return attackCooldown;
    }
}