using UnityEngine;

// Goes on the falling-fist prefab. Owns one fist's entire life:
// hover -> warn -> fall -> linger -> destroy. The boss spawns it and forgets it.
public class FallingFist : MonoBehaviour
{
    enum Phase { Warning, Falling, Landed }

    [Header("References")]
    [SerializeField] GameObject marker;          // flat quad child, transparent material
    [SerializeField] Renderer markerRenderer;

    [Header("Marker")]
    [SerializeField] float markerStartScale = 0.3f;
    [SerializeField] float markerEndScale = 1.2f;
    [SerializeField] Color warningColor = new Color(1f, 0.92f, 0.2f, 0.45f);
    [SerializeField] Color imminentColor = new Color(1f, 0.15f, 0.15f, 0.6f);

    [Header("Timing")]
    [SerializeField] float lingerDuration = 1f;  // how long it sits after landing

    Phase phase = Phase.Warning;
    Vector3 groundTarget;
    float fallSpeed;
    float warningDuration;
    float timer;

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
                        // hook for damage detection later
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
                        if (marker != null) Destroy(marker);
                        Destroy(gameObject);
                    }
                    break;
                }
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