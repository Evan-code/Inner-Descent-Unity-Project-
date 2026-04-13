using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RangedEnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 15f;

    [Header("Archer Distances")]
    public float shootRange = 8f;
    public float retreatRange = 4f;
    public float retreatDistance = 6f;

    [Header("Attack")]
    public float attackCooldown = 1.5f;

    [Header("Rotation")]
    public float faceSpeed = 10f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private EnemyShoot shooter;
    private float nextAttackTime;

    private const string ATTACK_TRIGGER = "Attack";

    private enum State
    {
        Idle,
        Chase,
        Shoot,
        Retreat
    }

    private State currentState = State.Idle;

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
            Debug.LogWarning("EnemyAI could not find a GameObject tagged 'Player'.");
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                HandleIdle(distance);
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

    void HandleIdle(float distance)
    {
        agent.isStopped = true;

        if (distance <= detectionRange)
        {
            if (distance < retreatRange)
                currentState = State.Retreat;
            else if (distance <= shootRange)
                currentState = State.Shoot;
            else
                currentState = State.Chase;
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
            currentState = State.Idle;
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
        if (distance > shootRange)
        {
            currentState = State.Chase;
            return;
        }

        if (distance >= retreatRange && distance <= shootRange)
        {
            currentState = State.Shoot;
            return;
        }

        Vector3 runDirection = (transform.position - player.position);
        Vector3 retreatTarget = transform.position + runDirection * retreatDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatTarget, out hit, 2f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
        else
        {
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
            Debug.LogWarning("EnemyAI: No EnemyShoot component found.");
        }
    }

    void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, faceSpeed * Time.deltaTime);
        }
    }

    void FaceMovementDirection()
    {
        if ((currentState == State.Chase || currentState == State.Retreat) && agent.velocity.sqrMagnitude > 0.05f)
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
}