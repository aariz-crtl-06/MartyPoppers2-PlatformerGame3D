using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    PlayerController playerControl;
    CharacterController controller;
    [SerializeField] LayerMask enemyLayer;

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

    [Header("Attack State")]
    bool canStrike = true;
    bool canSpin = true;
    bool canGroundPound = false;
    bool canUltimate = false;

    [Header("Attack Cooldowns")]
    [SerializeField] float strikeCooldown = 0.5f;
    [SerializeField] float spinCooldown = 5f;
    [SerializeField] float groundPoundCooldown = 10f;
    [SerializeField] float ultimateCooldown = 30f;

    [Header("Attack Timers")]
    float strikeTimer = 0.5f;
    float spinTimer = 5f;
    float groundPoundTimer = 10f;
    float ultimateTimer = 0f;

    private void Strike()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            Vector3 directionToTarget = (hitCollider.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle <= maxAttackAngle)
            {
                Debug.Log("Hit " + hitCollider.name + " for strike with" + attackDamage + " damage.");
            }
        }
    }
    private void Spin()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, spinRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            Debug.Log("Hit " + hitCollider.name + " for spin with " + spinDamage + " damage.");
        }

    }

    private void GroundPound()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, groundPoundRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            Debug.Log("Hit " + hitCollider.name + " for ground pound with " + groundPoundDamage + " damage.");
        }
    }

    private void Ultimate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ultimateRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            Debug.Log("Hit " + hitCollider.name + " for ultimate with " + ultimateDamage + " damage.");
        }
    }
    void Start()
    {
        playerControl = GetComponent<PlayerController>();
        controller = GetComponent<CharacterController>();
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

        //Fire commands for each attack type, with conditions for gliding and attack availability

        // Compute this FIRST so the other checks below can reference it
        //Accounts for slack by allowing different ways to fire both buttons
        bool ultimateComboPressed =
            (Input.GetButtonDown("Fire1") && Input.GetButton("Fire2")) ||
            (Input.GetButtonDown("Fire2") && Input.GetButton("Fire1"));

        if (Input.GetButtonDown("Fire1") && canStrike && !ultimateComboPressed && controller.isGrounded)
        {
            Strike();
            strikeTimer = 0f;
        }
        if (Input.GetButtonDown("Fire2") && canSpin && !ultimateComboPressed && controller.isGrounded)
        {
            Spin();
            spinTimer = 0f;
        }
        if (Input.GetButtonDown("Fire1") && canGroundPound && !ultimateComboPressed && !controller.isGrounded)
        {
            GroundPound();
            groundPoundTimer = 0f;
        }
        if (ultimateComboPressed && canUltimate && controller.isGrounded)
        {
            Ultimate();
            ultimateTimer = 0f;
        }
    }
}