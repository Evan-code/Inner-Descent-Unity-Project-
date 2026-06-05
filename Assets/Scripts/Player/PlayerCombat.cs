using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the player's combat
// It handles clicking, combo attacks, attack hitboxes, VFX, damage, and facing the mouse
public class PlayerCombat : MonoBehaviour {
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

    // This stores which attack in the combo we are on
    private int comboStep;

    // This changes every time a new attack starts
    // It helps old attack coroutines know they should stop mattering
    private int attackId;

    // This remembers when the last attack happened
    private float lastAttackTime = -999f;

    // This stores the next time the player is allowed to attack
    private float nextAttackTime;

    // This timer controls how long the player moves slower after attacking
    private float attackSlowTimer;

    // This says if the player is currently attacking
    private bool isAttacking;

    // This stores the current attack coroutine so we can stop it if needed
    private Coroutine attackRoutine;

    // Other scripts can check this to know if the player is attacking
    public bool IsAttacking => isAttacking;

    // Other scripts can check this to know if movement should be slowed
    public bool IsAttackSlowed => attackSlowTimer > 0f;

    // PlayerMovement uses this to know how much slower the player should move
    public float AttackMoveMultiplier => attackMoveMultiplier;

    void Update() {
        // Counts down the attack slow timer if it is above 0
        if (attackSlowTimer > 0f) {
            attackSlowTimer -= Time.deltaTime;
        }

        // If the player waits too long between attacks, reset the combo
        if (Time.time - lastAttackTime > comboInputWindow) {
            comboStep = 0;
        }

        // If attacking, keep facing the mouse
        if (isAttacking) {
            FaceMouse();
        }

        // If the player left clicks and the cooldown is ready, start an attack
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime) {
            StartComboAttack();
        }
    }

    private void StartComboAttack() {
        // Increase attackId so older attack stuff becomes outdated
        attackId++;

        // If too much time passed, start combo over at attack 1
        if (Time.time - lastAttackTime > comboInputWindow) {
            comboStep = 1;
        } else {
            // Otherwise continue to the next combo attack
            comboStep++;
        }

        // If combo goes past 3, loop back to attack 1
        if (comboStep > 3) {
            comboStep = 1;
        }

        // Saves the time of this attack
        lastAttackTime = Time.time;

        // Sets when the next attack is allowed
        nextAttackTime = Time.time + attackCooldown;

        // If an old attack routine is still going, stop it
        if (attackRoutine != null) {
            StopCoroutine(attackRoutine);
        }

        // Start the new attack routine
        attackRoutine = StartCoroutine(AttackRoutine(comboStep, attackId));
    }

    private IEnumerator AttackRoutine(int attackNumber, int id) {
        // Says the player is now attacking
        isAttacking = true;

        // Starts the movement slow timer
        attackSlowTimer = attackSlowDuration;

        // Face the mouse before attacking
        FaceMouse();

        // Play the animation for attack 1, 2, or 3
        PlayAnimation(attackNumber);

        // Start the VFX with a delay so it lines up with the swing
        StartCoroutine(DelayedVFX(attackNumber, id));

        // Start the damage window so enemies can be hit
        StartCoroutine(DamageWindow(id));

        // Wait for the attack duration
        yield return new WaitForSeconds(attackDuration);

        // Only end the attack if this is still the newest attack
        if (id == attackId) {
            isAttacking = false;
            attackRoutine = null;
        }
    }

    private void PlayAnimation(int attackNumber) {
        // If there is no Animator, do nothing
        if (animator == null) {
            return;
        }

        // Reset all attack triggers so they do not get stuck
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");

        // Plays Attack1, Attack2, or Attack3 depending on the combo
        animator.SetTrigger("Attack" + attackNumber);
    }

    private IEnumerator DelayedVFX(int attackNumber, int id) {
        // Wait the correct amount of time before playing VFX
        yield return new WaitForSeconds(GetVFXDelay(attackNumber));

        // Only play VFX if this attack is still the newest attack
        if (id == attackId) {
            PlayVFX(GetVFX(attackNumber));
        }
    }

    private void PlayVFX(ParticleSystem vfx) {
        // If no VFX was assigned, do nothing
        if (vfx == null) {
            return;
        }

        // Stops the VFX and clears old particles
        vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Plays the VFX from the beginning
        vfx.Play();
    }

    private ParticleSystem GetVFX(int attackNumber) {
        // If this is attack 1, return attack 1 VFX
        if (attackNumber == 1) {
            return attack1VFX;
        }

        // If this is attack 2, return attack 2 VFX
        if (attackNumber == 2) {
            return attack2VFX;
        }

        // Otherwise return attack 3 VFX
        return attack3VFX;
    }

    private float GetVFXDelay(int attackNumber) {
        // Attack 1 has its own VFX delay
        if (attackNumber == 1) {
            return attack1VFXDelay;
        }

        // Attack 2 has its own VFX delay
        if (attackNumber == 2) {
            return attack2VFXDelay;
        }

        // Attack 3 uses this delay
        return attack3VFXDelay;
    }

    private IEnumerator DamageWindow(int id) {
        // Wait before damage starts so it matches the attack animation
        yield return new WaitForSeconds(hitStartDelay);

        // Timer for how long the damage hitbox stays active
        float timer = 0f;

        // This keeps the same enemy from getting hit multiple times by one attack
        HashSet<EnemyReceiveDamage> damagedEnemies = new HashSet<EnemyReceiveDamage>();

        // Keep checking hitbox while the hit window is active
        while (timer < hitActiveTime && id == attackId) {
            // Checks if any enemies are inside the attack box
            CheckHitbox(damagedEnemies);

            // Adds time to the timer
            timer += Time.deltaTime;

            // Waits until next frame
            yield return null;
        }
    }

    private void CheckHitbox(HashSet<EnemyReceiveDamage> damagedEnemies) {
        // If there is no attack point, we cannot know where to check
        if (attackPoint == null) {
            return;
        }

        // Creates an invisible box and gets all colliders inside it
        Collider[] hits = Physics.OverlapBox(
            attackPoint.position,
            hitboxSize * 0.5f,
            attackPoint.rotation,
            enemyMask
        );

        // Loops through everything hit by the box
        foreach (Collider hit in hits) {
            // Tries to find EnemyReceiveDamage on the object or its parent
            EnemyReceiveDamage enemy = hit.GetComponentInParent<EnemyReceiveDamage>();

            // If enemy exists and has not been hit by this swing yet, damage it
            if (enemy != null && damagedEnemies.Add(enemy)) {
                enemy.Hit(damage);
            }
        }
    }

    private void FaceMouse() {
        // If there is no camera, do nothing
        if (Camera.main == null) {
            return;
        }

        // Creates a ray from the mouse position into the world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Makes an invisible flat ground plane at the player's height
        Plane ground = new Plane(Vector3.up, transform.position);

        // Checks where the mouse ray hits the ground plane
        if (ground.Raycast(ray, out float distance)) {
            // Gets the point where the ray hit the ground
            Vector3 mousePoint = ray.GetPoint(distance);

            // Gets direction from player to mouse
            Vector3 direction = mousePoint - transform.position;

            // Keeps the player from looking up or down
            direction.y = 0f;

            // Makes sure the direction is big enough to use
            if (direction.sqrMagnitude > 0.0001f) {
                // Faces the player toward the mouse
                transform.forward = direction.normalized;
            }
        }
    }

    void OnDrawGizmosSelected() {
        // If there is no attack point, do not draw the hitbox
        if (attackPoint == null) {
            return;
        }

        // Makes the gizmo red
        Gizmos.color = Color.red;

        // Saves the old gizmo matrix so we can put it back later
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // Moves and rotates the gizmo to match the attack point
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);

        // Draws the attack hitbox in the scene view
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);

        // Puts the gizmo matrix back to normal
        Gizmos.matrix = oldMatrix;
    }

    public void ResetComboFromDash() {
        // Makes old attack coroutines stop mattering
        attackId++;

        // Resets the combo back to the beginning
        comboStep = 0;

        // Makes the last attack time really far in the past
        lastAttackTime = -999f;

        // Allows attacking right after dash
        nextAttackTime = Time.time;

        // Says player is no longer attacking
        isAttacking = false;

        // Removes attack movement slow
        attackSlowTimer = 0f;

        // Stops the current attack routine if it exists
        if (attackRoutine != null) {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        // Resets attack animation triggers if there is an Animator
        if (animator != null) {
            animator.ResetTrigger("Attack1");
            animator.ResetTrigger("Attack2");
            animator.ResetTrigger("Attack3");
        }

        // Stops all attack VFX
        StopAttackVFX(attack1VFX);
        StopAttackVFX(attack2VFX);
        StopAttackVFX(attack3VFX);
    }

    private void StopAttackVFX(ParticleSystem vfx) {
        // If this VFX exists, stop and clear it
        if (vfx != null) {
            vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void SetDamage(int newDamage) {
        // Sets the player's attack damage
        damage = newDamage;
    }

    public int GetDamage() {
        // Returns the player's current attack damage
        return damage;
    }

    public void SetAttackCooldown(float newCooldown) {
        // Sets the attack cooldown
        attackCooldown = newCooldown;
    }

    public float GetAttackCooldown() {
        // Returns the current attack cooldown
        return attackCooldown;
    }

    public void SetAttackRange(float newRange) {
        // Changes the forward size of the attack hitbox
        hitboxSize.z = newRange;
    }

    public float GetAttackRange() {
        // Returns the current attack range
        return hitboxSize.z;
    }
}