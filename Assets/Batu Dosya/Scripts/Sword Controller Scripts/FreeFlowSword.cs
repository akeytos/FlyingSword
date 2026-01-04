using UnityEngine;

public class MomentumSword : MonoBehaviour
{
    [Header("🔗 Bağlantılar")]
    public Transform pilot;       // Player_PILOT (Merkez)
    public Transform visualSword; // VisualSword (Kılıcın kendisi)

    [Header("⚙️ Fizik Ayarları")]
    public float rotationSpeed = 15f;    // Dönüş hızı (Düşük = Çok Ağır, Yüksek = Hafif)
    public float reachDistance = 2.5f;   // Kılıç merkezden ne kadar uzakta dursun?
    public float swordHeight = 1.5f;     // Yerden yükseklik

    [Header("🌊 Hissiyat (Juice)")]
    public float tiltAmount = 30f;       // Dönüşlerde kılıç ne kadar yatsın?
    public float bobbingSpeed = 2f;      // Havada süzülme hızı
    public float bobbingAmount = 0.2f;   // Havada süzülme miktarı

    // Matematiksel Değişkenler
    private Camera cam;
    private Rigidbody rb;
    private float currentAngle; // Şu anki Y açımız
    private float targetAngle;  // Gitmek istediğimiz açı

    // Hız hesaplaması için (Hasar sistemi kullanabilsin diye)
    [HideInInspector] public float currentAngularVelocity;
    private float lastAngle;

    void Start()
    {
        cam = Camera.main;

        // Kılıç görselinin Rigidbody'sini alıyoruz (Çünkü fiziği ona uygulayacağız)
        if (visualSword != null)
        {
            rb = visualSword.GetComponent<Rigidbody>();

            // FİZİK AYARLARINI OTOMATİK DÜZELT (Hata yapmanı engeller)
            rb.useGravity = false;      // Yerçekimi yok
            rb.isKinematic = true;      // Kinematic OLMALI (MoveRotation için)
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Titremeyi önler
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // İçinden geçmeyi önler
        }
        else
        {
            Debug.LogError("❌ HATA: VisualSword'u scripte atamayı unuttun!");
        }

        if (pilot != null) transform.position = pilot.position;
    }

    void Update()
    {
        if (pilot == null || rb == null) return;

        // 1. POZİSYON TAKİBİ (Drone)
        // Drone, Pilot'u takip eder ama hafifçe yukarı aşağı yaylanır (Canlı hissi)
        float bobbing = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
        Vector3 targetPos = pilot.position;
        targetPos.y = swordHeight + bobbing; // Yükseklik ayarı + Bobbing
        transform.position = targetPos; // Drone merkeze gelir

        // 2. MOUSE HESAPLAMASI (Hedef Açıyı Bul)
        CalculateTargetAngle();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 3. FİZİKSEL DÖNÜŞ (En önemli kısım)
        // Lerp kullanarak açıyı yumuşatıyoruz (Ağırlık hissi burada oluşur)
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);

        // Kılıç Görselini döndür
        // Sadece Y ekseninde değil, hafifçe yatırarak (Tilt) döndür
        ApplyPhysicsRotation();

        // 4. KONUMU GÜNCELLE (Orbit)
        // Kılıcı merkezden "reachDistance" kadar uzağa koyuyoruz
        UpdateSwordPosition();

        // Hız hesabı (Hasar sistemi için)
        currentAngularVelocity = Mathf.Abs(Mathf.DeltaAngle(currentAngle, lastAngle)) / Time.fixedDeltaTime;
        lastAngle = currentAngle;
    }

    void CalculateTargetAngle()
    {
        // Mouse ekranın neresinde?
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, swordHeight, 0)); // Kılıç hizasında zemin

        float enter;
        if (groundPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = hitPoint - transform.position; // Merkezden Mouse'a vektör
            direction.y = 0; // Yükseklik farkını yoksay

            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                targetAngle = targetRot.eulerAngles.y;
            }
        }
    }

    void ApplyPhysicsRotation()
    {
        // TILT EFEKTİ: Kılıç dönerken yana yatsın
        // Hedef açı ile şu anki açı arasındaki farka göre yatma miktarı
        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
        float tiltZ = Mathf.Clamp(angleDiff * -0.5f, -tiltAmount, tiltAmount);

        // Yeni rotasyonu oluştur
        Quaternion newRotation = Quaternion.Euler(0, currentAngle, tiltZ);

        // MoveRotation kullanarak fiziği zorluyoruz. 
        // Bu sayede kılıç duvarın içinden geçmez, duvara sürter!
        rb.MoveRotation(newRotation);
    }

    void UpdateSwordPosition()
    {
        // Kılıcı merkezden (Drone) ileri doğru it
        // Matematiksel olarak Y açısına göre ileri vektörü bul
        Quaternion rotationY = Quaternion.Euler(0, currentAngle, 0);
        Vector3 offset = rotationY * Vector3.forward * reachDistance;

        // Kılıcı yeni yerine fiziksel olarak taşı
        rb.MovePosition(transform.position + offset);
    }
}