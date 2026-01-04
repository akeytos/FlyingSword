using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [Header("🔗 Bağlantılar")]
    public BoxCollider bladeCollider; // BladeHitBox objesindeki Collider'ı buraya sürükle!

    [Header("⚡ Hız ve Kontrol Ayarları")]
    public float minCutSpeed = 5.0f;  // Kesmenin aktif olması için gereken hız
    public float maxSpeedCap = 25f;   // Hitbox'ın max büyüklüğe ulaşacağı hız

    [Header("📏 Dinamik Hitbox")]
    // Hitbox'ın orijinal boyutunu kod otomatik alır, elle girmene gerek yok
    private Vector3 originalSize;
    private Vector3 lastPosition;
    private float currentSpeed;

    void Start()
    {
        // Eğer BladeHitBox'ı atamayı unuttuysan childlardan bulmaya çalış
        if (bladeCollider == null)
            bladeCollider = GetComponentInChildren<BoxCollider>();

        if (bladeCollider != null)
        {
            originalSize = bladeCollider.size;
            bladeCollider.enabled = false; // Başlangıçta kesici kapalı olsun (Güvenlik)
        }

        lastPosition = transform.position;
    }

    void Update()
    {
        CalculateSpeed();
        ManageHitBox();
    }

    void CalculateSpeed()
    {
        // Hız = Yol / Zaman
        float distance = (transform.position - lastPosition).magnitude;
        currentSpeed = distance / Time.deltaTime;
        lastPosition = transform.position;
    }

    void ManageHitBox()
    {
        if (bladeCollider == null) return;

        // 1. Eğer Hız YETERLİ İSE (Saldırı Modu)
        if (currentSpeed > minCutSpeed)
        {
            // Kesiciyi Aktif Et (Artık SlicerTrigger çalışabilir)
            bladeCollider.enabled = true;

            // Hitbox'ı Hıza Göre Büyüt (Daha kolay vuruş için)
            float expansion = 1f + (Mathf.Clamp01(currentSpeed / maxSpeedCap) * 0.5f);

            // X ve Z ekseninde şişiriyoruz (Kılıcın kalınlığı ve genişliği)
            bladeCollider.size = new Vector3(originalSize.x * expansion, originalSize.y, originalSize.z * expansion);
        }
        // 2. Eğer Hız YETERSİZ İSE (Dinlenme Modu)
        else
        {
            // Kesiciyi Kapat (Böylece dururken dokununca kesmez)
            bladeCollider.enabled = false;

            // Boyutu sıfırla
            bladeCollider.size = originalSize;
        }
    }

    // Debug için hızı görebilirsin
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}