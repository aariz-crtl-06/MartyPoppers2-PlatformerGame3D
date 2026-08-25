using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform playerTarget;
    NavMeshAgent agent;
    PlayerCombat playerCombat;

    [Header("Enemy Settings")]
    [SerializeField] float chaseRange = 10f;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float retreatDistance = 4f;
    [SerializeField] float damage = 1f;
    [SerializeField] float attackCooldown = 1f;
    float attackTimer = 0f;
    Vector3 destination;

    [Header("Health")]
    [SerializeField] int health = 3;

    [Header("Checks")]
    bool isChasing = false;
    bool isAttacking = false;
    bool isRetreating = false;
    private void Patrol()
    {
        // Only pick a new random point once actually arrived at the current one
        if (!agent.pathPending && agent.remainingDistance <= 1f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 10f;

            randomOffset.y = 0f; // keep the patrol point on the same horizontal plane

            destination = transform.position + randomOffset; // Set the new destination

            agent.SetDestination(destination);
        }
        if (playerTarget != null)
        {
            //Compare distance to player with chase range, if within range, set isChasing to true
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= chaseRange)
            {
                isChasing = true;
            }
        }
    }
    private void ChasePlayer()
    {
        // Set the destination to the player's position
        agent.SetDestination(playerTarget.position);

        //If the distance to the player is < attack range, set attack mode
        if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
        {
            isChasing = false; // Stop chasing
            isAttacking = true; // Start attacking
            agent.isStopped = true;
        }
    }
    private void AttackPlayer()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown)
        {
            // Attack logic here
            playerCombat.TakeDamage((int)damage);
            attackTimer = 0f; // Reset the attack timer
            isAttacking = false; // Reset attacking state after attack
            isRetreating = true; // Set retreating state after attack

            // Create a point directly away from the player, once, right as enemy will enter retreat
            Vector3 retreatDirection = (transform.position - playerTarget.position).normalized;
            destination = transform.position + retreatDirection * retreatDistance;
            agent.SetDestination(destination);
            agent.isStopped = false; // resume movement now that the attack landed
        }

        else if (Vector3.Distance(transform.position, playerTarget.position) > attackRange)
        {
            // Player escaped before the attack finished charging
            isAttacking = false;
            isChasing = true;
            agent.isStopped = false;
            attackTimer = 0f; // don't carry progress into the next attempt
        }
    }
    private void Retreat()
    {
        // Wait till enemy arrived at the retreat point
        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            isRetreating = false;
            isChasing = true;
        }
    }

    public void TakeDamage(int damageTaken)
    {
        health -= damageTaken;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerCombat = playerTarget.GetComponent<PlayerCombat>();
    }
    void Update()
    {
        if (isChasing)
        {
            ChasePlayer();
        }

        else if (isAttacking)
        {
            AttackPlayer();
        }

        else if (isRetreating)
        {
            Retreat();
        }

        else
        {
            Patrol();
        }
    }
}