using UnityEngine;
using UnityEngine.UI;

public class AttackMoveUI : MonoBehaviour
{
    [Header("Attack1")]
    [SerializeField] Sprite[] spritesA1;
    public Image displayA1;

    [Header("Attack2")]
    [SerializeField] Sprite[] spritesA2;
    public Image displayA2;

    [Header("Attack3")]
    [SerializeField] Sprite[] spritesA3;
    public Image displayA3;

    PlayerCombat playerCombat;

    void Start()
    {
        playerCombat = Object.FindFirstObjectByType<PlayerCombat>();
    }

    void Update()
    {
        if (playerCombat == null) return;

        UpdateCooldownIcon(displayA1, spritesA1, playerCombat.spinTimer, playerCombat.spinCooldown);
        UpdateCooldownIcon(displayA2, spritesA2, playerCombat.groundPoundTimer, playerCombat.groundPoundCooldown);
        UpdateCooldownIcon(displayA3, spritesA3, playerCombat.ultimateTimer, playerCombat.ultimateCooldown);
    }

    // Sprites should be ordered from "just used" (index 0) to "ready" (last index)
    void UpdateCooldownIcon(Image display, Sprite[] sprites, float timer, float cooldown)
    {
        if (display == null || sprites == null || sprites.Length == 0) return;

        // 0 = just used, 1 = ready. Clamped because the timers keep counting past the cooldown
        float progress = cooldown > 0f ? Mathf.Clamp01(timer / cooldown) : 1f;

        int index = Mathf.Min((int)(progress * sprites.Length), sprites.Length - 1);
        display.sprite = sprites[index];

        
        display.color = progress >= 1f ? Color.white : Color.gray;
    }
}
