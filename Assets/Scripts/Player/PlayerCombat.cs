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

    [Header("Attack 1 VFX")]
    public GameObject attack1VFX;
    public Vector3 attack1VFXOffset;
    public Vector3 attack1VFXRotation;
    public Vector3 attack1VFXScale = Vector3.one;

    [Header("Attack 2 VFX")]
    public GameObject attack2VFX;
    public Vector3 attack2VFXOffset;
    public Vector3 attack2VFXRotation;
    public Vector3 attack2VFXScale = Vector3.one;

    [Header("Attack 3 VFX")]
    public GameObject attack3VFX;
    public Vector3 attack3VFXOffset;
    public Vector3 attack3VFXRotation;
    public Vector3 attack3VFXScale = Vector3.one;

    [Header("VFX Settings")]
    public float vfxLifetime = 1.5f;

    [Header("Animation")]
    public Animator animator;

    private int comboStep = 0;
    private float lastAttackTime = -999f;
    private float nextAttackTime = 0f;
    private float attackSlowTimer = 0f;
    private bool isAttacking = false;

    public bool IsAttacking => isAttacking;
    public bool IsAttackSlowed => attackSlowTimer > 0f;
    public float AttackMoveMultiplier => attackMoveMultiplier;

    void Update()
    {
        if (attackSlowTimer > 0f)
            attackSlowTimer -= Time.deltaTime;

        if (isAttacking)
            FaceMouse();

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            StartComboAttack();
        }
    }

    void StartComboAttack()
    {
        StopAllCoroutines();

        if (Time.time - lastAttackTime <= comboInputWindow)
            comboStep++;
        else
            comboStep = 1;

        if (comboStep > 3)
            comboStep = 1;

        lastAttackTime = Time.time;
        nextAttackTime = Time.time + attackCooldown;

        StartCoroutine(AttackRoutine(comboStep));
    }

    IEnumerator AttackRoutine(int attackNumber)
    {
        isAttacking = true;
        attackSlowTimer = attackSlowDuration;

        FaceMouse();

        if (animator != null)
        {
            animator.ResetTrigger("Attack1");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("Attack3");

            animator.SetTrigger("Attack" + attackNumber);
        }

        StartCoroutine(DelayedVFX(attackNumber));
        StartCoroutine(ActiveDamageWindow());

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }

    IEnumerator ActiveDamageWindow()
    {
        yield return new WaitForSeconds(hitStartDelay);

        float timer = 0f;
        HashSet<EnemyReceiveDamage> damagedEnemies = new HashSet<EnemyReceiveDamage>();

        while (timer < hitActiveTime)
        {
            CheckHitbox(damagedEnemies);

            timer += Time.deltaTime;
            yield return null;
        }
    }

    void CheckHitbox(HashSet<EnemyReceiveDamage> damagedEnemies)
    {
        if (!isAttacking)
            return;

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
            EnemyReceiveDamage enemyDamage = hit.GetComponentInParent<EnemyReceiveDamage>();

            if (enemyDamage != null && !damagedEnemies.Contains(enemyDamage))
            {
                enemyDamage.Hit(damage);
                damagedEnemies.Add(enemyDamage);
            }
        }
    }

    IEnumerator DelayedVFX(int attackNumber)
    {
        float delay = 0f;

        if (attackNumber == 1) delay = attack1VFXDelay;
        if (attackNumber == 2) delay = attack2VFXDelay;
        if (attackNumber == 3) delay = attack3VFXDelay;

        yield return new WaitForSeconds(delay);

        SpawnAttackVFX(attackNumber);
    }

    void SpawnAttackVFX(int attackNumber)
    {
        GameObject prefab = null;
        Vector3 offset = Vector3.zero;
        Vector3 rotationOffset = Vector3.zero;
        Vector3 scale = Vector3.one;

        if (attackNumber == 1)
        {
            prefab = attack1VFX;
            offset = attack1VFXOffset;
            rotationOffset = attack1VFXRotation;
            scale = attack1VFXScale;
        }
        else if (attackNumber == 2)
        {
            prefab = attack2VFX;
            offset = attack2VFXOffset;
            rotationOffset = attack2VFXRotation;
            scale = attack2VFXScale;
        }
        else if (attackNumber == 3)
        {
            prefab = attack3VFX;
            offset = attack3VFXOffset;
            rotationOffset = attack3VFXRotation;
            scale = attack3VFXScale;
        }

        if (prefab == null)
            return;

        Vector3 spawnPosition =
            transform.position +
            transform.forward * offset.z +
            transform.right * offset.x +
            transform.up * offset.y;

        Quaternion spawnRotation =
            transform.rotation * Quaternion.Euler(rotationOffset);

        GameObject vfx = Instantiate(prefab, spawnPosition, spawnRotation);
        vfx.transform.localScale = scale;

        Destroy(vfx, vfxLifetime);
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
                transform.forward = direction.normalized;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            attackPoint.position,
            attackPoint.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);

        Gizmos.matrix = oldMatrix;
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