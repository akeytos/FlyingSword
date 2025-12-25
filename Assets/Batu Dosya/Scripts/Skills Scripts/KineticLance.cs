using UnityEngine;

public class KineticLance : MonoBehaviour
{
    [Header("--- SEV�YE & AYARLAR ---")]
    public int level = 1;
    public float chargeTimeNeeded = 2.0f; // M�zra��n a��lmas� i�in gereken s�re
    public float minSpeedToCharge = 8.0f; // Hangi h�z�n �st�ndeyken �arj olsun?
    public float rotationThreshold = 0.9f; // K�l�� ne kadar d�z gidiyor? (1 = D�md�z)

    [Header("--- SAVA� DE�ERLER� ---")]
    public int maxPenetration = 2;     // Ka� d��man�n i�inden ge�ebilir?
    public float explosionRadiusBase = 3f; // Patlama alan� geni�li�i

    [Header("--- G�RSELLER ---")]
    public GameObject lanceVisual;    // M�zrak Modeli (U�taki enerji konisi)
    public GameObject chargingVFX;    // �arj olurken ��kan k�v�lc�mlar
    public GameObject explosionVFX;   // D��mana �arp�nca ��kan patlama

    // --- �ZEL DE���KENLER ---
    private float currentChargeTimer = 0f;
    private bool isActive = false;       // M�zrak a��k m�?
    private int currentPenetrationLeft;  // Kalan delme hakk�
    private Rigidbody rb;
    private Vector3 lastVelocityDir;     // Y�n de�i�imi kontrol� i�in

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Ba�lang��ta efektleri kapat
        if (lanceVisual != null) lanceVisual.SetActive(false);
        if (chargingVFX != null) chargingVFX.SetActive(false);
    }

    void Update()
    {
        CheckActivationLogic();
    }

    void CheckActivationLogic()
    {
        if (rb == null) return;

        // 1. H�z ve Y�n Kontrol�
        // K�l�c�n h�z� yeterli mi?
        bool isFastEnough = rb.linearVelocity.magnitude >= minSpeedToCharge;

        // K�l�� y�n de�i�tiriyor mu? (Dot Product 1'e yak�nsa y�n ayn�d�r)
        // Unity 6 kullan�yorsan rb.linearVelocity, eski s�r�mse rb.velocity kullan
        Vector3 currentDir = rb.linearVelocity.normalized;
        float directionChange = Vector3.Dot(currentDir, lastVelocityDir);
        bool isFlyingStraight = directionChange > rotationThreshold;

        lastVelocityDir = currentDir; // Bir sonraki kare i�in kaydet

        // --- �ARJ MANTI�I ---
        if (isFastEnough && isFlyingStraight)
        {
            currentChargeTimer += Time.deltaTime;

            // G�rsel Geri Bildirim: %50 dolduysa �arj efektini yak
            if (chargingVFX != null && !isActive)
            {
                bool showCharging = currentChargeTimer > (chargeTimeNeeded * 0.5f);
                chargingVFX.SetActive(showCharging);
            }

            // S�re dolduysa A�
            if (currentChargeTimer >= chargeTimeNeeded && !isActive)
            {
                ActivateLance();
            }
        }
        else
        {
            // Yava�lad� veya d�nd� -> �arj� boz
            // E�er aktifse hemen kapatma, belki oyuncu hafif manevra yap�yordur (Tolerans eklenebilir)
            // Ama Bannerlord hissiyat� i�in sert kapat�yoruz:
            DeactivateLance();
        }
    }

    void ActivateLance()
    {
        isActive = true;
        currentPenetrationLeft = maxPenetration; // Hakk� fulle

        if (lanceVisual != null) lanceVisual.SetActive(true);
        if (chargingVFX != null) chargingVFX.SetActive(false); // �arj� kapat, m�zra�� a�

        // Ses efekti eklenebilir: AudioSource.PlayOneShot(lanceReadySound);
    }

    void DeactivateLance()
    {
        isActive = false;
        currentChargeTimer = 0f;

        if (lanceVisual != null) lanceVisual.SetActive(false);
        if (chargingVFX != null) chargingVFX.SetActive(false);
    }

    // --- �ARPI�MA (DELME MANTI�I) ---
    // Penetration (��inden ge�me) i�in TRIGGER kullanmak daha sa�l�kl�d�r.
    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Enemy"))
        {
            HandleImpact(other.gameObject);
        }
    }

    void HandleImpact(GameObject target)
    {
        // 1. PATLAMA YAP ??
        // K�l�c�n h�z� ne kadar y�ksekse patlama o kadar b�y�s�n
        float speedBonus = rb.linearVelocity.magnitude * 0.1f;
        Explode(target.transform.position, explosionRadiusBase + speedBonus);

        // 2. DELME HAKKINI D���R
        currentPenetrationLeft--;

        if (currentPenetrationLeft <= 0)
        {
            // Hakk�m�z bitti, m�zra�� kapat
            DeactivateLance();
        }
    }

    void Explode(Vector3 center, float radius)
    {
        // Patlama G�rseli
        if (explosionVFX != null) Instantiate(explosionVFX, center, Quaternion.identity);

        // Alan i�indeki herkesi bul
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        foreach (var hitCollider in hitColliders)
        {
            // D��man m�?
            if (hitCollider.CompareTag("Enemy"))
            {
                // Loot D���r
                EnemyStats stats = hitCollider.GetComponent<EnemyStats>();
                if (stats != null) stats.OnEnemySliced();

                // Yok Et
                Destroy(hitCollider.gameObject);

                // Fiziksel F�rlatma (Etraftaki di�er nesneler/k�r�k par�alar i�in)
                Rigidbody enemyRb = hitCollider.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    enemyRb.AddExplosionForce(1000f, center, radius);
                }
            }
        }
    }

    // --- UPGRADE S�STEM� ���N (PlayerSkillController �a��r�r) ---
    public void OnLevelUp()
    {
        level++;
        maxPenetration++;          // Art�k daha fazla ki�iyi deler
        explosionRadiusBase += 1f; // Patlama alan� b�y�r
        chargeTimeNeeded -= 0.2f;  // Daha h�zl� �arj olur

        Debug.Log("Kinetic Lance Seviye Atlad�! Level: " + level);
    }
}