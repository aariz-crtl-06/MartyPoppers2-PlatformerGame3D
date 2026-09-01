using UnityEngine;

// Goes on the falling-fist prefab. Owns one fist's entire life:
// hover -> warn -> fall -> linger -> destroy. The boss spawns it and forgets it.
public class FallingFist : MonoBehaviour
{
    enum Phase { Warning, Falling, Landed, Shrinking }

    [Header("References")]
    [SerializeField] GameObject marker;          // flat quad child, transparent material
    [SerializeField] Renderer markerRenderer;

    [Header("Marker")]
    [SerializeField] float markerStartScale = 0.3f;
    [SerializeField] float markerEndScale = 1.2f;
    [SerializeField] Color warningColor = new Color(1f, 0.92f, 0.2f, 0.45f);
    [SerializeField] Color imminentColor = new Color(1f, 0.15f, 0.15f, 0.6f);

    [Header("Damage")]
    [SerializeField] LayerMask playerLayer;
    [SerializeField] float impactRadius = 2.5f;
    [SerializeField] float verticalTolerance = 4f;
    [SerializeField] int damage = 1;

    [Header("Timing")]
    [SerializeField] float lingerDuration = 1.5f;   // how long it sits after landing
    [SerializeField] float shrinkDuration = 0.6f;   // then it shrinks away instead of popping

    Phase phase = Phase.Warning;
    Vector3 groundTarget;
    float fallSpeed;
    float warningDuration;
    float timer;
    Vector3 landedScale;

    // Called by the boss right after Instantiate.
    public void Launch(Vector3 target, float speed, float warningTime, float spawnHeight)
    {
        groundTarget = target;
        fallSpeed = speed;
        warningDuration = warningTime;
        timer = 0f;
        phase = Phase.Warning;

        transform.position = groundTarget + Vector3.up * spawnHeight;

        if (marker != null)
        {
            // detach so the marker stays on the ground while the fist falls
            marker.transform.SetParent(null, true);
            marker.transform.position = groundTarget + Vector3.up * 0.05f;
            marker.SetActive(true);

            if (markerRenderer == null) markerRenderer = marker.GetComponent<Renderer>();
            SetMarkerColor(warningColor);
            SetMarkerScale(markerStartScale);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Warning:
                {
                    // marker grows across THIS fist's own warning window
                    float t = Mathf.Clamp01(timer / warningDuration);
                    SetMarkerScale(Mathf.Lerp(markerStartScale, markerEndScale, t));
                    if (t > 0.7f) SetMarkerColor(imminentColor);

                    if (timer >= warningDuration)
                    {
                        timer = 0f;
                        phase = Phase.Falling;
                    }
                    break;
                }

            case Phase.Falling:
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position, groundTarget, fallSpeed * Time.deltaTime);

                    if (transform.position == groundTarget)
                    {
                        DamageAtPoint(groundTarget);
                        timer = 0f;
                        phase = Phase.Landed;
                        if (marker != null) marker.SetActive(false);
                    }
                    break;
                }

            case Phase.Landed:
                {
                    if (timer >= lingerDuration)
                    {
                        landedScale = transform.localScale;
                        timer = 0f;
                        phase = Phase.Shrinking;
                    }
                    break;
                }

            case Phase.Shrinking:
                {
                    float t = Mathf.Clamp01(timer / shrinkDuration);
                    transform.localScale = Vector3.Lerp(landedScale, Vector3.zero, t);

                    if (t >= 1f)
                    {
                        if (marker != null) Destroy(marker);
                        Destroy(gameObject);
                    }
                    break;
                }
        }
    }

    void DamageAtPoint(Vector3 groundPoint)
    {
        float searchRadius = Mathf.Max(impactRadius, verticalTolerance);
        Collider[] hits = Physics.OverlapSphere(groundPoint, searchRadius, playerLayer);

        foreach (Collider hit in hits)
        {
            Vector3 delta = hit.transform.position - groundPoint;

            float horizontal = new Vector2(delta.x, delta.z).magnitude;
            if (horizontal > impactRadius) continue;
            if (Mathf.Abs(delta.y) > verticalTolerance) continue;

            PlayerCombat target = hit.GetComponentInParent<PlayerCombat>();
            if (target != null) target.TakeDamage(damage);
        }
    }

    void SetMarkerScale(float scale)
    {
        if (marker == null) return;
        marker.transform.localScale = new Vector3(scale, scale, scale);
    }

    void SetMarkerColor(Color color)
    {
        if (markerRenderer == null) return;
        markerRenderer.material.color = color;
    }

    void OnDestroy()
    {
        // safety net if the fist is destroyed early (scene unload, boss death)
        if (marker != null) Destroy(marker);
    }
}