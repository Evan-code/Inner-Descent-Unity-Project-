using UnityEngine;
using UnityEngine.AI;

// This script controls a ranged enemy.
// It can:
// - roam around when the player is far away
// - chase when needed
// - shoot at medium range
// - retreat if the player gets too close
[RequireComponent(typeof(NavMeshAgent))]
public class RangedEnemyAI : MonoBehaviour
{
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

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private EnemyShoot shooter;

    private float nextAttackTime;
    private float roamTimer;

    private const string ATTACK_TRIGGER = "Attack";

    private enum State
    {
        Roam,
        Chase,
        Shoot,
        Retreat
    }

    private State currentState = State.Roam;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        shooter = GetComponent<EnemyShoot>();
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("RangedEnemyAI could not find a GameObject tagged 'Player'.");
        }

        roamTimer = roamWaitTime;
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

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

        FaceMovementDirection();
    }

    void HandleRoam(float distance)
    {
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

        if (!enableRoaming)
        {
            agent.isStopped = true;
            return;
        }

        roamTimer -= Time.deltaTime;

        // If the enemy reached its roam point, wait a bit, then choose another
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
        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (distance < retreatRange)
        {
            currentState = State.Retreat;
        }
        else if (distance <= shootRange)
        {
            currentState = State.Shoot;
        }
        else if (distance > detectionRange)
        {
            currentState = State.Roam;
            roamTimer = roamWaitTime;
        }
    }

    void HandleShoot(float distance)
    {
        agent.isStopped = true;
        FacePlayer();

        if (distance < retreatRange)
        {
            currentState = State.Retreat;
            return;
        }

        if (distance > shootRange)
        {
            currentState = State.Chase;
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            AttackPlayer();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void HandleRetreat(float distance)
    {
        // If player is no longer too close, go back to shoot/chase logic
        if (distance >= retreatRange && distance <= shootRange)
        {
            currentState = State.Shoot;
            return;
        }

        if (distance > shootRange)
        {
            currentState = State.Chase;
            return;
        }

        // Run directly away from the player
        Vector3 runDirection = (transform.position - player.position).normalized;
        Vector3 desiredRetreatPoint = transform.position + runDirection * retreatDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(desiredRetreatPoint, out hit, 3f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
        else
        {
            // If no retreat point is found, just stop
            agent.isStopped = true;
        }

        FacePlayer();
    }

    void AttackPlayer()
    {
        if (animator != null)
        {
            animator.SetTrigger(ATTACK_TRIGGER);
        }

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
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, faceSpeed * Time.deltaTime);
        }
    }

    void FaceMovementDirection()
    {
        if (agent.velocity.sqrMagnitude > 0.05f)
        {
            Vector3 direction = agent.velocity.normalized;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, faceSpeed * Time.deltaTime);
            }
        }
    }

    void SetRandomRoamDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }
}