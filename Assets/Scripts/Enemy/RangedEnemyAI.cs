using UnityEngine;
using UnityEngine.AI;

// This script controls a ranged enemy
// The enemy can roam, chase, shoot, and run away if the player gets too close
[RequireComponent(typeof(NavMeshAgent))]
public class RangedEnemyAI : MonoBehaviour {
    [Header("Detection")]
    public float detectionRange = 15f;

    [Header("Ranged Distances")]
    public float shootRange = 8f;
    public float retreatRange = 4f;
    public float retreatDistance = 5f;

    [Header("Attack")]
    public float attackCooldown = 1.5f;

    [Header("Rotation")]
    public float faceSpeed = 10f;

    [Header("Roaming")]
    public bool enableRoaming = true;
    public float roamRadius = 6f;
    public float roamWaitTime = 2f;

    // This stores the player's position
    private Transform player;

    // This moves the enemy around using Unity's NavMesh system
    private NavMeshAgent agent;

    // This controls enemy animations
    private Animator animator;

    // This script handles actually spawning and firing the bullet
    private EnemyShoot shooter;

    // This stores the next time the enemy is allowed to shoot
    private float nextAttackTime;

    // This timer is used for roaming
    private float roamTimer;

    // This is the exact trigger name in the Animator
    private const string ATTACK_TRIGGER = "Attack";

    // These are the possible states the ranged enemy can be in
    private enum State {
        Roam,
        Chase,
        Shoot,
        Retreat
    }

    // The enemy starts by roaming around
    private State currentState = State.Roam;

    void Awake() {
        // Gets the NavMeshAgent on this enemy
        agent = GetComponent<NavMeshAgent>();

        // Gets the Animator if this enemy has one
        animator = GetComponent<Animator>();

        // Gets the EnemyShoot script so this AI can shoot
        shooter = GetComponent<EnemyShoot>();
    }

    void Start() {
        // Looks for the player using the Player tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        // If the player was found, save their transform
        if (playerObj != null) {
            player = playerObj.transform;
        } else {
            // Warn if there is no player with the Player tag
            Debug.LogWarning("RangedEnemyAI could not find a GameObject tagged 'Player'.");
        }

        // Starts the roaming timer
        roamTimer = roamWaitTime;
    }

    void Update() {
        // If there is no player, the enemy cannot do anything
        if (player == null) {
            return;
        }

        // Finds how far the enemy is from the player
        float distance = Vector3.Distance(transform.position, player.position);

        // Runs the correct behavior based on the enemy's current state
        switch (currentState) {
            case State.Roam:
                HandleRoam(distance);
                break;

            case State.Chase:
                HandleChase(distance);
                break;

            case State.Shoot:
                HandleShoot(distance);
                break;

            case State.Retreat:
                HandleRetreat(distance);
                break;
        }

        // If not shooting or retreating, face the direction the enemy is moving
        if (currentState != State.Retreat && currentState != State.Shoot) {
            FaceMovementDirection();
        }
    }

    private void HandleRoam(float distance) {
        // If the player is close enough to notice, switch to the correct combat state
        if (distance <= detectionRange) {
            // If the player is too close, run away
            if (distance < retreatRange) {
                currentState = State.Retreat;
            }
            // If the player is close enough to shoot, shoot
            else if (distance <= shootRange) {
                currentState = State.Shoot;
            }
            // Otherwise chase until in shooting range
            else {
                currentState = State.Chase;
            }

            // Stop running roam code after switching states
            return;
        }

        // If roaming is off, stop moving
        if (!enableRoaming) {
            agent.isStopped = true;
            return;
        }

        // Count down before picking a new roam spot
        roamTimer -= Time.deltaTime;

        // Checks if the enemy has reached its current roam destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) {
            // If the wait timer is done, pick a new spot
            if (roamTimer <= 0f) {
                SetRandomRoamDestination();

                // Reset the roam wait timer
                roamTimer = roamWaitTime;
            }
        }
    }

    private void HandleChase(float distance) {
        // Make sure the enemy is allowed to move
        agent.isStopped = false;

        // Move toward the player
        agent.SetDestination(player.position);

        // If the player gets too close, retreat
        if (distance < retreatRange) {
            currentState = State.Retreat;
        }
        // If the player is close enough to shoot, stop and shoot
        else if (distance <= shootRange) {
            currentState = State.Shoot;
        }
        // If the player gets too far away, go back to roaming
        else if (distance > detectionRange) {
            currentState = State.Roam;
            roamTimer = roamWaitTime;
        }
    }

    private void HandleShoot(float distance) {
        // Stop moving while shooting
        agent.isStopped = true;

        // Face the player while shooting
        FacePlayer();

        // If the player gets too close, run away
        if (distance < retreatRange) {
            currentState = State.Retreat;
            return;
        }

        // If the player leaves shooting range, chase again
        if (distance > shootRange) {
            currentState = State.Chase;
            return;
        }

        // Shoot only when the cooldown is ready
        if (Time.time >= nextAttackTime) {
            AttackPlayer();

            // Set the next time this enemy can shoot again
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void HandleRetreat(float distance) {
        // If the enemy backed up enough and can still shoot, start shooting again
        if (distance >= retreatRange && distance <= shootRange) {
            currentState = State.Shoot;
            return;
        }

        // If the player is now too far away, chase again
        if (distance > shootRange) {
            currentState = State.Chase;
            return;
        }

        // Gets the direction away from the player
        Vector3 runDirection = transform.position - player.position;

        // Keeps the direction flat on the ground
        runDirection.y = 0f;

        // Makes the direction length 1
        runDirection.Normalize();

        // Picks a point farther away from the player
        Vector3 desiredRetreatPoint = transform.position + runDirection * retreatDistance;

        // Checks if that retreat point is on the NavMesh
        if (NavMesh.SamplePosition(desiredRetreatPoint, out NavMeshHit hit, 3f, NavMesh.AllAreas)) {
            // Let the enemy move
            agent.isStopped = false;

            // Move toward the retreat point
            agent.SetDestination(hit.position);
        } else {
            // If there is no valid retreat point, stop moving
            agent.isStopped = true;
        }

        // Face the direction the enemy is running
        FaceDirection(runDirection);
    }

    private void AttackPlayer() {
        // Play attack animation if the Animator exists
        if (animator != null) {
            animator.SetTrigger(ATTACK_TRIGGER);
        }

        // Shoot a bullet if the shooter script exists
        if (shooter != null) {
            shooter.Shoot();
        } else {
            // Warn if the EnemyShoot script is missing
            Debug.LogWarning("RangedEnemyAI: No EnemyShoot component found.");
        }
    }

    private void FacePlayer() {
        // Gets the direction from enemy to player
        Vector3 direction = player.position - transform.position;

        // Keeps the enemy from looking up or down
        direction.y = 0f;

        // Face that direction
        FaceDirection(direction);
    }

    private void FaceMovementDirection() {
        // Checks if the enemy is actually moving
        if (agent.velocity.sqrMagnitude > 0.05f) {
            // Gets the movement direction
            Vector3 direction = agent.velocity.normalized;

            // Keeps rotation flat
            direction.y = 0f;

            // Face the movement direction
            FaceDirection(direction);
        }
    }

    private void FaceDirection(Vector3 direction) {
        // Keeps rotation flat on the ground
        direction.y = 0f;

        // Only rotate if the direction is big enough
        if (direction.sqrMagnitude > 0.001f) {
            // Creates the rotation that faces the direction
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // Smoothly rotates instead of snapping
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, faceSpeed * Time.deltaTime);
        }
    }

    private void SetRandomRoamDestination() {
        // Picks a random point inside a sphere
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;

        // Moves that random point near this enemy
        randomDirection += transform.position;

        // Keeps it on the same height as the enemy
        randomDirection.y = transform.position.y;

        // Checks if the point is on the NavMesh
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, roamRadius, NavMesh.AllAreas)) {
            // Let the enemy move
            agent.isStopped = false;

            // Send enemy to the random point
            agent.SetDestination(hit.position);
        }
    }
}