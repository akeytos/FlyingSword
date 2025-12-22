using UnityEngine;

public class SwordMaster_SolidEdge : MonoBehaviour
{
    [Header("⚔️ KILIÇ AYARLARI")]
    public float motorGucu = 50f;
    public float mouseHassasiyeti = 1.5f;
    public float maksimumHiz = 30f;

    [Header("🎥 EKRAN SINIRLARI")]
    // 0.3 demek: Ekranın %30'luk kenar payı. Ortadaki %40'lık alanda kamera ÖLÜDÜR (kıpırdamaz).
    [Range(0.1f, 0.45f)] public float kenarPayi = 0.35f;

    [Header("🎥 KAMERA AYARLARI")]
    public Transform kameraObjesi;
    public float kameraMesafesi = 7f;   // Kılıçtan ne kadar geride?
    public float kameraYuksekligi = 3f; // Yerden ne kadar yüksekte?
    public float kameraDonusHizi = 60f; // Kenara gelince dönüş hızı
    public float kameraTakipHizi = 10f; // Pozisyon takip hızı (Yüksek = Zımba gibi)

    private Rigidbody rb;
    private Camera cam;
    private Vector2 viewportPos = new Vector2(0.5f, 0.5f); // Kılıcın ekrandaki sanal konumu

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        if (kameraObjesi == null) kameraObjesi = cam.transform;

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Fiziği sustur
        rb.linearDamping = 1f;

        // Mouse'u yok et
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // --- 1. KILIÇ EKRANDA GEZSİN ---
        float fareX = Input.GetAxis("Mouse X") * mouseHassasiyeti * Time.deltaTime;
        float fareY = Input.GetAxis("Mouse Y") * mouseHassasiyeti * Time.deltaTime;

        viewportPos.x += fareX;
        viewportPos.y += fareY;

        // Kılıcı ekran sınırlarına hapset (Asla dışarı taşmasın)
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.05f, 0.95f);
        viewportPos.y = Mathf.Clamp(viewportPos.y, 0.05f, 0.95f);
    }

    void FixedUpdate()
    {
        // --- 2. KAMERA NE ZAMAN DÖNECEK? ---
        float donusY = 0;
        float donusX = 0;

        // SAĞ/SOL SINIRI
        if (viewportPos.x > (0.5f + kenarPayi)) // Sağa dayandıysa
            donusY = (viewportPos.x - (0.5f + kenarPayi)) * kameraDonusHizi;
        else if (viewportPos.x < (0.5f - kenarPayi)) // Sola dayandıysa
            donusY = (viewportPos.x - (0.5f - kenarPayi)) * kameraDonusHizi;

        // YUKARI/AŞAĞI SINIRI
        if (viewportPos.y > (0.5f + kenarPayi)) // Yukarı dayandıysa
            donusX = -(viewportPos.y - (0.5f + kenarPayi)) * kameraDonusHizi;
        else if (viewportPos.y < (0.5f - kenarPayi)) // Aşağı dayandıysa
            donusX = -(viewportPos.y - (0.5f - kenarPayi)) * kameraDonusHizi;

        // Kamerayı döndür
        Vector3 mevcutRot = kameraObjesi.rotation.eulerAngles;
        // X açısını (Yukarı/Aşağı) 80 dereceyle sınırla ki tepe taklak olmasın
        float yeniX = mevcutRot.x + donusX;
        if (yeniX > 180) yeniX -= 360; // Açıyı düzelt
        yeniX = Mathf.Clamp(yeniX, -60f, 60f);

        kameraObjesi.rotation = Quaternion.Euler(yeniX, mevcutRot.y + donusY, 0);

        // --- 3. KILIÇ NEREYE BAKSIN? ---
        // Kamera açısından bir ışın yolla
        Ray ray = cam.ViewportPointToRay(new Vector3(viewportPos.x, viewportPos.y, 0));
        Vector3 hedefNokta = ray.GetPoint(20f); // İlerideki nokta
        Vector3 yon = (hedefNokta - transform.position).normalized;

        if (yon != Vector3.zero)
        {
            Quaternion hedefRot = Quaternion.LookRotation(yon);
            // Kılıç anında dönsün, gecikme olmasın
            rb.MoveRotation(hedefRot);
        }

        // --- 4. GAZLA ---
        if (Input.GetMouseButton(0))
        {
            rb.AddForce(transform.forward * motorGucu, ForceMode.Acceleration);
        }

        // Hız Limiti
        if (rb.linearVelocity.magnitude > maksimumHiz)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maksimumHiz;
        }
    }

    void LateUpdate()
    {
        if (kameraObjesi == null) return;

        // --- 5. KAMERA POZİSYONU (KİLİT NOKTA BURASI) 🛑 ---

        // ESKİSİ (Hatalı): transform.TransformPoint(...) -> Kılıç dönünce kamera da dönüyordu.
        // YENİSİ (Sağlam): Kılıcın pozisyonunu al, KAMERANIN arkasına at.

        // Hedef = Kılıcın Yeri - (Kameranın Baktığı Yön * Mesafe) + Yükseklik
        Vector3 hedefPozisyon = transform.position - (kameraObjesi.forward * kameraMesafesi) + (Vector3.up * kameraYuksekligi);

        // Kamerayı oraya götür
        kameraObjesi.position = Vector3.Lerp(kameraObjesi.position, hedefPozisyon, kameraTakipHizi * Time.deltaTime);
    }
}