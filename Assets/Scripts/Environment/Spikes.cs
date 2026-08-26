using UnityEngine;

public class Spikes : MonoBehaviour
{
    [Header("References")]
    PlayerCombat playerCombat;
    PlayerController playerControl;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit the spikes!");
            playerCombat = other.GetComponent<PlayerCombat>();
            playerControl = other.GetComponent<PlayerController>();
            playerCombat.TakeDamage(1);
            playerControl.Bounce(10f); // Makes player bounce back when they hit the spikes
        }
    }
}
