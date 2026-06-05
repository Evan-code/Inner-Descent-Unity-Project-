using UnityEngine;
using UnityEngine.AI;

// This script controls a melee enemy
// It lets the enemy roam around, chase the player, and attack when it gets close enough
[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemyAI : MonoBehaviour {
    [Header("Detection")]
    public float detectionRange = 12f;
    public float attackRange = 2f;

    [Header("Attack")]
    public float attackCooldown = 1f;
    public int damage = 1;

    [Header("Rotation")]
    public float faceSpeed = 10f;

    [Header("Roaming")]
    public bool enableRoaming = true;
    public float roamRadius = 6f;
    public float roamWaitTime = 2f;

    // This stores the player position so the enemy knows who to chase
    private Transform player;

    // NavMeshAgent is what actually moves the enemy around the map
    private NavMeshAgent agent;

    // Animator is used to play animations like attacking
    private Animator animator;

    // This stores the next time the enemy is allowed to attack again
    private float nextAttackTime;

    // This timer controls how long the enemy waits before picking a new roam spot
    private float roamTimer;

    // This is the exact name of the attack trigger inside the Animator
    private const string ATTACK_TRIGGER = "Attack";

    // These are the different states the enemy can be in
    private enum State {
        Roam,
        Chase,
        Attack
    }

    // Enemy starts by roaming
    private State currentState = State.Roam;

    void Awake() {
        // Gets the NavMeshAgent component on this enemy
        agent = GetComponent<NavMeshAgent>();

        // Gets the Animator component on this enemy if it has one
        animator = GetComponent<Animator>();
    }

    void Start() {
        // Looks for the player object using the Player tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        // Checks if the player was actually found
        if (playerObj != null) {
            // Saves the player's transform so we can track their position
            player = playerObj.transform;
        } else {
            // Warns us if there is no object tagged Player
            Debug.LogWarning("MeleeEnemyAI could not find a GameObject tagged 'Player'.");
        }

        // Starts the roam timer at the wait time
        roamTimer = roamWaitTime;
    }

    void Update() {
        // If there is no player, the enemy should do nothing
        if (player == null) {
            return;
        }

        // Measures how far away the enemy is from the player
        float distance = Vector3.Distance(transform.position, player.position);

        // Checks what state the enemy is in and runs the correct code
        switch (currentState) {
            case State.Roam:
                HandleRoam(distance);
                break;

            case State.Chase:
                HandleChase(distance);
                break;

            case State.Attack:
                HandleAttack(distance);
                break;
        }

        // Makes the enemy face where it is moving, unless attacking
        FaceMovementDirection();
    }

    private void HandleRoam(float distance) {
        // If the player is close enough, switch to chasing
        if (distance <= detectionRange) {
            currentState = State.Chase;
            return;
        }

        // If roaming is turned off, the enemy just stands still
        if (!enableRoaming) {
            agent.isStopped = true;
            return;
        }

        // Counts down the timer before choosing a new roam spot
        roamTimer -= Time.deltaTime;

        // Checks if the enemy is done moving to its current roam spot
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) {
            // If the timer is done, pick a new random roam spot
            if (roamTimer <= 0f) {
                SetRandomRoamDestination();

                // Resets the timer for the next roam movement
                roamTimer = roamWaitTime;
            }
        }
    }

    private void HandleChase(float distance) {
        // Makes sure the enemy is allowed to move
        agent.isStopped = false;

        // Tells the enemy to move toward the player
        agent.SetDestination(player.position);

        // If close enough to attack, switch to attack mode
        if (distance <= attackRange) {
            currentState = State.Attack;
        }
        // If the player gets too far away, go back to roaming
        else if (distance > detectionRange) {
            currentState = State.Roam;
            roamTimer = roamWaitTime;
        }
    }

    private void HandleAttack(float distance) {
        // Stop moving while attacking
        agent.isStopped = true;

        // Make the enemy look at the player while attacking
        FacePlayer();

        // If the player moves away, chase again
        if (distance > attackRange) {
            currentState = State.Chase;
            return;
        }

        // Only attack if the cooldown is finished
        if (Time.time >= nextAttackTime) {
            AttackPlayer();

            // Sets the next time the enemy is allowed to attack
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void AttackPlayer() {
        // Plays the attack animation if there is an Animator
        if (animator != null) {
            animator.SetTrigger(ATTACK_TRIGGER);
        }

        // Gets the player's damage receiving script
        PlayerReceiveDamage playerDamage = player.GetComponent<PlayerReceiveDamage>();

        // If the player has the script, damage the player
        if (playerDamage != null) {
            playerDamage.Hit(damage);
        }
    }

    private void FacePlayer() {
        // Gets the direction from the enemy to the player
        Vector3 direction = player.position - transform.position;

        // Keeps the enemy from looking up or down
        direction.y = 0f;

        // Makes sure the direction is big enough to rotate toward
        if (direction.sqrMagnitude > 0.001f) {
            // Creates the rotation that faces the player
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // Smoothly rotates toward the player instead of snapping instantly
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, faceSpeed * Time.deltaTime);
        }
    }

    private void FaceMovementDirection() {
        // If attacking, FacePlayer handles rotation instead
        if (currentState == State.Attack) {
            return;
        }

        // Checks if the enemy is actually moving
        if (agent.velocity.sqrMagnitude > 0.05f) {
            // Gets the direction the enemy is moving
            Vector3 direction = agent.velocity.normalized;

            // Keeps rotation flat on the ground
            direction.y = 0f;

            // Makes sure the direction is big enough to use
            if (direction.sqrMagnitude > 0.001f) {
                // Creates a rotation facing the movement direction
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Smoothly turns the enemy toward where it is moving
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, faceSpeed * Time.deltaTime);
            }
        }
    }

    private void SetRandomRoamDestination() {
        // Picks a random direction around the enemy
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;

        // Moves that random point near the enemy's current position
        randomDirection += transform.position;

        // Keeps the random point on the same height level as the enemy
        randomDirection.y = transform.position.y;

        // Checks if the random point is actually on the NavMesh
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, roamRadius, NavMesh.AllAreas)) {
            // Lets the enemy move again
            agent.isStopped = false;

            // Sends the enemy to the random roam position
            agent.SetDestination(hit.position);
        }
    }
}