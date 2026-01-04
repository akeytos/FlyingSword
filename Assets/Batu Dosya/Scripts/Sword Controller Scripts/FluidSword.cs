using UnityEngine;

public class FluidSword : MonoBehaviour
{
    [Header("🔗 Bağlantılar")]
    public Transform pilot;       // Player_PILOT (Merkezimiz)
    public Transform socket;      // Görselin olduğu Child obje (Socket)

    [Header("⚔️ Hissiyat Ayarları")]
    public float rotationSmoothTime = 0.1f; // 0.1 = Tok ve Hızlı, 0.2 = Ağır
    public float tiltAmount = 25f;          // Dönüş hızına göre yatma açısı
    public float distanceFromBody = 2.0f;   // Kılıç merkezden ne kadar uzakta?

    // Matematiksel Değişkenler
    private float currentYRotation;
    private float currentRotationVelocity;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (pilot == null) return;

        // Başlangıç açısını ayarla
        Vector3 startDir = (transform.position - pilot.position).normalized;
        currentYRotation = Mathf.Atan2(startDir.x, startDir.z) * Mathf.Rad2Deg;
    }

    void Update()
    {
        if (pilot == null) return;

        // 1. POZİSYON: Hep Pilot'un merkezinde kal
        transform.position = pilot.position;

        // 2. AÇI HESAPLAMA: Mouse'a göre hedef açıyı bul
        float targetAngle = GetMouseAngle();

        // 3. YUMUŞATMA (SMOOTHING): Hedefe "yaylanarak" git
        currentYRotation = Mathf.SmoothDampAngle(currentYRotation, targetAngle, ref currentRotationVelocity, rotationSmoothTime);

        // 4. UYGULA: Dönüşü yap
        transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);

        // 5. SOCKET AYARLARI (Mesafe ve Yatma)
        if (socket != null)
        {
            // Mesafeyi ayarla
            socket.localPosition = new Vector3(0, 0, distanceFromBody);

            // Tilt (Yatma) Efekti: Hıza göre sağa/sola yatır
            // Hız çok yüksekse max 25 derece yatsın
            float tiltZ = Mathf.Clamp(currentRotationVelocity * -0.05f, -tiltAmount, tiltAmount);
            socket.localRotation = Quaternion.Euler(0, 0, tiltZ);
        }
    }

    float GetMouseAngle()
    {
        // Mouse ekranın neresinde?
        Vector3 mousePos = Input.mousePosition;
        // Pilot ekranın neresinde?
        Vector3 playerScreenPos = cam.WorldToScreenPoint(pilot.position);

        // İkisi arasındaki açıyı bul (Atan2)
        return Mathf.Atan2(mousePos.x - playerScreenPos.x, mousePos.y - playerScreenPos.y) * Mathf.Rad2Deg;
    }
}