using UnityEngine;
using UnityEngine.UI;
public class HealthUI : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    public Image display;

    PlayerCombat playerCombat;
    void Start()
    {
        playerCombat = Object.FindFirstObjectByType<PlayerCombat>();
    }

    private void UpdateHealthUI()
    {
        if (playerCombat != null && display != null)
        {
            int health = playerCombat.playerHealth;

            if (health >= 0 && health < sprites.Length)
            {
                display.sprite = sprites[health];
            }
        }
    }

    void Update()
    {
        UpdateHealthUI();
    }
}
