using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    [Header("Gun")]
    public Gun equippedGun;

    private CharacterController controller;
    private PlayerInput input;

    private InputAction moveAction, sprintAction, lookAction, aimAction, fireAction;
    private Vector3 verticalVelocity;
    private float verticalLookRotation;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();

        moveAction = input.actions["Move"];
        sprintAction = input.actions["Sprint"];
        lookAction = input.actions["Look"];
        aimAction = input.actions["Aim"];
        fireAction = input.actions["Attack"];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleGunInput();
    }

    void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.TransformDirection(new Vector3(moveInput.x, 0f, moveInput.y));
        float speed = sprintAction.IsPressed() ? sprintSpeed : moveSpeed;

        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 totalMovement = move * speed + verticalVelocity;
        controller.Move(totalMovement * Time.deltaTime);
    }

    void HandleLook()
    {
        Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * (mouseDelta.x * mouseSensitivity * Time.deltaTime));

        verticalLookRotation -= mouseDelta.y * mouseSensitivity * Time.deltaTime;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);

        if (equippedGun != null)
            equippedGun.UpdateLookInput(mouseDelta);
    }

    void HandleGunInput()
    {
        if (equippedGun == null) return;

        bool isSprinting = sprintAction.IsPressed();

        equippedGun.SetAiming(!isSprinting && aimAction.IsPressed());
        equippedGun.SetSprinting(isSprinting);

        if (!isSprinting && fireAction.IsPressed())
            equippedGun.TryShoot();
    }
}
