using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Hitbox")]
    public Transform attackPoint;
    public Vector3 hitboxSize = new Vector3(0.4f, 0.4f, 3f);
    public LayerMask enemyMask;
    public int damage = 1;

    [Header("Combo Timing")]
    public float comboInputWindow = 0.75f;
    public float attackCooldown = 0.15f;
    public float attackDuration = 0.45f;

    [Header("Damage Timing")]
    public float hitStartDelay = 0.12f;
    public float hitActiveTime = 0.18f;

    [Header("VFX Delay")]
    public float attack1VFXDelay = 0.08f;
    public float attack2VFXDelay = 0.1f;
    public float attack3VFXDelay = 0.12f;

    [Header("Movement While Attacking")]
    public float attackSlowDuration = 0.45f;
    public float attackMoveMultiplier = 0.35f;

    [Header("Attack VFX")]
    public ParticleSystem attack1VFX;
    public ParticleSystem attack2VFX;
    public ParticleSystem attack3VFX;

    [Header("Animation")]
    public Animator animator;

    private int comboStep;
    private int attackId;
    private float lastAttackTime = -999f;
    private float nextAttackTime;
    private float attackSlowTimer;
    private bool isAttacking;
    private Coroutine attackRoutine;

    public bool IsAttacking => isAttacking;
    public bool IsAttackSlowed => attackSlowTimer > 0f;
    public float AttackMoveMultiplier => attackMoveMultiplier;

    void Update()
    {
        if (attackSlowTimer > 0f)
            attackSlowTimer -= Time.deltaTime;

        if (Time.time - lastAttackTime > comboInputWindow)
            comboStep = 0;

        if (isAttacking)
            FaceMouse();

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
            StartComboAttack();
    }

    void StartComboAttack()
    {
        attackId++;

        comboStep = Time.time - lastAttackTime > comboInputWindow ? 1 : comboStep + 1;

        if (comboStep > 3)
            comboStep = 1;

        lastAttackTime = Time.time;
        nextAttackTime = Time.time + attackCooldown;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackRoutine(comboStep, attackId));
    }

    IEnumerator AttackRoutine(int attackNumber, int id)
    {
        isAttacking = true;
        attackSlowTimer = attackSlowDuration;

        FaceMouse();
        PlayAnimation(attackNumber);

        StartCoroutine(DelayedVFX(attackNumber, id));
        StartCoroutine(DamageWindow(id));

        yield return new WaitForSeconds(attackDuration);

        if (id == attackId)
        {
            isAttacking = false;
            attackRoutine = null;
        }
    }

    void PlayAnimation(int attackNumber)
    {
        if (animator == null)
            return;

        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");
        animator.SetTrigger("Attack" + attackNumber);
    }

    IEnumerator DelayedVFX(int attackNumber, int id)
    {
        yield return new WaitForSeconds(GetVFXDelay(attackNumber));

        if (id == attackId)
            PlayVFX(GetVFX(attackNumber));
    }

    void PlayVFX(ParticleSystem vfx)
    {
        if (vfx == null)
            return;

        vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfx.Play();
    }

    ParticleSystem GetVFX(int attackNumber)
    {
        if (attackNumber == 1) return attack1VFX;
        if (attackNumber == 2) return attack2VFX;
        return attack3VFX;
    }

    float GetVFXDelay(int attackNumber)
    {
        if (attackNumber == 1) return attack1VFXDelay;
        if (attackNumber == 2) return attack2VFXDelay;
        return attack3VFXDelay;
    }

    IEnumerator DamageWindow(int id)
    {
        yield return new WaitForSeconds(hitStartDelay);

        float timer = 0f;
        HashSet<EnemyReceiveDamage> damagedEnemies = new HashSet<EnemyReceiveDamage>();

        while (timer < hitActiveTime && id == attackId)
        {
            CheckHitbox(damagedEnemies);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    void CheckHitbox(HashSet<EnemyReceiveDamage> damagedEnemies)
    {
        if (attackPoint == null)
            return;

        Collider[] hits = Physics.OverlapBox(
            attackPoint.position,
            hitboxSize * 0.5f,
            attackPoint.rotation,
            enemyMask
        );

        foreach (Collider hit in hits)
        {
            EnemyReceiveDamage enemy = hit.GetComponentInParent<EnemyReceiveDamage>();

            if (enemy != null && damagedEnemies.Add(enemy))
                enemy.Hit(damage);
        }
    }

    void FaceMouse()
    {
        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, transform.position);

        if (ground.Raycast(ray, out float distance))
        {
            Vector3 direction = ray.GetPoint(distance) - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
                transform.forward = direction.normalized;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
        Gizmos.matrix = oldMatrix;
    }

    public void ResetComboFromDash()
    {
        attackId++;

        comboStep = 0;
        lastAttackTime = -999f;

        // Allows attacking right after dash, but resets back to Attack1.
        nextAttackTime = Time.time;

        isAttacking = false;
        attackSlowTimer = 0f;

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (animator != null)
        {
            animator.ResetTrigger("Attack1");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("Attack3");
        }

        if (attack1VFX != null)
            attack1VFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (attack2VFX != null)
            attack2VFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (attack3VFX != null)
            attack3VFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void SetDamage(int newDamage) => damage = newDamage;
    public int GetDamage() => damage;

    public void SetAttackCooldown(float newCooldown) => attackCooldown = newCooldown;
    public float GetAttackCooldown() => attackCooldown;

    public void SetAttackRange(float newRange) => hitboxSize.z = newRange;
    public float GetAttackRange() => hitboxSize.z;
}