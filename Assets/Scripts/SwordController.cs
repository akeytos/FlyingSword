using UnityEngine;

public class MnBFlyController : MonoBehaviour
{
    [Header("Uçuþ Ayarlarý")]
    public float flySpeed = 15f;    // Ýleri gidiþ hýzý
    public float strafeSpeed = 10f; // Yana kayýþ hýzý

    [Header("Mount & Blade Görüþ Ayarlarý")]
    public float mouseSensitivity = 2f; // Mouse hassasiyeti
    public float maxLookAngleX = 60f;   // Kýlýç saða/sola en fazla kaç derece dönsün? (Sýnýr)
    public float maxLookAngleY = 50f;   // Kýlýç yukarý/aþaðý en fazla kaç derece baksýn?
    public float bodyTurnMultiplier = 2f; // Sýnýra gelince kamera ne kadar hýzlý dönsün?

    [Header("Referanslar")]
    public Transform swordPivot; // Hiyerarþideki SwordPivot'u buraya at

    private Rigidbody rb;
    private float currentY = 0f; // Kýlýcýn þu anki sað/sol açýsý
    private float currentX = 0f; // Kýlýcýn þu anki yukarý/aþaðý açýsý

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Mouse'u gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb.useGravity = false;
        rb.linearDamping = 2f; // Unity eski sürümse 'drag' yap
    }

    void Update()
    {
        HandleMouseLook();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 1. Mouse hareketini açýlara ekle
        currentY += mouseX;
        currentX -= mouseY; // Yukarý bakmak için eksi

        // --- GÖVDE DÖNDÜRME MANTIÐI (M&B Stili) ---

        // YATAY (SAÐ/SOL) KONTROLÜ
        // Eðer kýlýç belirlenen sýnýrdan (maxLookAngleX) fazla dönmek isterse...
        if (currentY > maxLookAngleX)
        {
            // Aradaki farký bul (Ne kadar taþtýk?)
            float overshoot = currentY - maxLookAngleX;
            // O fark kadar GÖVDEYÝ (Kamerayý) saða döndür
            transform.Rotate(0, overshoot * bodyTurnMultiplier * Time.deltaTime * 60f, 0);
            // Kýlýcý sýnýrda tut
            currentY = maxLookAngleX;
        }
        else if (currentY < -maxLookAngleX)
        {
            float overshoot = currentY - (-maxLookAngleX); // Negatif fark
            transform.Rotate(0, overshoot * bodyTurnMultiplier * Time.deltaTime * 60f, 0);
            currentY = -maxLookAngleX;
        }

        // DÝKEY (YUKARI/AÞAÐI) KONTROLÜ
        // Gövdeyi yukarý aþaðý eðmek (pitch) istemeyiz genelde, sadece kýlýç sýnýrlansýn yeter.
        // Ama istersen "Uçak gibi takla atsýn" diyorsan buraya da rotate ekleriz.
        // Þimdilik sadece kýlýcý kilitliyoruz (Clamping).
        currentX = Mathf.Clamp(currentX, -maxLookAngleY, maxLookAngleY);


        // 2. KILIÇ PÝVOTUNU DÖNDÜR
        // Pivot kameranýn içinde olduðu için localRotation kullanýyoruz.
        // Böylece kýlýç ekranýn ortasýndan baðýmsýz hareket ediyor.
        if (swordPivot)
        {
            // Z eksenine (Roll) hafif bir eðim verelim mi? (Opsiyonel)
            // Mouse saða giderken kýlýç hafif saða yatsýn (-currentY * 0.5f gibi)
            float tilt = -currentY * 0.3f;

            swordPivot.localRotation = Quaternion.Euler(currentX, currentY, tilt);
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A-D
        float v = Input.GetAxis("Vertical");   // W-S

        // Hareket her zaman kameranýn (yani gövdenin) baktýðý yöne göre olur
        Vector3 moveDir = transform.right * h * strafeSpeed + transform.forward * v * flySpeed;

        rb.AddForce(moveDir, ForceMode.Acceleration);
    }
}