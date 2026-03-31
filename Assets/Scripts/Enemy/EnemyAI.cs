using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 12f;
    public float attackRange = 2f;

    [Header("Attack")]
    public float attackCooldown = 1f;
    public int damage = 1;

    [Header("Rotation")]
    public float faceSpeed = 10f;

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private float nextAttackTime;

    private const string ATTACK_TRIGGER = "Attack";

    private enum State
    {
        Idle,
        Chase,
        Attack
    }

    private State currentState = State.Idle;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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

            case State.Attack:
                HandleAttack(distance);
                break;
        }

        FaceMovementDirection();
    }

    void HandleIdle(float distance)
    {
        agent.isStopped = true;

        if (distance <= detectionRange)
        {
            currentState = State.Chase;
        }
    }

    void HandleChase(float distance)
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (distance <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (distance > detectionRange)
        {
            currentState = State.Idle;
        }
    }

    void HandleAttack(float distance)
    {
        agent.isStopped = true;
        FacePlayer();

        if (distance > attackRange)
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

    void AttackPlayer()
    {
        if (animator != null)
        {
            animator.SetTrigger(ATTACK_TRIGGER);
        }

        PlayerReceiveDamage pd = player.GetComponent<PlayerReceiveDamage>();

        if (pd != null)
        {
            pd.Hit(damage);
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
        if (currentState == State.Chase && agent.velocity.sqrMagnitude > 0.05f)
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