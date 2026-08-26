using UnityEngine;

public class Launchpad : MonoBehaviour
{
    [Header("References")]
    PlayerController playerControl;

    float originalJumpHeight;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerControl = other.GetComponent<PlayerController>();
            originalJumpHeight = playerControl.jumpHeight; // grab the current value right before changing it
            playerControl.jumpHeight = 10f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerControl = other.GetComponent<PlayerController>();
            playerControl.jumpHeight = originalJumpHeight; // Reset jump height when player exits the launchpad
        }
    }
}
