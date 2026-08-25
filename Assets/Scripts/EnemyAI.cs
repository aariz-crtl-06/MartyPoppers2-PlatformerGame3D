using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform playerTarget;
    NavMeshAgent agent;

    [Header("Enemy Settings")]
    [SerializeField] float chaseRange = 10f;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float patrolSpeed = 3.5f;
    Vector3 destination;

    private void Patrol()
    {

    }

    private void ChasePlayer()
    {

    }

    private void AttackPlayer()
    {

    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        destination = agent.destination;
    }

    void Update()
    {
        
    }
}
