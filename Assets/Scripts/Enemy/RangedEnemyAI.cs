using UnityEngine;
using UnityEngine.AI;

// This script controls a ranged enemy.
// The enemy can roam, chase the player, shoot the player, and run away if the player gets too close.
[RequireComponent(typeof(NavMeshAgent))]
public class RangedEnemyAI : MonoBehaviour
{
    [Header("Detection")]
    // How far away the enemy can notice the player
    public float detectionRange = 15f;

    [Header("Ranged Distances")]
    // How close the enemy needs to be before it can shoot
    public float shootRange = 8f;

    // If the player gets closer than this, the enemy runs away
    public float retreatRange = 4f;

    // How far the enemy tries to move away when retreating
    public float retreatDistance = 5f;

    [Header("Attack")]
    // Time between each shot
    public float attackCooldown = 1.5f;

    [Header("Rotation")]
    // How quickly the enemy turns to face a direction
    public float faceSpeed = 10f;

    [Header("Roaming")]
    // If true, the enemy wanders around when it does not see the player
    public bool enableRoaming = true;

    // How far the enemy can wander from its current position
    public float roamRadius = 6f;

    // How long the enemy waits before picking a new roaming spot
    public float roamWaitTime = 2f;

    // Reference to the player's transform
    private Transform player;

    // NavMeshAgent moves the enemy around the level
    private NavMeshAgent agent;

    // Animator controls enemy animations
    private Animator animator;

    // EnemyShoot handles creating and firing bullets
    private EnemyShoot shooter;

    // Keeps track of when the enemy is allowed to shoot again
    private float nextAttackTime;

    // Timer used for waiting between roaming movements
    private float roamTimer;

    // Name of the attack trigger in the Animator
    private const string ATTACK_TRIGGER = "Attack";

    // These are the different things the enemy can currently be doing
    private enum State
    {
        Roam,      // Wander around
        Chase,     // Move toward the player
        Shoot,     // Stop and shoot the player
        Retreat    // Run away from the player
    }

    // Enemy starts by roaming
    private State currentState = State.Roam;

    void Awake()
    {
        // Get needed components from this enemy
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        shooter = GetComponent<EnemyShoot>();
    }

    void Start()
    {
        // Find the player by looking for the Player tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("RangedEnemyAI could not find a GameObject tagged 'Player'.");
        }

        // Start the roaming timer
        roamTimer = roamWaitTime;
    }

    void Update()
    {
        // If there is no player, do nothing
        if (player == null)
            return;

        // Measure how far the enemy is from the player
        float distance = Vector3.Distance(transform.position, player.position);

        // Run the correct behavior based on the current state
        switch (currentState)
        {
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

        // When not shooting or retreating, face the direction the enemy is moving
        if (currentState != State.Retreat && currentState != State.Shoot)
        {
            FaceMovementDirection();
        }
    }

    void HandleRoam(float distance)
    {
        // If the player is close enough to detect, switch to a combat state
        if (distance <= detectionRange)
        {
            if (distance < retreatRange)
                currentState = State.Retreat;
            else if (distance <= shootRange)
                currentState = State.Shoot;
            else
                currentState = State.Chase;

            return;
        }

        // If roaming is turned off, stop moving
        if (!enableRoaming)
        {
            agent.isStopped = true;
            return;
        }

        // Count down before choosing a new roam destination
        roamTimer -= Time.deltaTime;

        // If the enemy reached its roam point, pick a new one after the timer ends
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (roamTimer <= 0f)
            {
                SetRandomRoamDestination();
                roamTimer = roamWaitTime;
            }
        }
    }

    void HandleChase(float distance)
    {
        // Move toward the player
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // If the player gets too close, run away
        if (distance < retreatRange)
        {
            currentState = State.Retreat;
        }
        // If close enough to shoot, stop chasing and shoot
        else if (distance <= shootRange)
        {
            currentState = State.Shoot;
        }
        // If the player gets too far away, go back to roaming
        else if (distance > detectionRange)
        {
            currentState = State.Roam;
            roamTimer = roamWaitTime;
        }
    }

    void HandleShoot(float distance)
    {
        // Stop moving while shooting
        agent.isStopped = true;

        // Face the player while shooting
        FacePlayer();

        // If the player gets too close, run away
        if (distance < retreatRange)
        {
            currentState = State.Retreat;
            return;
        }

        // If the player leaves shooting range, chase them again
        if (distance > shootRange)
        {
            currentState = State.Chase;
            return;
        }

        // Shoot if the cooldown is ready
        if (Time.time >= nextAttackTime)
        {
            AttackPlayer();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void HandleRetreat(float distance)
    {
        // If the enemy has backed up enough, start shooting again
        if (distance >= retreatRange && distance <= shootRange)
        {
            currentState = State.Shoot;
            return;
        }

        // If the player is now too far away, chase again
        if (distance > shootRange)
        {
            currentState = State.Chase;
            return;
        }

        // Get the direction directly away from the player.
        // This creates the true 180-degree opposite direction from the player.
        Vector3 runDirection = transform.position - player.position;
        runDirection.y = 0f;
        runDirection.Normalize();

        // Pick a point farther away in that direction
        Vector3 desiredRetreatPoint = transform.position + runDirection * retreatDistance;

        NavMeshHit hit;

        // Check if that retreat point is actually on the NavMesh
        if (NavMesh.SamplePosition(desiredRetreatPoint, out hit, 3f, NavMesh.AllAreas))
        {
            // Move toward the safe retreat point
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
        else
        {
            // If there is no valid retreat spot, stop moving
            agent.isStopped = true;
        }

        // Face the same direction the enemy is running.
        // This fixes the problem where the enemy was running away sideways.
        FaceDirection(runDirection);
    }

    void AttackPlayer()
    {
        // Play the attack animation
        if (animator != null)
        {
            animator.SetTrigger(ATTACK_TRIGGER);
        }

        // Fire the projectile
        if (shooter != null)
        {
            shooter.Shoot();
        }
        else
        {
            Debug.LogWarning("RangedEnemyAI: No EnemyShoot component found.");
        }
    }

    void FacePlayer()
    {
        // Direction from enemy to player
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        FaceDirection(direction);
    }

    void FaceMovementDirection()
    {
        // If the enemy is moving, face the direction of movement
        if (agent.velocity.sqrMagnitude > 0.05f)
        {
            Vector3 direction = agent.velocity.normalized;
            direction.y = 0f;

            FaceDirection(direction);
        }
    }

    void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;

        // Only rotate if the direction is big enough to be useful
        if (direction.sqrMagnitude > 0.001f)
        {
            // Turn the enemy toward the chosen direction
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // Smoothly rotate instead of snapping instantly
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                faceSpeed * Time.deltaTime
            );
        }
    }

    void SetRandomRoamDestination()
    {
        // Pick a random point around the enemy
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        NavMeshHit hit;

        // Make sure the random point is on the NavMesh
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
        {
            // Move to the random roaming point
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }
}