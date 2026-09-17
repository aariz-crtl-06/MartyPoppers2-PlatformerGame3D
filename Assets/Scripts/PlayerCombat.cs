using UnityEngine;
using Unity.Cinemachine;
using System;
public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    PlayerController playerControl;
    CharacterController controller;
    EnemyAI enemyAI;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] CinemachineImpulseSource impulseSource;
    [SerializeField] float damageShakeForce = 0.5f;
    [SerializeField] float attackShakeForce = 0.2f;
    Animator animator;

    [Header("Combat Settings")]
    [SerializeField] float maxAttackAngle = 60f;
    [SerializeField] float attackRadius = 2f;
    [SerializeField] float spinRadius = 4f;
    [SerializeField] float groundPoundRadius = 6f;
    [SerializeField] float ultimateRadius = 8f;
    [SerializeField] int attackDamage = 1;
    [SerializeField] int spinDamage = 3;
    [SerializeField] int groundPoundDamage = 3;
    [SerializeField] int ultimateDamage = 5;
    [SerializeField] float slamSpeed = 20f;

    [Header("Attack State")]
    bool canStrike = true;
    bool canSpin = true;
    bool canGroundPound = false;
    bool canUltimate = false;
    bool isStriking = false; // tracks whether the strike animation is currently playing
    bool isSpinning = false; //tracks animation
    bool isGroundPounding = false; //tracks animation

    [Header("Attack Cooldowns")]
    [SerializeField] float strikeCooldown = 0.5f;
    [SerializeField] float spinCooldown = 5f;
    [SerializeField] float groundPoundCooldown = 10f;
    [SerializeField] float ultimateCooldown = 30f;
    [SerializeField] float strikeAnimLength = 0.4f; // how long the strike animation actually takes
    [SerializeField] float spinAnimLength = 0.8f; // how long the strike animation actually takes
    [SerializeField] float groundPoundAnimLength = 0.8f;

    [Header("Attack Timers")]
    float strikeTimer = 0.5f;
    float spinTimer = 5f;
    float groundPoundTimer = 10f;
    float ultimateTimer = 0f;

    [Header("Health")]
    public int playerHealth;
    public int maxHealth = 6;
    public event Action OnPlayerDeath;

    private void Strike()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            Vector3 directionToTarget = (hitCollider.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle <= maxAttackAngle)
            {
                IDamageable enemy = hitCollider.GetComponentInParent<IDamageable>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                }

                impulseSource.GenerateImpulseWithForce(attackShakeForce);
            }
        }
    }
    private void Spin()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, spinRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            IDamageable enemy = hitCollider.GetComponentInParent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(spinDamage);
                impulseSource.GenerateImpulseWithForce(attackShakeForce);
            }
        }

    }

    private void GroundPound()
    {
        playerControl.GroundPoundSlam(slamSpeed); //slam player downwards

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, groundPoundRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            IDamageable enemy = hitCollider.GetComponentInParent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(groundPoundDamage);
                impulseSource.GenerateImpulseWithForce(attackShakeForce);
            }
                
        }
    }

    private void Ultimate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ultimateRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            IDamageable enemy = hitCollider.GetComponentInParent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(ultimateDamage);
                impulseSource.GenerateImpulseWithForce(attackShakeForce);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        playerHealth -= damage;
        if (impulseSource != null)
            impulseSource.GenerateImpulseWithForce(damageShakeForce);
        Debug.Log("Player took " + damage + " damage. Remaining health: " + playerHealth);
        if (playerHealth <= 0)
        {
            OnPlayerDeath?.Invoke();
        }
    }

    public void Respawn()
    {
        controller.enabled = false;
        transform.position = CheckpointManager.Instance.CurrentRespawnPosition;
        controller.enabled = true;
        playerHealth = maxHealth;
    }

    public void Kill()
    {
        playerHealth = 0;
        OnPlayerDeath?.Invoke();
    }
    void Start()
    {
        playerControl = GetComponent<PlayerController>();
        controller = GetComponent<CharacterController>();
        enemyAI = GetComponent<EnemyAI>();
        animator = GetComponentInChildren<Animator>();
    }
    
    void Update()
    {
        strikeTimer += Time.deltaTime;
        spinTimer += Time.deltaTime;
        groundPoundTimer += Time.deltaTime;
        ultimateTimer += Time.deltaTime;

        if (strikeTimer >= strikeCooldown)
        {
            canStrike = true;
        }
        else
        {
            canStrike = false;
        }

        if (spinTimer >= spinCooldown)
        {
            canSpin = true;
        }
        else
        {
            canSpin = false;
        }

        if (groundPoundTimer >= groundPoundCooldown)
        {
            canGroundPound = true;
        }
        else
        {
            canGroundPound = false;
        }

        if (ultimateTimer >= ultimateCooldown)
        {
            canUltimate = true;
        }
        else
        {
            canUltimate = false;
        }

        // Turn off the strike animation once it's had time to play out
        if (isStriking && strikeTimer >= strikeAnimLength)
        {
            animator.SetBool("isStriking", false);
            isStriking = false;
        }

        // Turn off the spin animation once it's had time to play out
        if (isSpinning && spinTimer >= spinAnimLength)
        {
            animator.SetBool("isSpinning", false);
            isSpinning = false;
        }

        // Turn off the groundpound animation once it's had time to play out
        if (isGroundPounding && groundPoundTimer >= groundPoundAnimLength)
        {
            animator.SetBool("isGroundPounding", false);
            isGroundPounding = false;
        }

        //Fire commands for each attack type, with conditions for gliding and attack availability

        // Compute this FIRST so the other checks below can reference it
        //Accounts for slack by allowing different ways to fire both buttons
        if (Input.GetButtonDown("Fire1") && canStrike && controller.isGrounded)
        {
            Strike();
            animator.SetBool("isStriking", true);
            strikeTimer = 0f;
            isStriking = true;
        }


        if (Input.GetButtonDown("Fire2") && canSpin && controller.isGrounded)
        {
            Spin();
            animator.SetBool("isSpinning", true);
            spinTimer = 0f;
            isSpinning = true;
        }

        bool inAir = !controller.isGrounded || playerControl.isGliding;

        if (Input.GetButtonDown("Fire1") && canGroundPound && inAir)
        {
            GroundPound();
            animator.SetBool("isGroundPounding", true);
            groundPoundTimer = 0f;
            isGroundPounding = true;
        }

        if (Input.GetButtonDown("Fire3") && canUltimate && playerControl.isGliding == false)
        {
            Ultimate();
            ultimateTimer = 0f;
        }
    }
}