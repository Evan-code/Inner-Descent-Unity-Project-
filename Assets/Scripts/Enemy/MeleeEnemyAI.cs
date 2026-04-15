using UnityEngine;
using UnityEngine.AI;

// This script controls a melee enemy.
// It can roam when idle, chase the player, and attack up close.
[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemyAI : MonoBehaviour
{
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

    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private float nextAttackTime;
    private float roamTimer;

    private const string ATTACK_TRIGGER = "Attack";

    private enum State
    {
        Roam,
        Chase,
        Attack
    }

    private State currentState = State.Roam;

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
            Debug.LogWarning("MeleeEnemyAI could not find a GameObject tagged 'Player'.");
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

            case State.Attack:
                HandleAttack(distance);
                break;
        }

        FaceMovementDirection();
    }

    void HandleRoam(float distance)
    {
        if (distance <= detectionRange)
        {
            currentState = State.Chase;
            return;
        }

        if (!enableRoaming)
        {
            agent.isStopped = true;
            return;
        }

        roamTimer -= Time.deltaTime;

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

        if (distance <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (distance > detectionRange)
        {
            currentState = State.Roam;
            roamTimer = roamWaitTime;
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

        PlayerReceiveDamage playerDamage = player.GetComponent<PlayerReceiveDamage>();

        if (playerDamage != null)
        {
            playerDamage.Hit(damage);
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
        if (currentState == State.Chase || currentState == State.Roam)
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