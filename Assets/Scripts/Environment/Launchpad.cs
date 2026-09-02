using UnityEngine;

public class Launchpad : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float boostedJumpHeight = 10f;
    [SerializeField] string jumpButton = "Jump";
    [SerializeField] string launchTriggerName = "Launch";

    [Header("References")]
    [SerializeField] Animator padAnimator;   // drag BouncePad here in the Inspector

    PlayerController playerControl;
    CharacterController charController;

    float originalJumpHeight;
    bool playerOnPad;

    void Awake()
    {
        // self-wire if the Inspector slot was left empty
        if (padAnimator == null) padAnimator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!playerOnPad || playerControl == null) return;

        // Input has to be polled every frame, not inside OnTriggerEnter
        if (Input.GetButtonDown(jumpButton))
        {
            // only play the launch anim if they're actually leaving the ground
            if (charController == null || charController.isGrounded)
            {
                if (padAnimator != null) padAnimator.SetTrigger(launchTriggerName);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerOnPad) return; // already boosted — don't overwrite the saved value

        playerControl = other.GetComponentInParent<PlayerController>();
        if (playerControl == null) return;

        charController = playerControl.GetComponent<CharacterController>();

        originalJumpHeight = playerControl.jumpHeight;
        playerControl.jumpHeight = boostedJumpHeight;
        playerOnPad = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!playerOnPad) return;

        RestoreJumpHeight();
    }

    void OnDisable()
    {
        // pad gets destroyed / disabled while the player is standing on it
        if (playerOnPad) RestoreJumpHeight();
    }

    void RestoreJumpHeight()
    {
        if (playerControl != null) playerControl.jumpHeight = originalJumpHeight;
        playerOnPad = false;
    }
}