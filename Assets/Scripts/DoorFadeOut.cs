using System.Collections;
using UnityEngine;

/// <summary>
/// Fades a Shader Graph material's opacity to zero, then disables the GameObject.
/// Call Open() from your key/door unlock logic.
/// </summary>
public class DoorFadeOut : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Tooltip("Reference name from Shader Graph — NOT the display name.")]
    [SerializeField] private string opacityProperty = "_Opacity";

    [Header("Collision")]
    [Tooltip("On: player can walk through as soon as the fade starts. Off: only once it finishes.")]
    [SerializeField] private bool disableColliderImmediately = true;

    private Material mat;
    private bool isFading;

    private void Awake()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null)
            rend = GetComponentInChildren<Renderer>();

        if (rend == null)
        {
            Debug.LogError("DoorFadeOut: no Renderer found.", this);
            enabled = false;
            return;
        }

        // .material (not .sharedMaterial) gives this object its own runtime copy,
        // so fading one door doesn't fade every door — and doesn't edit the asset.
        mat = rend.material;

        if (!mat.HasProperty(opacityProperty))
            Debug.LogError($"DoorFadeOut: material has no property '{opacityProperty}'.", this);
    }

    /// <summary>Begins the fade. Safe to call more than once.</summary>
    public void Open()
    {
        if (isFading || !enabled) return;
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        isFading = true;

        if (disableColliderImmediately)
            SetCollidersEnabled(false);

        float startOpacity = mat.GetFloat(opacityProperty);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            mat.SetFloat(opacityProperty, Mathf.Lerp(startOpacity, 0f, t));
            yield return null;
        }

        // The loop exits on a frame that may have overshot, so snap to exactly 0.
        mat.SetFloat(opacityProperty, 0f);

        SetCollidersEnabled(false);
        gameObject.SetActive(false);
    }

    private void SetCollidersEnabled(bool value)
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = value;
    }
}