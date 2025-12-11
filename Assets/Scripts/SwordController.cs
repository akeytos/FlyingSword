using UnityEngine;

public class RealSwordFeel : MonoBehaviour
{
    [Header("Uçuþ Hýzý")]
    public float flySpeed = 15f;

    [Header("Bilek (Swing) Ayarlarý")]
    public Transform swordPivot;
    public float reachDistance = 10f; // Kýlýcýn ucunun baktýðý mesafe
    public float swingSpeed = 25f;    // Dönüþ hýzý (Bilek kývraklýðý)

    [Header("El Gezinmesi (KABZA HAREKETÝ) - ÖNEMLÝ")]
    public float handMoveRangeX = 2.0f; // El saða sola ne kadar gitsin? (Bunu artýr!)
    public float handMoveRangeY = 1.5f; // El yukarý aþaðý ne kadar gitsin?
    public float handMoveSpeed = 8f;    // Elin pozisyon deðiþtirme hýzý
    public float baseDistance = 1.5f;   // Kýlýcýn kameradan temel uzaklýðý

    [Header("Gövde Dönüþü")]
    public float bodyTurnSpeed = 60f;
    public float edgeThreshold = 0.85f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked; // Mouse kilitli
        Cursor.visible = false;

        rb.useGravity = false;
        rb.linearDamping = 5f;
    }

    // Sanal Mouse Pozisyonu (0.5 ekranýn ortasýdýr)
    private Vector2 virtualMouse = new Vector2(0.5f, 0.5f);

    void Update()
    {
        HandleInput();
        HandleSwordSwing();
        HandleBodyTurn();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleInput()
    {
        // Mouse hareketini alýp sanal imlece ekle
        float mouseX = Input.GetAxis("Mouse X") * 0.05f;
        float mouseY = Input.GetAxis("Mouse Y") * 0.05f;

        virtualMouse.x = Mathf.Clamp01(virtualMouse.x + mouseX);
        virtualMouse.y = Mathf.Clamp01(virtualMouse.y + mouseY);
    }

    void HandleSwordSwing()
    {
        if (!swordPivot) return;

        // --- 1. POZÝSYON (KABZANIN GEZÝNMESÝ) ---
        // Burasý kabzayý sabit kalmaktan kurtaran yer.
        // Mouse ne kadar saðdaysa, el o kadar saða kaysýn.

        // 0.5 çýkartýyoruz ki merkez 0 olsun (-0.5 ile 0.5 arasý)
        float targetX = (virtualMouse.x - 0.5f) * handMoveRangeX;
        float targetY = (virtualMouse.y - 0.5f) * handMoveRangeY;

        // Kýlýç yukarý bakarken hafif ileri de çýksýn (Reach) - Ýsteðe baðlý
        // float reachEffect = targetY > 0 ? targetY * 0.5f : 0; 

        Vector3 targetPos = new Vector3(targetX, targetY, baseDistance);

        // Kabzayý oraya götür
        swordPivot.localPosition = Vector3.Lerp(swordPivot.localPosition, targetPos, Time.deltaTime * handMoveSpeed);


        // --- 2. DÖNÜÞ (BÝLEK) ---
        // Mouse'un olduðu noktaya bak
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(virtualMouse.x, virtualMouse.y, 0));
        Vector3 lookPoint = ray.GetPoint(reachDistance);

        Vector3 direction = lookPoint - swordPivot.position;
        Quaternion targetRot = Quaternion.LookRotation(direction, Camera.main.transform.up);

        // Bileði döndür
        swordPivot.rotation = Quaternion.Lerp(swordPivot.rotation, targetRot, Time.deltaTime * swingSpeed);
    }

    void HandleBodyTurn()
    {
        // Saða Sola Dönüþ
        if (virtualMouse.x > edgeThreshold)
            transform.Rotate(Vector3.up * bodyTurnSpeed * Time.deltaTime);
        else if (virtualMouse.x < 1 - edgeThreshold)
            transform.Rotate(Vector3.up * -bodyTurnSpeed * Time.deltaTime);

        // Aþaðý Yukarý Kamera
        float pitch = 0f;
        if (virtualMouse.y > edgeThreshold) pitch = -bodyTurnSpeed * Time.deltaTime;
        if (virtualMouse.y < 1 - edgeThreshold) pitch = bodyTurnSpeed * Time.deltaTime;

        float currentX = Camera.main.transform.localEulerAngles.x;
        if (currentX > 180) currentX -= 360;

        float nextX = Mathf.Clamp(currentX + pitch, -80f, 80f);
        Camera.main.transform.localEulerAngles = new Vector3(nextX, 0, 0);
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveDir = Camera.main.transform.right * h + Camera.main.transform.forward * v;
        rb.AddForce(moveDir * flySpeed * 50f, ForceMode.Force);
    }
}