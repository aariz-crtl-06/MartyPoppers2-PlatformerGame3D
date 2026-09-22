using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour, IDamageable
{
    enum BossState
    {
        Idle,
        FistTracking, FistWindup, FistSlam, FistEmbedded, FistRetracting,   // attack 1
        AirDeploying,                                                        // attack 2
        SpawningMinions, WaitingForMinions,                                  // attack 3
        GiantSwap,                                                           // scale transition
        BossTracking, BossWindup, BossSlam, BossLanding, BossStunned         // attack 4
    }

    [Header("References")]
    [SerializeField] Animator animator;          // on the Model child
    [SerializeField] Transform player;
    [SerializeField] GameObject giantFist;
    [SerializeField] GameObject marker;
    [SerializeField] GameObject giantModel;      // the big one watching during attacks 1-3
    [SerializeField] GameObject bossModel;       // the smaller one that lands in attack 4
    [SerializeField] GameObject giantDustPrefab;
    [SerializeField] GameObject bossDustPrefab;
    [SerializeField] float giantSwapDuration = 0.5f;
    [SerializeField] float bossSwapDuration = 0.35f;
    [SerializeField] float dustLifetime = 3f;
    [SerializeField] LayerMask groundLayer;
    public AudioSource fistSound;
    public AudioSource bossLand;

    [Header("Damage")]
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float verticalTolerance = 4f;   // ignores a player gliding overhead
    [SerializeField] float fistImpactRadius = 3f;
    [SerializeField] int fistDamage = 1;
    [SerializeField] float bossImpactRadius = 4f;
    [SerializeField] int bossSlamDamage = 2;

    [Header("Attack Flow")]
    [SerializeField] float timeBetweenAttacks = 2f;

    [Header("Boss Health")]
    [SerializeField] int health = 15;
    int maxHealth;
    public int Health => health;
    public int MaxHealth => maxHealth;

    [Header("Attack 1 - Tracking Fist")]
    [SerializeField] float trackDuration = 5f;
    [SerializeField] float windupDuration = 1.2f;
    [SerializeField] float embedDuration = 5f;
    [SerializeField] float hoverHeight = 8f;
    [SerializeField] float slamSpeed = 40f;
    [SerializeField] float retractSpeed = 10f;

    [Header("Attack 1 - Marker")]
    [SerializeField] float markerStartScale = 0.3f;
    [SerializeField] float markerEndScale = 1.2f;
    [SerializeField] Color trackingColor = new Color(1f, 0.92f, 0.2f, 0.45f);
    [SerializeField] Color lockedColor = new Color(1f, 0.15f, 0.15f, 0.6f);

    [Header("Attack 2 - Air Deploy")]
    [SerializeField] GameObject fallingFistPrefab;
    [SerializeField] Transform arenaCenter;
    [SerializeField] float arenaRadius = 20f;
    [SerializeField] int waveCount = 4;
    [SerializeField] int fistsPerWave = 5;
    [SerializeField] float waveInterval = 3f;
    [SerializeField] float earlyWarningTime = 5f;
    [SerializeField] float lateWarningTime = 2f;
    [SerializeField] float fistSpawnHeight = 25f;
    [SerializeField] float minFallSpeed = 12f;
    [SerializeField] float maxFallSpeed = 35f;
    [SerializeField] float playerSafeRadius = 4f;
    [SerializeField] float minSeparation = 4f;

    [Header("Attack 3 - Minions")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] int minionCount = 3;
    [SerializeField] float minionSpawnRadius = 12f;
    [SerializeField] float minionPhaseTimeout = 60f;   // safety valve -- never soft-lock

    [Header("Attack 4 - Boss Ground Pound")]
    [SerializeField] float bossHoverHeight = 18f;
    [SerializeField] float bossTrackDuration = 5f;    // follows the player, same as attack 1
    [SerializeField] float bossWindupDuration = 1.5f;
    [SerializeField] float bossSlamSpeed = 35f;
    [SerializeField] float landAnimDuration = 0.7f;   // length of the JumpLand clip
    [SerializeField] float minStunDuration = 5f;
    [SerializeField] float maxStunDuration = 10f;

    [Header("Debug (read-only)")]
    [SerializeField] BossState state = BossState.Idle;
    [SerializeField] int attackIndex = 0;
    [SerializeField] int minionsDefeated = 0;

    float idleCountdown;
    float phaseTimer;
    Vector3 slamTarget;
    Renderer markerRenderer;
    List<GameObject> activeMinions = new List<GameObject>();
    Vector3 homePosition;
    Vector3 bossLandTarget;
    float stunDuration;
    Vector3 giantFullScale = Vector3.one;
    Vector3 bossFullScale = Vector3.one;

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
    public AudioSource hitSound;

    // ---------------- attack flow ----------------

    private void Idle()
    {
        idleCountdown += Time.deltaTime;
        if (idleCountdown < timeBetweenAttacks) return;

        idleCountdown = 0f;

        switch (attackIndex)
        {
            case 0: StartFistAttack(); break;
            case 1: StartAirDeploy(); break;
            case 2: StartMinionPhase(); break;
            case 3: StartVulnerable(); break;
        }

        attackIndex = (attackIndex + 1) % 4;
    }

    private void EndAttack()
    {
        idleCountdown = 0f;
        state = BossState.Idle;
    }

    // ---------------- attack 1: tracking fist ----------------

    private void StartFistAttack()
    {
        phaseTimer = 0f;

        if (giantFist != null) giantFist.SetActive(true);
        if (marker != null)
        {
            marker.SetActive(true);
            SetMarkerColor(trackingColor);
        }

        state = BossState.FistTracking;
    }

    private void FistTracking()
    {
        phaseTimer += Time.deltaTime;

        Vector3 playerPos = player.position;
        giantFist.transform.position = new Vector3(playerPos.x, playerPos.y + hoverHeight, playerPos.z);

        float t = Mathf.Clamp01(phaseTimer / trackDuration);
        PlaceMarker(GroundPointBelow(playerPos), Mathf.Lerp(markerStartScale, markerEndScale, t));

        if (phaseTimer >= trackDuration)
        {
            // commit: target frozen from here, which is what makes it dodgeable
            slamTarget = GroundPointBelow(playerPos);
            PlaceMarker(slamTarget, markerEndScale);
            SetMarkerColor(lockedColor);

            phaseTimer = 0f;
            state = BossState.FistWindup;
        }
    }

    private void FistWindup()
    {
        phaseTimer += Time.deltaTime;
        giantFist.transform.position = slamTarget + Vector3.up * hoverHeight;

        if (phaseTimer >= windupDuration)
            state = BossState.FistSlam;
    }

    private void FistSlam()
    {
        giantFist.transform.position = Vector3.MoveTowards(
            giantFist.transform.position, slamTarget, slamSpeed * Time.deltaTime);

        

        if (giantFist.transform.position == slamTarget)
        {
            fistSound.PlayOneShot(fistSound.clip);
            DamageAtPoint(slamTarget, fistImpactRadius, fistDamage);
            phaseTimer = 0f;
            state = BossState.FistEmbedded;
        }
    }

    private void FistEmbedded()
    {
        phaseTimer += Time.deltaTime;
        giantFist.transform.position = slamTarget;

        if (phaseTimer >= embedDuration)
        {
            if (marker != null) marker.SetActive(false);
            state = BossState.FistRetracting;
        }
    }

    private void FistRetracting()
    {
        Vector3 restPos = slamTarget + Vector3.up * hoverHeight;

        giantFist.transform.position = Vector3.MoveTowards(
            giantFist.transform.position, restPos, retractSpeed * Time.deltaTime);

        if (giantFist.transform.position == restPos)
        {
            if (giantFist != null) giantFist.SetActive(false);
            EndAttack();
        }
    }

    // ---------------- attack 2: air deploy ----------------

    private void StartAirDeploy()
    {
        state = BossState.AirDeploying;
        StartCoroutine(AirDeployRoutine());
    }

    private IEnumerator AirDeployRoutine()
    {
        for (int wave = 0; wave < waveCount; wave++)
        {
            float warningTime = (wave < 2) ? earlyWarningTime : lateWarningTime;
            List<Vector3> points = PickWavePoints(fistsPerWave);

            foreach (Vector3 point in points)
            {
                GameObject obj = Instantiate(fallingFistPrefab);
                FallingFist fist = obj.GetComponent<FallingFist>();
                float speed = Random.Range(minFallSpeed, maxFallSpeed);
                fist.Launch(point, speed, warningTime, fistSpawnHeight);
            }

            yield return new WaitForSeconds(waveInterval);
        }

        yield return new WaitForSeconds(lateWarningTime + 1f);
        EndAttack();
    }

    private List<Vector3> PickWavePoints(int count)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 center = (arenaCenter != null) ? arenaCenter.position : transform.position;
        Vector3 playerFlat = new Vector3(player.position.x, 0f, player.position.z);

        for (int i = 0; i < count; i++)
        {
            Vector3 chosen = Vector3.zero;
            bool found = false;

            for (int attempt = 0; attempt < 30; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * arenaRadius;
                Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);
                Vector3 candidateFlat = new Vector3(candidate.x, 0f, candidate.z);

                if (Vector3.Distance(candidateFlat, playerFlat) < playerSafeRadius) continue;

                bool tooClose = false;
                foreach (Vector3 existing in points)
                {
                    Vector3 existingFlat = new Vector3(existing.x, 0f, existing.z);
                    if (Vector3.Distance(candidateFlat, existingFlat) < minSeparation)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                chosen = candidate;
                found = true;
                break;
            }

            if (!found)
            {
                Vector2 offset = Random.insideUnitCircle * arenaRadius;
                chosen = center + new Vector3(offset.x, 0f, offset.y);
            }

            points.Add(GroundPointBelow(chosen));
        }

        return points;
    }

    // ---------------- attack 3: minions ----------------

    private void StartMinionPhase()
    {
        minionsDefeated = 0;
        phaseTimer = 0f;
        activeMinions.Clear();

        Vector3 center = (arenaCenter != null) ? arenaCenter.position : transform.position;

        for (int i = 0; i < minionCount; i++)
        {
            // space them evenly around the arena rather than clumping
            float angle = (360f / minionCount) * i + Random.Range(-20f, 20f);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 candidate = center + dir * minionSpawnRadius;

            // NavMeshAgents must start on the baked mesh
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                candidate = hit.position;

            GameObject obj = Instantiate(enemyPrefab, candidate, Quaternion.identity);
            EnemyAI enemy = obj.GetComponent<EnemyAI>();
            if (enemy != null) enemy.SetupAsMinion(player, this);

            activeMinions.Add(obj);
        }

        state = BossState.WaitingForMinions;
    }

    // called by EnemyAI when a spawned minion dies
    public void OnMinionDefeated()
    {
        minionsDefeated++;
    }

    private void WaitingForMinions()
    {
        phaseTimer += Time.deltaTime;

        if (minionsDefeated >= minionCount)
        {
            EndAttack();   // attackIndex already points at attack 4
            return;
        }

        // safety valve: a stuck or fallen minion must never soft-lock the fight
        if (phaseTimer >= minionPhaseTimeout)
        {
            CleanupMinions();
            EndAttack();
        }
    }

    private void CleanupMinions()
    {
        foreach (GameObject minion in activeMinions)
            if (minion != null) Destroy(minion);

        activeMinions.Clear();
    }

    // ---------------- attack 4: boss ground pound + stun ----------------

    private void StartVulnerable()
    {
        state = BossState.GiantSwap;
        StartCoroutine(GiantExitRoutine());
    }

    // giant shrinks away, then the smaller boss grows in above the player
    private IEnumerator GiantExitRoutine()
    {
        if (giantModel != null)
        {
            SpawnDust(giantDustPrefab, giantModel.transform.position);
            yield return ScaleOver(giantModel, giantFullScale, Vector3.zero, giantSwapDuration);
            giantModel.SetActive(false);
        }

        // move into position before appearing, so it grows in where it belongs
        Vector3 playerPos = player.position;
        transform.position = new Vector3(playerPos.x, playerPos.y + bossHoverHeight, playerPos.z);

        if (bossModel != null)
        {
            bossModel.transform.localScale = Vector3.zero;
            bossModel.SetActive(true);
            SpawnDust(bossDustPrefab, bossModel.transform.position);
            yield return ScaleOver(bossModel, Vector3.zero, bossFullScale, bossSwapDuration);
        }

        phaseTimer = 0f;
        if (marker != null)
        {
            marker.SetActive(true);
            SetMarkerColor(trackingColor);
        }

        PlayAnim("Idle");            // airborne for the whole track + windup
        state = BossState.BossTracking;
    }

    // boss shrinks away, then the giant grows back
    private IEnumerator GiantEnterRoutine()
    {
        if (bossModel != null)
        {
            SpawnDust(bossDustPrefab, bossModel.transform.position);
            yield return ScaleOver(bossModel, bossFullScale, Vector3.zero, bossSwapDuration);
            bossModel.SetActive(false);
        }

        transform.position = homePosition;

        if (giantModel != null)
        {
            giantModel.transform.localScale = Vector3.zero;
            giantModel.SetActive(true);
            SpawnDust(giantDustPrefab, giantModel.transform.position);
            yield return ScaleOver(giantModel, Vector3.zero, giantFullScale, giantSwapDuration);
        }

        EndAttack();
    }

    private IEnumerator ScaleOver(GameObject obj, Vector3 from, Vector3 to, float duration)
    {
        if (obj == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            obj.transform.localScale = Vector3.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }

        obj.transform.localScale = to;
    }

    private void SpawnDust(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;

        GameObject dust = Instantiate(prefab, position, Quaternion.identity);
        Destroy(dust, dustLifetime);
    }

    private void BossTracking()
    {
        phaseTimer += Time.deltaTime;

        // boss hovers overhead and follows, exactly like the fist in attack 1
        Vector3 playerPos = player.position;
        transform.position = new Vector3(playerPos.x, playerPos.y + bossHoverHeight, playerPos.z);

        float t = Mathf.Clamp01(phaseTimer / bossTrackDuration);
        PlaceMarker(GroundPointBelow(playerPos), Mathf.Lerp(markerStartScale, markerEndScale, t));

        if (phaseTimer >= bossTrackDuration)
        {
            // commit: landing spot frozen from here
            bossLandTarget = GroundPointBelow(playerPos);
            PlaceMarker(bossLandTarget, markerEndScale);
            SetMarkerColor(lockedColor);

            phaseTimer = 0f;
            state = BossState.BossWindup;
        }
    }

    private void BossWindup()
    {
        phaseTimer += Time.deltaTime;

        // hold still above the locked spot -- this is the dodge window
        transform.position = bossLandTarget + Vector3.up * bossHoverHeight;

        if (phaseTimer >= bossWindupDuration)
            state = BossState.BossSlam;
    }

    private void BossSlam()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, bossLandTarget, bossSlamSpeed * Time.deltaTime);

        if (transform.position == bossLandTarget)
        {
            DamageAtPoint(bossLandTarget, bossImpactRadius, bossSlamDamage);
            if (marker != null) marker.SetActive(false);

            PlayAnim("JumpLand");
            bossLand.PlayOneShot(bossLand.clip);
            phaseTimer = 0f;
            state = BossState.BossLanding;
        }
    }

    private void BossLanding()
    {
        // let the landing clip finish before the stun begins
        phaseTimer += Time.deltaTime;

        if (phaseTimer >= landAnimDuration)
        {
            PlayAnim("Hurt");
            stunDuration = Random.Range(minStunDuration, maxStunDuration);
            phaseTimer = 0f;
            state = BossState.BossStunned;
        }
    }

    private void BossStunned()
    {
        // the only window where TakeDamage does anything
        phaseTimer += Time.deltaTime;

        if (phaseTimer >= stunDuration)
        {
            PlayAnim("Idle");

            state = BossState.GiantSwap;
            StartCoroutine(GiantEnterRoutine());
        }
    }

    // matches EnemyAI's signature so your existing PlayerCombat hits this too
    public void TakeDamage(int damageTaken)
    {
        if (state != BossState.BossStunned) return;   // armoured every other phase

        flashTimer = flashDuration;
        hitSound.PlayOneShot(hitSound.clip);
        health -= damageTaken;

        if (health <= 0)
        {
            CleanupMinions();
            SpawnDust(bossDustPrefab, bossModel.transform.position);
            Destroy(gameObject);
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
                p.renderer.SetPropertyBlock(null, p.materialIndex);
            }
        }
    }

    // ---------------- shared helpers ----------------

    // One overlap check on the impact frame. More predictable than letting a
    // fast collider try to catch a CharacterController.
    private void DamageAtPoint(Vector3 groundPoint, float radius, int damage)
    {
        float searchRadius = Mathf.Max(radius, verticalTolerance);
        Collider[] hits = Physics.OverlapSphere(groundPoint, searchRadius, playerLayer);

        foreach (Collider hit in hits)
        {
            Vector3 delta = hit.transform.position - groundPoint;

            float horizontal = new Vector2(delta.x, delta.z).magnitude;
            if (horizontal > radius) continue;                       // dodged sideways
            if (Mathf.Abs(delta.y) > verticalTolerance) continue;    // above or below

            PlayerCombat target = hit.GetComponentInParent<PlayerCombat>();
            if (target != null) target.TakeDamage(damage);
        }
    }

    private void PlayAnim(string stateName)
    {
        if (animator == null) return;

        // CrossFade plays a state directly, so no transitions need wiring in the
        // Animator window. 0.1 is the blend time between clips.
        animator.CrossFade(stateName, 0.1f, 0, 0f);
    }

    private void PlaceMarker(Vector3 groundPoint, float scale)
    {
        if (marker == null) return;

        marker.transform.position = groundPoint + Vector3.up * 0.05f;
        marker.transform.localScale = new Vector3(scale, scale, scale);
    }

    private void SetMarkerColor(Color color)
    {
        if (markerRenderer == null) return;
        markerRenderer.material.color = color;
    }

    private Vector3 GroundPointBelow(Vector3 worldPos)
    {
        Vector3 origin = worldPos + Vector3.up * 50f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f, groundLayer))
            return hit.point;

        return new Vector3(worldPos.x, transform.position.y, worldPos.z);
    }

    void Start()
    {
        maxHealth = health;   // remember the starting value so the bar knows what "full" is

        if (marker != null)
        {
            markerRenderer = marker.GetComponent<Renderer>();
            marker.SetActive(false);
        }
        if (giantFist != null) giantFist.SetActive(false);

        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (giantModel != null)
        {
            giantFullScale = giantModel.transform.localScale;
            giantModel.SetActive(true);
        }

        block = new MaterialPropertyBlock();

        if (bossModel != null)
        {
            // true = include inactive, because bossModel starts hidden
            foreach (Renderer r in bossModel.GetComponentsInChildren<Renderer>(true))
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

        if (bossModel != null)
        {
            bossFullScale = bossModel.transform.localScale;
            bossModel.SetActive(false);   // hidden during attacks 1-3
        }

        homePosition = transform.position;   // where the boss sits during attacks 1-3
    }

    void Update()
    {
        UpdateFlash();
        switch (state)
        {
            case BossState.Idle: Idle(); break;
            case BossState.FistTracking: FistTracking(); break;
            case BossState.FistWindup: FistWindup(); break;
            case BossState.FistSlam: FistSlam(); break;
            case BossState.FistEmbedded: FistEmbedded(); break;
            case BossState.FistRetracting: FistRetracting(); break;
            case BossState.AirDeploying:      /* coroutine drives */ break;
            case BossState.GiantSwap:         /* coroutine drives */ break;
            case BossState.WaitingForMinions: WaitingForMinions(); break;
            case BossState.BossTracking: BossTracking(); break;
            case BossState.BossWindup: BossWindup(); break;
            case BossState.BossSlam: BossSlam(); break;
            case BossState.BossLanding: BossLanding(); break;
            case BossState.BossStunned: BossStunned(); break;
        }
    }
}