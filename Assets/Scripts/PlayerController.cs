using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    private float moveInput;
    private float turnInput;

    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float turnSpeed = 1f;
    [SerializeField] float gravity = 9.81f;
    public float jumpHeight = 2f;
    public bool isGliding;
    private bool isGroundPounding = false;
    private bool isBouncing = false; // for spike obstcle 
    private float verticalVelocity = 0f;


    [Header("References")]
    CharacterController controller;
    [SerializeField] Transform playerCamera;
    private MovingPlatform currentPlatform;
    Animator animator;

    Vector3 moveDirection;

    private void InputManagement()
    {
        //Apply WASD
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        MovingPlatform platform = hit.collider.GetComponent<MovingPlatform>();
        if (platform != null)
        {
            currentPlatform = platform;
        }
    }

    private void GroundMovement()
    {
        Vector3 move = new Vector3(turnInput, 0, moveInput);

        move = playerCamera.transform.TransformDirection(move); // Makes sure player is moving in facing direction

        // Apply speed
        move *= walkSpeed;

        

        move.y = VerticalForceCalculation(); // Apply vertical force (gravity)

        Vector3 platformDelta = Vector3.zero;
        if (currentPlatform != null)
        {
            platformDelta = currentPlatform.DeltaMovement;
        }
        currentPlatform = null; // OnControllerColliderHit will re-set this during move

        // Move the player
        controller.Move((move * Time.deltaTime) + platformDelta); //to ensure player can land on platform
    }

    private void Turn()
    {
        // Rotate the player to face the direction of movement, only if there is movement input
        if (Mathf.Abs(turnInput) > 0 || Mathf.Abs(moveInput) > 0)
        {
            animator.SetBool("isRunning", true);
            Vector3 currentLookAtDirection = controller.velocity.normalized;
            currentLookAtDirection.y = 0; // Keep rotation strictly horizontal

            currentLookAtDirection.Normalize(); // Ensure the direction is normalized

            Quaternion lookRotation = Quaternion.LookRotation(currentLookAtDirection);

            //Apply turn to character
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        }

        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    private float VerticalForceCalculation()
    {
        if (controller.isGrounded)
        {
            if (!isBouncing)
            {
                verticalVelocity = -1f; // Apply a small downward force to keep the player grounded
            }
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            }
        }
        else
        {
            isBouncing = false; // once actually left the ground, clear the flag
            verticalVelocity -= gravity * Time.deltaTime;
            if (isGliding)
            {
                verticalVelocity = Mathf.Max(verticalVelocity, -1f);
            }
        }
        return verticalVelocity;
    }

    private void UmbrellaGlide()
    {
        if (!controller.isGrounded)
        {
            if (!isGroundPounding && Input.GetButton("Fire2"))
            {
                isGliding = true;
            }
            else
            {
                isGliding = false;
            }
        }
        else
        {
            isGliding = false;
            isGroundPounding = false; // reset once we've landed
        }
    }

    public void GroundPoundSlam(float slamSpeed)
    {
        isGroundPounding = true;
        isGliding = false;
        verticalVelocity = -slamSpeed;
    }

    public void Bounce(float bounceForce)
    {
        isBouncing = true;
        verticalVelocity = bounceForce;
    }

    private void Movement()
    {
        // Function to handle all movement related functions
        GroundMovement();
        Turn();
    }

    void Start()
    {
      controller = GetComponent<CharacterController>();
      animator = GetComponentInChildren<Animator>();

      // Lock cursor to center of screen
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    }

    void Update()
    {
        InputManagement();
        UmbrellaGlide();
        Movement();

        
        
    }
}
