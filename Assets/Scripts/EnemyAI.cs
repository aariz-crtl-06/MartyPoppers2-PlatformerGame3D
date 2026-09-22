using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
public class EnemyAI : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] Transform playerTarget;
    NavMeshAgent agent;
    PlayerCombat playerCombat;
    BossAI spawner;
    Animator animator;
    [SerializeField] GameObject dustPrefab;
    public AudioSource hitSound;
    public AudioSource deathSound;

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

    [Header("Death")]
    [SerializeField] float deathDuration = 0.6f;
    [SerializeField] float deathHop = 1f;   // how high it pops as it flips
    bool isDead = false;
    [SerializeField] float dustLifetime = 3f;

    [Header("Checks")]
    bool isChasing = false;
    bool isAttacking = false;
    bool isRetreating = false;

    [Header("Damage Flash")]
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashDuration = 0.3f;

    class FlashPiece
    {
        public Renderer renderer;
        public int materialIndex;
        public int colorId;
        public Color original;
    }

    List<FlashPiece> flashPieces = new List<FlashPiece>();
    MaterialPropertyBlock block;
    float flashTimer = 0f;
    private void Patrol()
    {
        
        animator.SetBool("isAttacking", false);
        // Only pick a new random point once actually arrived at the current one
        if (!agent.pathPending && agent.remainingDistance <= 1f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 5f;

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
       
        animator.SetBool("isAttacking", false);
        // Set the destination to the player's position
        agent.SetDestination(playerTarget.position);

        //If the distance to the player is < attack range, set attack mode
        if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
        {
            animator.SetBool("isAttacking", true);
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
            animator.SetBool("isAttacking", false);
        }

        else if (Vector3.Distance(transform.position, playerTarget.position) > attackRange)
        {
            // Player escaped before the attack finished charging
            animator.SetBool("isAttacking", false);
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
        if (isDead) return;          // guard: a second hit must not re-trigger death
        flashTimer = flashDuration;

        hitSound.PlayOneShot(hitSound.clip);
        health -= damageTaken;

        if (health <= 0)
        {
            isDead = true;
            if (spawner != null) spawner.OnMinionDefeated();
            StartCoroutine(DeathSequence());
        }
    }

    public void SetupAsMinion(Transform player, BossAI boss)
    {
        playerTarget = player;
        spawner = boss;
        isChasing = true;              // skip patrol, go straight for the player
    }

    private IEnumerator DeathSequence()
    {
        deathSound.PlayOneShot(deathSound.clip);
        // Hand control of the transform back to us
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        if (animator != null) animator.enabled = false;

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        // Animate the model child if there is one, otherwise the root
        Transform model = (animator != null) ? animator.transform : transform;
        SpawnDust(dustPrefab, transform.position);

        Quaternion startRot = model.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(180f, 0f, 0f);
        Vector3 startScale = model.localScale;
        Vector3 startPos = model.position;

        float elapsed = 0f;
        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / deathDuration);

            model.rotation = Quaternion.Slerp(startRot, endRot, t);
            model.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            model.position = startPos + Vector3.up * Mathf.Sin(t * Mathf.PI) * deathHop;

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);


        Destroy(gameObject);
    }

    private void SpawnDust(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;
        GameObject dust = Instantiate(prefab, position, Quaternion.identity);
        Destroy(dust, dustLifetime);
    }


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerCombat = playerTarget.GetComponent<PlayerCombat>();
        animator = GetComponentInChildren<Animator>();

        block = new MaterialPropertyBlock();

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            Material[] mats = r.sharedMaterials;

            for (int i = 0; i < mats.Length; i++)
            {
                Material m = mats[i];
                if (m == null) continue;

                // Skip Quick Outline's mask and fill materials
                if (m.shader.name.IndexOf("outline", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                int id;
                if (m.HasProperty("_BaseColor")) id = Shader.PropertyToID("_BaseColor");
                else if (m.HasProperty("_Color")) id = Shader.PropertyToID("_Color");
                else continue;

                flashPieces.Add(new FlashPiece
                {
                    renderer = r,
                    materialIndex = i,
                    colorId = id,
                    original = m.GetColor(id)
                });
            }
        }
    }

    void UpdateFlash()
    {
        if (flashTimer <= 0f) return;

        flashTimer -= Time.deltaTime;
        float t = Mathf.Clamp01(flashTimer / flashDuration);

        foreach (FlashPiece p in flashPieces)
        {
            if (p.renderer == null) continue;

            if (t > 0f)
            {
                block.SetColor(p.colorId, Color.Lerp(p.original, flashColor, t));
                p.renderer.SetPropertyBlock(block, p.materialIndex);
            }
            else
            {
                // Flash is over: remove the override so the material looks normal again
                p.renderer.SetPropertyBlock(null, p.materialIndex);
            }
        }
    }
    void Update()
    {
        UpdateFlash();

        if (isDead) return;

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