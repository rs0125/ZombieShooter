using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction fireAction;
    public AudioSource shootAudioSource;
    public AudioClip shootClip; // Assign in Inspector
    public ParticleSystem muzzleFlash;


    public Transform cameraTransform;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float shootForce = 30f;
    public float shootRange = 100f;
    public LayerMask shootableLayers;

    [Tooltip("Delay in seconds between shots")]
    public float fireRate = 0.2f;

    private float fireCooldown = 0f;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        fireAction = playerInput.actions["Attack"];
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (fireAction.IsPressed() && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = fireRate;
        }
    }

    void Shoot()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, shootRange, shootableLayers))
            targetPoint = hit.point;
        else
            targetPoint = cameraTransform.position + cameraTransform.forward * shootRange;

        Vector3 direction = (targetPoint - bulletSpawnPoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(direction));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = direction * shootForce;
        Destroy(bullet, 5f);

        // 🔊 Play audio
        shootAudioSource.PlayOneShot(shootClip);

        // 🔥 Play muzzle flash
        muzzleFlash.Play();
    }


}