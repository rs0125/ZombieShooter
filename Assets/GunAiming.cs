using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GunAiming : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction aimAction;

    public Transform gunTransform;
    public Transform hipTransform; // child of camera
    public Transform aimTransform; // also child of camera
    public RawImage crosshair;

    public float aimLerpSpeed = 10f;

    private bool isAiming = false;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        aimAction = playerInput.actions["Aim"];
    }

    void Update()
    {
        crosshair.enabled = true;
        isAiming = aimAction.IsPressed();
        if (isAiming)
        {
            crosshair.enabled = false;
        }

        // Smoothly lerp LOCAL position and rotation
        Vector3 targetLocalPos = isAiming ? aimTransform.localPosition : hipTransform.localPosition;
        Quaternion targetLocalRot = isAiming ? aimTransform.localRotation : hipTransform.localRotation;

        gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, targetLocalPos, Time.deltaTime * aimLerpSpeed);
        gunTransform.localRotation = Quaternion.Slerp(gunTransform.localRotation, targetLocalRot, Time.deltaTime * aimLerpSpeed);
    }
}