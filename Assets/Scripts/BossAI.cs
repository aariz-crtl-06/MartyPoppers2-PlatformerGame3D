using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    enum BossState { Idle, FistTracking, FistWindup, FistSlam, FistEmbedded, AirDeploying }

    [Header("References")]
    Animator animator;
    [SerializeField] Transform player;
    [SerializeField] GameObject giantFist;       // attack 1 only -- scene object
    [SerializeField] GameObject marker;          // attack 1's ground marker
    [SerializeField] LayerMask groundLayer;

    [Header("Attack Flow")]
    [SerializeField] float timeBetweenAttacks = 2f;
    [SerializeField] bool alternateAttacks = true;   // off = pick at random

    [Header("Attack 1 - Tracking Fist")]
    [SerializeField] float trackDuration = 5f;    // how long the fist follows before locking
    [SerializeField] float windupDuration = 1.2f; // locked, before it drops -- dodge window
    [SerializeField] float embedDuration = 5f;    // stays down -- punish window
    [SerializeField] float hoverHeight = 8f;
    [SerializeField] float slamSpeed = 40f;

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
    [SerializeField] float earlyWarningTime = 5f;   // waves 1-2
    [SerializeField] float lateWarningTime = 2f;    // waves 3-4
    [SerializeField] float fistSpawnHeight = 25f;
    [SerializeField] float minFallSpeed = 12f;
    [SerializeField] float maxFallSpeed = 35f;
    [SerializeField] float playerSafeRadius = 4f;
    [SerializeField] float minSeparation = 4f;

    [Header("Debug (read-only)")]
    [SerializeField] BossState state = BossState.Idle;

    float idleCountdown;
    float phaseTimer;
    Vector3 slamTarget;
    bool nextIsAirDeploy = false;
    Renderer markerRenderer;

    // ---------------- attack selection ----------------

    private void Idle()
    {
        idleCountdown += Time.deltaTime;
        if (idleCountdown < timeBetweenAttacks) return;

        idleCountdown = 0f;

        bool useAirDeploy = alternateAttacks ? nextIsAirDeploy : (Random.value < 0.5f);
        nextIsAirDeploy = !nextIsAirDeploy;

        if (useAirDeploy) StartAirDeploy();
        else StartFistAttack();
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

        // marker trails the player, growing as the lock approaches
        float t = Mathf.Clamp01(phaseTimer / trackDuration);
        PlaceMarker(GroundPointBelow(playerPos), Mathf.Lerp(markerStartScale, markerEndScale, t));

        if (phaseTimer >= trackDuration)
        {
            // commit: target is now frozen, this is what makes it dodgeable
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
            // hook for damage detection later
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
            if (giantFist != null) giantFist.SetActive(false);
            if (marker != null) marker.SetActive(false);
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

        // let the last wave land before the next attack begins
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

            // capped retries -- never loop forever on an unlucky seed
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
                // relax the constraints rather than spawning fewer fists
                Vector2 offset = Random.insideUnitCircle * arenaRadius;
                chosen = center + new Vector3(offset.x, 0f, offset.y);
            }

            points.Add(GroundPointBelow(chosen));
        }

        return points;
    }

    // ---------------- shared helpers ----------------

    private void PlaceMarker(Vector3 groundPoint, float scale)
    {
        if (marker == null) return;

        // small lift to avoid z-fighting with the ground mesh
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

    private void GroundSlam()
    {
    }

    private void Stunned()
    {
    }

    void Start()
    {
        if (marker != null)
        {
            markerRenderer = marker.GetComponent<Renderer>();
            marker.SetActive(false);
        }
        if (giantFist != null) giantFist.SetActive(false);
    }

    void Update()
    {
        switch (state)
        {
            case BossState.Idle: Idle(); break;
            case BossState.FistTracking: FistTracking(); break;
            case BossState.FistWindup: FistWindup(); break;
            case BossState.FistSlam: FistSlam(); break;
            case BossState.FistEmbedded: FistEmbedded(); break;
            case BossState.AirDeploying: /* coroutine drives this */ break;
        }
    }
}