using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SwordControl : MonoBehaviour
{
    [Header("Uçuş Ayarları")]
    public float forwardSpeed = 40f;
    public float boostMultiplier = 1.5f;
    public float turnSpeed = 200f;

    [Header("Layer Ayarları")]
    public LayerMask bounceLayers;         // Ground
    public LayerMask sliceableLayers;      // Sliceable

    [Header("Sekme Ayarı")]
    public float bounceForce = 1.0f;       // 1 = Hızını korur, 1.2 = Hızlanır, 0.8 = Yavaşlar

    [Header("Görsel")]
    public Transform visualModel;
    public float bankingAngle = 45f;

    private Rigidbody rb;
    private float yaw, pitch;
    private float currentSpeed;

    // Sekme sırasında kontrolü anlık kitlemek için (Yoksa mouse ile sekmeyi bozarsın)
    private bool isBouncing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f; // Dönüşte direnç olmasın
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Başlangıç açısını al
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        currentSpeed = forwardSpeed;
    }

    void Update()
    {
        // Sekme anında mouse kontrolünü 0.2 saniye devre dışı bırakıyoruz
        // Bu sayede top gibi sektiğinde biz yanlışlıkla yönü bozmuyoruz
        if (!isBouncing)
        {
            HandleInput();
        }

        HandleVisuals();
    }

    void FixedUpdate()
    {
        // Kılıcı her zaman burnunun dikine (Forward) iteriz
        // Yönü değiştirdiğimiz an (sekince), kılıç otomatik oraya gider
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        rb.MoveRotation(targetRotation);

        rb.linearVelocity = transform.forward * currentSpeed;
    }

    void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * turnSpeed * Time.deltaTime;
        pitch -= mouseY * turnSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        // Hız kontrolü
        float targetSpeed = forwardSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) targetSpeed *= boostMultiplier;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f);
    }

    void HandleVisuals()
    {
        if (visualModel)
        {
            float targetBank = -Input.GetAxis("Mouse X") * bankingAngle;
            Quaternion bankRot = Quaternion.Euler(0, 0, targetBank);
            visualModel.localRotation = Quaternion.Slerp(visualModel.localRotation, bankRot, Time.deltaTime * 10f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        int hitLayer = collision.gameObject.layer;

        // --- TENİS TOPU MANTIĞI BURADA ---
        if (((1 << hitLayer) & bounceLayers) != 0)
        {
            // 1. Çarptığın yüzeyin normalini (baktığı yönü) al
            Vector3 normal = collision.contacts[0].normal;

            // 2. Geliş açısına göre yansıma açısını hesapla (Matematiksel Sekme)
            Vector3 reflectDir = Vector3.Reflect(transform.forward, normal);

            // 3. Kılıcın YÖN DEĞİŞKENLERİNİ (yaw/pitch) bu yeni yöne zorla!
            // İşte burası önceki kodda eksik olandı. Sadece hızı değil,
            // bizim kontrol ettiğimiz değişkenleri de güncelliyoruz.
            Quaternion bounceRot = Quaternion.LookRotation(reflectDir);

            yaw = bounceRot.eulerAngles.y;
            pitch = bounceRot.eulerAngles.x;

            // Eğer açılar 360 karışıklığı yaparsa düzelt
            if (pitch > 180) pitch -= 360;

            // 4. Hızı koru veya artır (Tenis topu efekti)
            currentSpeed *= bounceForce;

            // 5. Kısa bir süre kontrolü kitle (Cooldown)
            StartCoroutine(BounceCooldown());
        }
        // --- DÜŞMAN KESME ---
        else if (((1 << hitLayer) & sliceableLayers) != 0)
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>(), true);
            // Slice logic...
        }
    }

    IEnumerator BounceCooldown()
    {
        isBouncing = true;
        yield return new WaitForSeconds(0.15f); // 0.15 saniye mouse iptal
        isBouncing = false;
    }
}