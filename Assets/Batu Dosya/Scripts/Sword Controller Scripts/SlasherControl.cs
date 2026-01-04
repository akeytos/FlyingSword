using UnityEngine;

public class SlasherControl : MonoBehaviour
{
    [Header("🔗 Bağlantılar")]
    public Transform pilot;       // Player_PILOT (Merkezimiz)
    public Transform socket;      // Kılıç Görseli (Socket)
    public Collider swordCollider;// Kılıcın içindeki Collider (Trigger)
    public TrailRenderer trail;   // Kılıcın izi (Görsellik için şart)

    [Header("⚙️ Kesme Ayarları")]
    public float distanceFromPlayer = 1.5f; // Kılıç karakterden ne kadar uzakta dursun?
    public float rotationSmoothing = 50f;   // Mouse takibi ne kadar keskin? (Yüksek = Anlık)

    [Header("⚡ Durumlar")]
    public bool isSlicing = false; // Şuan kesiyor muyuz?

    private Camera cam;
    private float targetAngle; // Mouse'un açısı

    void Start()
    {
        cam = Camera.main;

        // Başlangıçta kılıcın kesici özelliği kapalı olsun
        if (swordCollider) swordCollider.enabled = false;
        if (trail) trail.enabled = false;
    }

    void Update()
    {
        if (pilot == null) return;

        // 1. Mouse Pozisyonunu Bul (Zeminde nereyi gösteriyor?)
        Vector3 mousePos = GetMouseWorldPosition();

        // 2. Açıyı Hesapla (Atan2 Matematigi)
        // Karakterden Mouse'a giden vektör
        Vector3 direction = mousePos - pilot.position;
        direction.y = 0; // Yükseklik farkını yoksay

        if (direction != Vector3.zero)
        {
            // Bu vektörün açısını bul (Y ekseninde)
            Quaternion targetRot = Quaternion.LookRotation(direction);

            // Kılıcı karakterin etrafında döndür (Pivot noktası Pilot'tur)
            // Slerp ile çok hafif yumuşatıyoruz ki "titreme" olmasın, ama çok hızlı olacak.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothing * Time.deltaTime);
        }

        // 3. Pozisyonu Sabitle
        // Kılıç her zaman Pilot'un pozisyonunda (veya hafif önünde) durmalı
        // Ama "Player_DRONE" objesi Pilot'un tam merkezinde durup dönerse, 
        // içindeki "Socket"i ileri alarak mesafeyi ayarlarız.
        transform.position = pilot.position;

        // 4. Input Kontrolü (Kesme İşlemi)
        HandleInput();
    }

    void HandleInput()
    {
        // Mouse Sol Tık BASILI TUTULDUĞU SÜRECE
        if (Input.GetMouseButton(0))
        {
            if (!isSlicing) StartSlice();
        }
        else
        {
            if (isSlicing) StopSlice();
        }
    }

    void StartSlice()
    {
        isSlicing = true;
        if (swordCollider) swordCollider.enabled = true; // Kesmeye başla
        if (trail) trail.enabled = true; // İzi aç

        // Buraya "Kılıç Parlama Sesi" eklenebilir
    }

    void StopSlice()
    {
        isSlicing = false;
        if (swordCollider) swordCollider.enabled = false; // Kesmeyi durdur
        if (trail) trail.enabled = false; // İzi kapat
    }

    // Ekranın 2D Mouse koordinatını 3D Dünya koordinatına çevirir
    Vector3 GetMouseWorldPosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, pilot.position); // Pilot hizasında sanal zemin

        if (groundPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return Vector3.zero;
    }
}