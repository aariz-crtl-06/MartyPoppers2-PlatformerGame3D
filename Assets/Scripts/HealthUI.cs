using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    public Image display;

    [Header("Damage Shake")]
    [SerializeField] float shakeDuration = 0.5f;
    [SerializeField] float shakeMagnitude = 8f; // in canvas pixels, tune to taste

    PlayerCombat playerCombat;
    Vector2 restingPosition;
    float shakeTimer = 0f;
    int previousHealth = -1; // -1 means "haven't read health yet"

    void Start()
    {
        playerCombat = Object.FindFirstObjectByType<PlayerCombat>();

        if (display != null)
            restingPosition = display.rectTransform.anchoredPosition;
    }

    void UpdateHealthUI()
    {
        if (playerCombat == null || display == null) return;

        int health = playerCombat.playerHealth;

        // First frame: just record health so we don't shake on load
        if (previousHealth == -1)
            previousHealth = health;

        // Health went down = damage. Going up (like Respawn) doesn't shake
        if (health < previousHealth)
            shakeTimer = shakeDuration;

        previousHealth = health;

        if (health >= 0 && health < sprites.Length)
        {
            display.sprite = sprites[health];
        }
    }

    void UpdateShake()
    {
        if (display == null) return;

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            // Fades out as the timer runs down
            float fade = Mathf.Clamp01(shakeTimer / shakeDuration);
            Vector2 offset = Random.insideUnitCircle * shakeMagnitude * fade;

            display.rectTransform.anchoredPosition = restingPosition + offset;
        }
        else
        {
            // Always snap back to the saved position so it can't get stuck off-center
            display.rectTransform.anchoredPosition = restingPosition;
        }
    }

    void Update()
    {
        UpdateHealthUI();
        UpdateShake();
    }
}