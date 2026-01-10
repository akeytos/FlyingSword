using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SwordMasterController : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform swordPivot;
    public TrailRenderer swordTrail;
    public Camera playerCamera;

    [Header("Input Ayarları")]
    public float mouseSensitivity = 0.05f;

    [HideInInspector] public Vector2 virtualMouse = new Vector2(0.5f, 0.5f);
    [HideInInspector] public bool isAttacking = false;

    private float horizontalInput;
    private float verticalInput;
    private float currentCameraPitch = 0f;

    [Header("Uçuş Ayarları")]
    public float flySpeed = 15f;
    public float linearDamping = 5f;

    [Header("Dash Ayarları")]
    public float dashSpeedMultiplier = 3f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.0f;
    private bool isDashing = false;
    private float lastDashTime = -10f;

    [Header("Kılıç Mekaniği")]
    public float reachDistance = 10f;
    public float swingSpeed = 25f;
    public float trailSpeedThreshold = 0.1f;

    [Header("El Pozisyonu")]
    public float handMoveRangeX = 2.0f;
    public float handMoveRangeY = 1.5f;
    public float handMoveSpeed = 8f;
    public float baseDistance = 1.5f;

    private Vector2 lastMousePosForTrail;

    [Header("Gövde ve Kamera Dönüşü")]
    public float bodyTurnSpeed = 60f;
    public float edgeThreshold = 0.85f;

    [Header("Denge")]
    public float duzelmeHizi = 5.0f;
    public float maxYatis = 30f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Start()
    {
        rb.useGravity = false;
        rb.linearDamping = linearDamping;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentCameraPitch = playerCamera.transform.localEulerAngles.x;
        if (currentCameraPitch > 180) currentCameraPitch -= 360;
    }

    void Update()
    {
        HandleInput();
        HandleSwordMovement();
        HandleTrail();
        HandleBodyRotation();

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDashTime + dashCooldown)
        {
            StartCoroutine(DashRoutine());
        }
    }

    void FixedUpdate()
    {
        HandleFlight();
        HandleStabilization();
        rb.angularVelocity = Vector3.zero;
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        float originalWidth = 1f;
        if (swordTrail != null)
        {
            originalWidth = swordTrail.widthMultiplier;
            swordTrail.widthMultiplier *= 2f;
            swordTrail.emitting = true;
        }

        float originalFOV = playerCamera.fieldOfView;
        float targetFOV = originalFOV + 10f;

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * 10f);
            yield return null;
        }

        isDashing = false;

        if (swordTrail != null) swordTrail.widthMultiplier = originalWidth;

        float resetTime = Time.time;
        while (Time.time < resetTime + 0.5f)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, originalFOV, Time.deltaTime * 5f);
            yield return null;
        }
        playerCamera.fieldOfView = originalFOV;
    }

    void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        virtualMouse.x = Mathf.Clamp01(virtualMouse.x + mouseX);
        virtualMouse.y = Mathf.Clamp01(virtualMouse.y + mouseY);

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    void HandleSwordMovement()
    {
        if (swordPivot == null) return;

        float targetX = (virtualMouse.x - 0.5f) * handMoveRangeX;
        float targetY = (virtualMouse.y - 0.5f) * handMoveRangeY;
        Vector3 targetPos = new Vector3(targetX, targetY, baseDistance);
        swordPivot.localPosition = Vector3.Lerp(swordPivot.localPosition, targetPos, Time.deltaTime * handMoveSpeed);

        if (!isAttacking)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(virtualMouse.x, virtualMouse.y, 0));
            Vector3 lookPoint = ray.GetPoint(reachDistance);

            Vector3 direction = lookPoint - swordPivot.position;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction, playerCamera.transform.up);
                swordPivot.rotation = Quaternion.Lerp(swordPivot.rotation, targetRot, Time.deltaTime * swingSpeed);
            }
        }
    }

    void HandleTrail()
    {
        if (swordTrail == null) return;
        if (isAttacking || isDashing) return;

        float mouseSpeed = (virtualMouse - lastMousePosForTrail).magnitude / Time.deltaTime;
        swordTrail.emitting = mouseSpeed > trailSpeedThreshold;
        lastMousePosForTrail = virtualMouse;
    }

    void HandleBodyRotation()
    {
        if (virtualMouse.x > edgeThreshold)
            transform.Rotate(Vector3.up * bodyTurnSpeed * Time.deltaTime);
        else if (virtualMouse.x < 1 - edgeThreshold)
            transform.Rotate(Vector3.up * -bodyTurnSpeed * Time.deltaTime);

        float pitchChange = 0f;
        if (virtualMouse.y > edgeThreshold) pitchChange = -bodyTurnSpeed * Time.deltaTime;
        if (virtualMouse.y < 1 - edgeThreshold) pitchChange = bodyTurnSpeed * Time.deltaTime;

        if (Mathf.Abs(pitchChange) > 0.001f)
        {
            currentCameraPitch += pitchChange;
            currentCameraPitch = Mathf.Clamp(currentCameraPitch, -80f, 80f);
            playerCamera.transform.localEulerAngles = new Vector3(currentCameraPitch, 0, 0);
        }
    }

    void HandleFlight()
    {
        Vector3 moveDir = playerCamera.transform.right * horizontalInput + playerCamera.transform.forward * verticalInput;

        float currentSpeed = isDashing ? (flySpeed * dashSpeedMultiplier) : flySpeed;

        rb.AddForce(moveDir * currentSpeed * 50f, ForceMode.Force);
    }

    void HandleStabilization()
    {
        Vector3 currentRot = rb.rotation.eulerAngles;
        float zAngle = currentRot.z;
        if (zAngle > 180) zAngle -= 360;

        float clampedZ = Mathf.Clamp(zAngle, -maxYatis, maxYatis);
        float newZ = Mathf.Lerp(clampedZ, 0, duzelmeHizi * Time.fixedDeltaTime);

        Quaternion targetRotation = Quaternion.Euler(currentRot.x, currentRot.y, newZ);
        rb.MoveRotation(targetRotation);
    }

    public void ApplyRecoil(float forceGucu = 20f)
    {
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(-transform.forward * forceGucu, ForceMode.Impulse);
    }
}