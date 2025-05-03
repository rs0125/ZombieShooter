using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction sprintAction;

    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    private CharacterController controller;
    private float verticalRotation = 0f;
    private Vector3 verticalVelocity;

    public Transform cameraTransform; // Assign this in Inspector (the Camera)

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Movement
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, input.y);
        move = transform.TransformDirection(move);
        float currentSpeed = sprintAction.IsPressed() ? sprintSpeed : moveSpeed;


        // Gravity
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; // small value to keep grounded
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        // Combine horizontal + vertical movement
        Vector3 totalMove = move * currentSpeed + verticalVelocity;
        controller.Move(totalMove * Time.deltaTime);

        // Mouse look
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        transform.Rotate(Vector3.up * (mouseDelta.x * mouseSensitivity * Time.deltaTime));

        verticalRotation -= mouseDelta.y * mouseSensitivity * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}