using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [Header("Transforms")]
    public Transform gunTransform;
    public Transform hipTransform;
    public Transform aimTransform;
    public Transform sprintTransform;
    public RawImage crosshair;

    [Header("Aiming")]
    public float aimLerpSpeed = 10f;
    private bool isAiming;

    [Header("Sway")]
    public float swayAmount = 2f;
    public float swaySmooth = 6f;
    public float maxSwayAngle = 5f;
    private Vector2 lookInput;
    private Vector3 swayEulerRotation;

    public float positionalSwayAmount = 0.02f;
    public float positionalSwaySmooth = 8f;
    private Vector3 swayPositionalOffset;

    [Header("Bobbing")]
    public float bobFrequency = 6f;
    public float bobAmplitude = 0.01f;
    private float bobTimer;
    private Vector3 bobOffset;

    [Header("Recoil Position")]
    public float recoilKickback = 0.1f;
    public float recoilRecoverySpeed = 10f;
    private Vector3 recoilOffset;

    [Header("Recoil Rotation")]
    public float recoilRotationAmount = 2f;
    public float recoilRotationRecoverySpeed = 8f;
    private Vector3 recoilRotationOffset;

    [Header("Shooting")]
    public Transform cameraTransform;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float shootForce = 30f;
    public float shootRange = 100f;
    public float fireRate = 0.2f;
    public LayerMask shootableLayers;
    public AudioSource shootAudioSource;
    public AudioClip shootClip;
    public ParticleSystem muzzleFlash;

    [Header("Crosshair Bounce")]
    public float crosshairBounceAmount = 20f;
    public float crosshairBounceSpeed = 8f;

    [Header("Sprinting")]
    public float sprintLerpSpeed = 6f;
    public float sprintOscillationAmplitude = 0.01f;
    public float sprintOscillationFrequency = 8f;
    private bool isSprinting;
    private float sprintTimer;

    private float fireCooldown;
    private float crosshairVerticalOffset = 0f;
    private Vector2 crosshairOriginalPos;

    void Start()
    {
        if (crosshair != null)
        {
            crosshairOriginalPos = crosshair.rectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;
        UpdateGunTransform();
    }

    void UpdateGunTransform()
    {
        Vector3 targetLocalPos;
        Quaternion targetLocalRot;

        if (isSprinting)
        {
            targetLocalPos = sprintTransform.localPosition;
            targetLocalRot = sprintTransform.localRotation;

            // Add dynamic sprint bobbing oscillation
            sprintTimer += Time.deltaTime * sprintOscillationFrequency;
            float oscX = Mathf.Sin(sprintTimer) * sprintOscillationAmplitude;
            float oscY = Mathf.Cos(sprintTimer * 2f) * sprintOscillationAmplitude * 0.5f;
            Vector3 sprintOscillation = new Vector3(oscX, oscY, 0f);

            swayPositionalOffset += sprintOscillation;
        }
        else
        {
            targetLocalPos = isAiming ? aimTransform.localPosition : hipTransform.localPosition;
            targetLocalRot = isAiming ? aimTransform.localRotation : hipTransform.localRotation;
        }

        float swayX = Mathf.Clamp(-lookInput.y * swayAmount, -maxSwayAngle, maxSwayAngle);
        float swayY = Mathf.Clamp(lookInput.x * swayAmount, -maxSwayAngle, maxSwayAngle);
        Vector3 desiredSway = new Vector3(swayX, swayY, 0f);
        swayEulerRotation = Vector3.Lerp(swayEulerRotation, desiredSway, Time.deltaTime * swaySmooth);

        Vector3 desiredPositionalSway = new Vector3(
            Mathf.Clamp(-lookInput.x * positionalSwayAmount, -positionalSwayAmount, positionalSwayAmount),
            Mathf.Clamp(-lookInput.y * positionalSwayAmount, -positionalSwayAmount, positionalSwayAmount),
            0f);
        swayPositionalOffset = Vector3.Lerp(swayPositionalOffset, desiredPositionalSway, Time.deltaTime * positionalSwaySmooth);

        Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        if (moveInput.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            bobOffset = new Vector3(0f, Mathf.Sin(bobTimer) * bobAmplitude, 0f);
        }
        else
        {
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * bobFrequency);
        }

        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
        recoilRotationOffset = Vector3.Lerp(recoilRotationOffset, Vector3.zero, Time.deltaTime * recoilRotationRecoverySpeed);

        Vector3 finalPosition = targetLocalPos + bobOffset + recoilOffset + swayPositionalOffset;
        Quaternion recoilRot = Quaternion.Euler(recoilRotationOffset);
        Quaternion finalRotation = targetLocalRot * Quaternion.Euler(swayEulerRotation) * recoilRot;

        gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, finalPosition, Time.deltaTime * aimLerpSpeed);
        gunTransform.localRotation = Quaternion.Slerp(gunTransform.localRotation, finalRotation, Time.deltaTime * aimLerpSpeed);

        if (crosshair != null)
        {
            crosshair.enabled = !isAiming && !isSprinting;
            crosshairVerticalOffset = Mathf.Lerp(crosshairVerticalOffset, 0f, Time.deltaTime * crosshairBounceSpeed);
            Vector2 targetCrosshairPos = crosshairOriginalPos + new Vector2(0f, crosshairVerticalOffset);
            crosshair.rectTransform.anchoredPosition = targetCrosshairPos;
        }
    }

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;
    }

    public void TryShoot()
    {
        if (isSprinting)
        {
            Debug.Log("Cannot shoot while sprinting");
            return;
        }

        if (fireCooldown > 0f)
        {
            Debug.Log($"Cannot shoot: cooling down ({fireCooldown:F2}s)");
            return;
        }

        Shoot();
        fireCooldown = fireRate;
    }

    void Shoot()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, shootRange, shootableLayers)
            ? hit.point
            : cameraTransform.position + cameraTransform.forward * shootRange;

        Vector3 direction = (targetPoint - bulletSpawnPoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(direction));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * shootForce;
        Destroy(bullet, 5f);

        recoilOffset -= new Vector3(0f, 0f, recoilKickback);

        float recoilX = Random.Range(recoilRotationAmount * 0.5f, recoilRotationAmount);
        float recoilY = Random.Range(-recoilRotationAmount, recoilRotationAmount);
        recoilRotationOffset += new Vector3(-recoilX, recoilY, 0f);

        crosshairVerticalOffset = crosshairBounceAmount;

        shootAudioSource?.PlayOneShot(shootClip);
        muzzleFlash?.Play();
    }

    public void UpdateLookInput(Vector2 look)
    {
        lookInput = look;
    }
}
