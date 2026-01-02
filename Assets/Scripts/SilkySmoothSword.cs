using UnityEngine;

public class RailShooter_Ultimate : MonoBehaviour
{
    [Header("⚔️ KILIÇ HAREKET AYARLARI")]
    public float ilerlemeHizi = 20f;   // Oyunun akış hızı
    public float xGenislik = 15f;      // Sağa/Sola ne kadar gidebilsin? (Küçük gelirse bunu arttır!)
    public float yYukseklik = 8f;      // Yukarı/Aşağı ne kadar gidebilsin?
    public float hareketYumusakligi = 15f; // Mouse takibi ne kadar keskin?

    [Header("🎥 KAMERA AYARLARI")]
    public Transform kameraObjesi;     // Main Camera'yı buraya sürükle
    public float kameraMesafesi = 12f; // Kamera kılıcın kaç metre arkasında?
    public float kameraYuksekligi = 3f;

    [Header("✨ GÖRSEL DÖNÜŞ")]
    public float donusHizi = 10f;

    private Rigidbody rb;
    private float mevcutZ; // Sürekli artan ilerleme mesafesi

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true; // Fizik motorunu kapatıyoruz, kontrol tamamen matematikte
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (kameraObjesi == null) kameraObjesi = Camera.main.transform;

        // Mouse imlecini yok et
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        // Başlangıç pozisyonunu kaydet
        mevcutZ = transform.position.z;
    }

    void Update()
    {
        // 1. İLERLEME HESABI (Sürekli ileri git)
        mevcutZ += ilerlemeHizi * Time.deltaTime;

        // 2. MOUSE HESABI (0 ile 1 arasını Dünya Koordinatına çevir)
        // Ekranın en solu (0) -> -xGenislik
        // Ekranın en sağı (1) -> +xGenislik
        Vector3 mouseViewport = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        // Mouse ekran dışına taşmasın (0-1 arası kilitle)
        mouseViewport.x = Mathf.Clamp01(mouseViewport.x);
        mouseViewport.y = Mathf.Clamp01(mouseViewport.y);

        // Matematiksel haritalama (Lerp)
        // Mouse 0 iken -15'e, Mouse 1 iken +15'e git diyoruz.
        float hedefX = Mathf.Lerp(-xGenislik, xGenislik, mouseViewport.x);
        float hedefY = Mathf.Lerp(1f, yYukseklik, mouseViewport.y); // Yerde sürünmesin diye 1f'den başlattım

        // 3. KILIÇ HAREKETİ
        // X ve Y: Mouse'un istediği yer
        // Z: Otomatik ilerleyen yer
        Vector3 hedefKonum = new Vector3(hedefX, hedefY, mevcutZ);

        // Yumuşak geçişle oraya git
        transform.position = Vector3.Lerp(transform.position, hedefKonum, hareketYumusakligi * Time.deltaTime);

        // 4. KILIÇ GÖRSEL DÖNÜŞÜ (Hafif Eğim)
        Vector3 hareketYonu = (hedefKonum - transform.position).normalized;
        if (hareketYonu != Vector3.zero)
        {
            Quaternion hedefRot = Quaternion.LookRotation(Vector3.forward + (hareketYonu * 0.3f));
            transform.rotation = Quaternion.Lerp(transform.rotation, hedefRot, donusHizi * Time.deltaTime);
        }

        // 5. KAMERA TAKİBİ (Çok Basit ve Stabil)
        // Kamera sadece Z ekseninde takip etsin. Sağa sola oynamasın ki ekran sabit kalsın.
        if (kameraObjesi != null)
        {
            Vector3 kameraHedef = new Vector3(0, kameraYuksekligi, mevcutZ - kameraMesafesi);
            kameraObjesi.position = Vector3.Lerp(kameraObjesi.position, kameraHedef, hareketYumusakligi * Time.deltaTime);
        }
    }
}