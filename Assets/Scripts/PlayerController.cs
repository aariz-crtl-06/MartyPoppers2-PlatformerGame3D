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
    [SerializeField] float jumpHeight = 2f;
    private bool isGliding;

    private float verticalVelocity = 0f;


    [Header("References")]
    CharacterController controller;
    [SerializeField] Transform playerCamera;

    Vector3 moveDirection;

    private void InputManagement()
    {
        //Apply WASD
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }


    private void GroundMovement()
    {
        Vector3 move = new Vector3(turnInput, 0, moveInput);

        move = playerCamera.transform.TransformDirection(move); // Makes sure player is moving in facing direction

        // Apply speed
        move *= walkSpeed;

        move.y = VerticalForceCalculation(); // Apply vertical force (gravity)

        // Move the player
        controller.Move(move * Time.deltaTime);
    }

    private void Turn()
    {
        // Rotate the player to face the direction of movement, only if there is movement input
        if (Mathf.Abs(turnInput) > 0 || Mathf.Abs(moveInput) > 0)
        {
            Vector3 currentLookAtDirection = controller.velocity.normalized;
            currentLookAtDirection.y = 0; // Keep rotation strictly horizontal

            currentLookAtDirection.Normalize(); // Ensure the direction is normalized

            Quaternion lookRotation = Quaternion.LookRotation(currentLookAtDirection);

            //Apply turn to character
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        }
    }

    private float VerticalForceCalculation()
    {
       if (controller.isGrounded)
        {
            verticalVelocity = -1f; // Apply a small downward force to keep the player grounded

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity); // Calculate jump velocity
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime; // Apply gravity when in the air

            if (isGliding)
            {
                verticalVelocity = Mathf.Max(verticalVelocity, -1f); // Limit the fall speed when gliding
            }
            
        }
        return verticalVelocity;
    }

    private void UmbrellaGlide()
    {
        // If not grounded, and pressing 'E' enable gliding
        if (!controller.isGrounded)
        {
            if (Input.GetButton("Jump"))
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
            isGliding = false; // Disable gliding when grounded
        }
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
