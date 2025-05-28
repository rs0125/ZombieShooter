using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GunAiming : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction aimAction;
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction shootAction;

    public Transform gunTransform;
    public Transform hipTransform; // child of camera
    public Transform aimTransform; // also child of camera
    public RawImage crosshair;

    public float aimLerpSpeed = 10f;

    private bool isAiming = false;

    // Sway settings
    public float swayAmount = 2f;
    public float swaySmooth = 6f;
    public float maxSwayAngle = 5f;
    private Vector2 lookInput;
    private Vector3 swayEulerRotation = Vector3.zero;

    // Bobbing settings
    public float bobFrequency = 6f;
    public float bobAmplitude = 0.01f;
    private float bobTimer = 0f;
    private Vector3 bobOffset = Vector3.zero;

    // Recoil settings
    public float recoilKickback = 0.1f;
    public float recoilRecoverySpeed = 10f;
    private Vector3 recoilOffset = Vector3.zero;
    private bool isRecoiling = false;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        aimAction = playerInput.actions["Aim"];
        lookAction = playerInput.actions["Look"];
        moveAction = playerInput.actions["Move"];
    }

    void Update()
    {
        // Aiming
        isAiming = aimAction.IsPressed();
        crosshair.enabled = !isAiming;

        Vector3 targetLocalPos = isAiming ? aimTransform.localPosition : hipTransform.localPosition;
        Quaternion targetLocalRot = isAiming ? aimTransform.localRotation : hipTransform.localRotation;

        // Sway
        lookInput = lookAction.ReadValue<Vector2>();
        float swayX = Mathf.Clamp(-lookInput.y * swayAmount, -maxSwayAngle, maxSwayAngle);
        float swayY = Mathf.Clamp(lookInput.x * swayAmount, -maxSwayAngle, maxSwayAngle);
        Vector3 desiredSway = new Vector3(swayX, swayY, 0f);
        swayEulerRotation = Vector3.Lerp(swayEulerRotation, desiredSway, Time.deltaTime * swaySmooth);

        // Bobbing
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        if (moveInput.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            bobOffset = new Vector3(0f, Mathf.Sin(bobTimer) * bobAmplitude, 0f);
        }
        else
        {
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * bobFrequency);
        }

        // Recoil recovery
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);

        // Final position and rotation
        Vector3 finalPosition = targetLocalPos + bobOffset + recoilOffset;
        Quaternion finalRotation = targetLocalRot * Quaternion.Euler(swayEulerRotation);

        gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, finalPosition, Time.deltaTime * aimLerpSpeed);
        gunTransform.localRotation = Quaternion.Slerp(gunTransform.localRotation, finalRotation, Time.deltaTime * aimLerpSpeed);
    }

    public void ApplyRecoil()
    {
        recoilOffset -= new Vector3(0f, 0f, recoilKickback);
    }
}
