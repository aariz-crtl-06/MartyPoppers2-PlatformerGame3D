using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    private float moveInput;
    private float turnInput;

    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float turnSpeed = 2f;

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

        move = transform.TransformDirection(move); // Makes sure player is moving in facing direction

        // Don't allow player to move up vertically
        move.y = 0;

        // Apply speed
        move *= walkSpeed;

        // Move the player
        controller.Move(move * Time.deltaTime);
    }

    private void Turn()
    {
        // Rotate the player to face the direction of movement, only if there is movement input
        if (Mathf.Abs(turnInput) > 0 || Mathf.Abs(moveInput) > 0)
        {
            Vector3 currentLookAtDirection = playerCamera.forward;
            currentLookAtDirection.y = 0; // Keep rotation strictly horizontal

            Quaternion lookRotation = Quaternion.LookRotation(currentLookAtDirection);

            //Apply turn to character
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
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
        Movement();
    }
}
