using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PilotHover : MonoBehaviour
{
    [Header("🛸 Uçuş Ayarları")]
    public float hoverHeight = 2.0f;   // Yerden yükseklik
    public float hoverForce = 50f;     // Kaldırma gücü (Biraz artırdım)
    public float damping = 10f;        // Titreme önleyici (Süspansiyon sertliği)

    [Header("🌍 Zemin Ayarı (ÇOK ÖNEMLİ)")]
    public LayerMask groundLayer;      // Sadece bu katmandakileri "Yer" sayar

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Yerçekimini kapatıyoruz, biz yöneteceğiz
        rb.linearDamping = 2f;          // Havada buzda kayar gibi gitmemesi için sürtünme
        rb.angularDamping = 10f;  // Kendi etrafında dönmemesi için

        // Dengesiz dönmeleri engelle (Sadece Y ekseninde dönebilsin)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        // Raycast'i karakterin tam merkezinden değil, hafif altından başlat (Kendi içine çarpmasın diye)
        // Ama en garantisi LayerMask kullanmaktır.
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        // SADECE "groundLayer" İLE BELİRTİLEN ZEMİNLERİ GÖR
        if (Physics.Raycast(ray, out hit, hoverHeight + 3.0f, groundLayer))
        {
            float distanceToGround = hit.distance;
            float heightError = hoverHeight - distanceToGround;

            // PID Kontrolcü Mantığı (Yaylanma)
            // Yukarıdaysak aşağı çek, aşağıdaysak yukarı it
            float currentUpSpeed = rb.linearVelocity.y;
            float force = (heightError * hoverForce) - (currentUpSpeed * damping);

            rb.AddForce(Vector3.up * force, ForceMode.Acceleration);
        }
        else
        {
            // Eğer altımızda zemin yoksa (uçurumdaysak veya çok yüksekteysek)
            // Yavaşça aşağı süzül (Yapay Yerçekimi)
            rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
    }
}