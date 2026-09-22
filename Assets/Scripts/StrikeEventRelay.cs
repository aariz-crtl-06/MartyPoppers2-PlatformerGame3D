using UnityEngine;

public class StrikeEventRelay : MonoBehaviour
{
    PlayerCombat combat;

    void Awake()
    {
        combat = GetComponentInParent<PlayerCombat>();
    }

    public void StrikeHit()
    {
        if (combat != null) combat.StrikeHit();
    }
}