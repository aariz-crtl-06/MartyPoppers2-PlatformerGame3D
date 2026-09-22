using UnityEngine;

public class HealPickup : MonoBehaviour
{
    [SerializeField] int healAmount = 1;
    void OnTriggerStay(Collider other)
    {
        PlayerCombat player = other.GetComponentInParent<PlayerCombat>();
        if (player == null) return;


        if (player.Heal(healAmount))
            Destroy(gameObject);
    }
}