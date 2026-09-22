using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
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
    public AudioSource lightningSound;
    public AudioSource windSound;
    public AudioSource hurtSound;
    public AudioSource healSound;
    public AudioSource respawnSound;
    Animator animator;
    [SerializeField] AnimationClip strikeClip;

    [Header("Combat Settings")]
    [SerializeField] float maxAttackAngle = 90f;
    [SerializeField] float attackRadius = 3f;
    [SerializeField] float spinRadius = 4f;
    [SerializeField] float groundPoundRadius = 6f;
    [SerializeField] float ultimateRadius = 8f;
    [SerializeField] int attackDamage = 1;
    [SerializeField] int spinDamage = 3;
    [SerializeField] int groundPoundDamage = 3;
    [SerializeField] int ultimateDamage = 3;
    [SerializeField] float slamSpeed = 20f;
    [SerializeField] float aimAssistRadius = 4f;
    [SerializeField] float aimAssistAngle = 110f; // how far off-center an enemy can be and still get targeted

    [Header("Attack State")]
    bool canStrike = true;
    bool canSpin = true;
    bool canGroundPound = false;
    bool canUltimate = false;
    bool isStriking = false; // tracks whether the strike animation is currently playing
    bool isSpinning = false; //tracks animation
    bool isGroundPounding = false; //tracks animation
    bool isUltimate = false;

    [Header("Attack Cooldowns")]
    [SerializeField] float strikeCooldown = 0.5f;
    public float spinCooldown = 3f;
    public float groundPoundCooldown = 8f;
    public float ultimateCooldown = 12f;
    [SerializeField] float strikeAnimLength = 0.4f; // how long the strike animation actually takes
    [SerializeField] float spinAnimLength = 0.8f; // how long the strike animation actually takes
    [SerializeField] float ultimateAnimLength = 1f;

    [Header("Attack Timers")]
    float strikeTimer = 0.5f;
   public float spinTimer = 3f;
   public float groundPoundTimer = 8f;
   public float ultimateTimer = 0f;

    float groundPoundStartTime;

    [Header("Health")]
    public int playerHealth;
    public int maxHealth = 6;
    public event Action OnPlayerDeath;

    [Header("Damage Flash")]
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashDuration = 0.5f;
    [SerializeField] SkinnedMeshRenderer bodyRenderer;
    List<Material> flashMaterials = new List<Material>();
    List<Color> originalColors = new List<Color>();
    float flashTimer = 0f;
    [SerializeField] Color healFlashColor = Color.green;
    Color currentFlashColor;

    private Transform FindStrikeTarget()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, aimAssistRadius, enemyLayer);
        Transform best = null;
        float bestScore = float.MaxValue;

        foreach (var col in cols)
        {
            if (col.GetComponentInParent<IDamageable>() == null) continue;

            Vector3 toEnemy = col.transform.position - transform.position;
            toEnemy.y = 0f; // ignore height differences
            if (toEnemy.sqrMagnitude < 0.001f) continue;

            float angle = Vector3.Angle(transform.forward, toEnemy);
            if (angle > aimAssistAngle) continue;

            // lower score = better: closer and more centered wins
            float score = toEnemy.magnitude + angle * 0.05f;
            if (score < bestScore)
            {
                bestScore = score;
                best = col.transform;
            }
        }
        return best;
    }
    // Runs on button press: face the target so the swing goes the right way
    private void StartStrike()
    {
        Transform target = FindStrikeTarget();
        if (target != null)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // Runs from the Animation Event at the contact frame: deal the damage
    public void StrikeHit()
    {
        HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);

        foreach (var hitCollider in hitColliders)
        {
            IDamageable enemy = hitCollider.GetComponentInParent<IDamageable>();
            if (enemy == null || alreadyHit.Contains(enemy)) continue;

            Vector3 directionToTarget = hitCollider.transform.position - transform.position;
            directionToTarget.y = 0f;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle <= maxAttackAngle)
            {
                alreadyHit.Add(enemy);
                enemy.TakeDamage(attackDamage);
            }
        }

        if (alreadyHit.Count > 0)
            impulseSource.GenerateImpulseWithForce(attackShakeForce);
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

    private void StartGroundPound()
    {
        isGroundPounding = true;
        groundPoundStartTime = Time.time;
        animator.SetBool("isGroundPounding", true);
        playerControl.GroundPoundSlam(slamSpeed); // slam player downwards
    }

    private void GroundPoundImpact()
    {
        isGroundPounding = false;
        groundPoundTimer = 0f; // cooldown starts on landing
        animator.SetBool("isGroundPounding", false); // tells the Animator to go Fall -> Impact

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, groundPoundRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            IDamageable enemy = hitCollider.GetComponentInParent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(groundPoundDamage);
            }
        }

        impulseSource.GenerateImpulseWithForce(attackShakeForce);
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
        hurtSound.PlayOneShot(hurtSound.clip);
        if (impulseSource != null)
            impulseSource.GenerateImpulseWithForce(damageShakeForce);
        Debug.Log("Player took " + damage + " damage. Remaining health: " + playerHealth);
        if (playerHealth <= 0)
        {
            OnPlayerDeath?.Invoke();
        }

        flashTimer = flashDuration;
        currentFlashColor = flashColor;

    }

    public bool Heal(int amount)
    {
        if (playerHealth <= 0 || playerHealth >= maxHealth)
            return false;

        playerHealth = Mathf.Min(playerHealth + amount, maxHealth);
        healSound.PlayOneShot(healSound.clip);
        flashTimer = flashDuration;
        currentFlashColor = healFlashColor;
        return true;
    }

    public void Respawn()
    {
        controller.enabled = false;
        transform.position = CheckpointManager.Instance.CurrentRespawnPosition;
        controller.enabled = true;
        playerHealth = maxHealth;
        respawnSound.PlayOneShot(respawnSound.clip);
        flashTimer = 0f;
        for (int i = 0; i < flashMaterials.Count; i++)
        {
            flashMaterials[i].color = originalColors[i];
        }
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

        if (bodyRenderer != null)
        {
            // sharedMaterials doesn't clone anything, so the outline's materials stay untouched
            Material[] mats = bodyRenderer.sharedMaterials;

            // Make our own copy of just the body material (Element 0) so we don't edit the CelMaterial asset
            Material bodyInstance = new Material(mats[0]);
            mats[0] = bodyInstance;

            // Put the array back. The outline materials go back in as the same references
            bodyRenderer.sharedMaterials = mats;

            if (bodyInstance.HasProperty("_Color") || bodyInstance.HasProperty("_BaseColor"))
            {
                flashMaterials.Add(bodyInstance);
                originalColors.Add(bodyInstance.color);
            }

        }

        if (strikeClip != null)
            strikeAnimLength = strikeClip.length; // if the Strike state's Speed isn't 1, divide by it
    }
    
    void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;

            // 1 right after the hit, fading to 0 as the flash ends
            float t = Mathf.Clamp01(flashTimer / flashDuration);

            for (int i = 0; i < flashMaterials.Count; i++)
            {
                flashMaterials[i].color = Color.Lerp(originalColors[i], currentFlashColor, t);
            }
        }

        strikeTimer += Time.deltaTime;
        spinTimer += Time.deltaTime;
        groundPoundTimer += Time.deltaTime;
        ultimateTimer += Time.deltaTime;

        // can't strike again until the animation has fully played, plus a small gap
        // so the bool has time to go false and the Animator can leave and re-enter Strike
        canStrike = strikeTimer >= Mathf.Max(strikeCooldown, strikeAnimLength + 0.1f);

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

        // Landing check: the ground pound ends when we touch the ground, not on a timer.
        // The small delay stops a stray isGrounded reading on the press frame from ending it instantly.
        if (isGroundPounding && controller.isGrounded && Time.time - groundPoundStartTime > 0.1f)
        {
            GroundPoundImpact();
        }

        if (isUltimate && ultimateTimer >= ultimateAnimLength)
        {
            animator.SetBool("isUltimate", false);
            isUltimate = false;
        }

        //Fire commands for each attack type, with conditions for gliding and attack availability

        // Compute this FIRST so the other checks below can reference it
        //Accounts for slack by allowing different ways to fire both buttons
        if (Input.GetButtonDown("Fire1") && canStrike && controller.isGrounded)
        {
            StartStrike();
            animator.SetBool("isStriking", true);
            strikeTimer = 0f;
            isStriking = true;
        }


        if (Input.GetButtonDown("Fire2") && canSpin && controller.isGrounded)
        {
            Spin();
            windSound.PlayOneShot(windSound.clip);
            animator.SetBool("isSpinning", true);
            spinTimer = 0f;
            isSpinning = true;
        }

        bool inAir = !controller.isGrounded || playerControl.isGliding;

        if (Input.GetButtonDown("Fire1") && canGroundPound && inAir && !isGroundPounding)
        {
            StartGroundPound();
        }

        if (Input.GetButtonDown("Fire3") && canUltimate && playerControl.isGliding == false)
        {
            Ultimate();
            lightningSound.PlayOneShot(lightningSound.clip);
            ultimateTimer = 0f;
            animator.SetBool("isUltimate", true);
            isUltimate = true;
        }
    }
}