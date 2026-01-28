using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    [Header("--- DROP PREFABLARI ---")]
    public GameObject xpGemPrefab;
    public GameObject coinPrefab;

    [Header("--- DÜŞME ORANLARI (%) ---")]
    public float xpDropChance = 100f;
    public float coinDropChance = 20f;

    [Header("--- TEMEL STATLAR ---")]
    public float maxHealth = 10f;
    private float currentHealth;
    private bool isDead = false; // Çifte ölümü engellemek için koruma

    [Header("--- ELITE & ARMOR AYARLARI ---")]
    public bool isElite = false;
    public float armor = 0f;
    public float eliteScaleMultiplier = 2.0f;
    public float eliteHealthMultiplier = 5.0f;

    [Header("--- HASAR HİSSİYATI (JUICE) ---")]
    public float knockbackGucu = 15f;
    public float flashSuresi = 0.1f;
    public float scatterRange = 1.0f;

    // --- FLASH İÇİN GEREKLİLER ---
    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material whiteFlashMaterial;

    private Rigidbody rb;
    private Collider col; // Collider kontrolü için
    private Vector3 baseScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>(); // Collider'ı al

        // Renderları ve Orijinal Materyalleri Al
        renderers = GetComponentsInChildren<Renderer>();

        // Hata koruması: Renderer yoksa dizi oluşturma
        if (renderers != null && renderers.Length > 0)
        {
            originalMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
            }
        }

        // Universal Render Pipeline (URP) veya Standart Shader desteği
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color"); // Standart fallback

        whiteFlashMaterial = new Material(shader);
        if (whiteFlashMaterial.HasProperty("_BaseColor"))
            whiteFlashMaterial.SetColor("_BaseColor", Color.white); // URP için
        else
            whiteFlashMaterial.color = Color.white; // Standart için
    }

    void OnEnable()
    {
        // 1. ÖLÜM DURUMUNU SIFIRLA
        isDead = false;

        // 2. COLLIDER'I AÇ (Önceki ölümde kapanmış olabilir)
        if (col != null) col.enabled = true;

        // 3. FİZİĞİ SIFIRLA (Önceki ölümden kalan savrulma hızını durdur)
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Unity 6'da linearVelocity, eski sürümlerde velocity
            rb.angularVelocity = Vector3.zero;
        }

        // 4. ELITE KONTROLÜ VE CAN SIFIRLAMA
        if (isElite)
        {
            baseScale = Vector3.one * eliteScaleMultiplier;
            currentHealth = maxHealth * eliteHealthMultiplier;
        }
        else
        {
            baseScale = Vector3.one;
            currentHealth = maxHealth;
        }

        transform.localScale = baseScale;
        ResetMaterialsImmediately(); // Renkleri düzelt
    }

    public bool TakeDamage(float amount)
    {
        if (isDead) return true; // Zaten ölüyse işlem yapma

        float finalDamage = amount - armor;
        if (finalDamage < 1) finalDamage = 1;

        currentHealth -= finalDamage;

        // --- EFEKTLER ---
        // Coroutine çakışmasını önlemek için önce durdur, sonra başlat
        StopCoroutine("FlashRoutine");
        StartCoroutine(FlashRoutine());

        if (rb != null)
        {
            // Geri tepme (Recoil)
            rb.linearVelocity = Vector3.zero; // Önce hızı sıfırla ki net tepsin
            rb.AddForce(-transform.forward * knockbackGucu, ForceMode.Impulse);
        }

        StartCoroutine(ScalePunchRoutine());
        // ----------------

        // ÖLDÜ MÜ?
        if (currentHealth <= 0)
        {
            // NOT: Burada HandleDeath çağırmıyoruz! 
            // Çünkü SlicerTrigger, "DeathSequence" ile zamanı durduracak.
            // SlicerTrigger işini bitirince "OnEnemySliced" çağıracak, asıl ölüm orada olacak.
            return true;
        }

        return false;
    }

    // Bu fonksiyon SlicerTrigger tarafından çağrılacak (Sinematik bittikten sonra)
    public void OnEnemySliced()
    {
        if (isDead) return; // Zaten işlendiyse tekrar yapma
        HandleDeath();
    }

    void HandleDeath()
    {
        isDead = true;

        // --- SESİ BURADA ÇAL ---
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeathSFX();
        }

        // Loot düşür
        DropLoot();

        // Kill sayısını arttır
        if (GameManager.Instance != null) GameManager.Instance.AddKill();

        // Collider'ı kapat ki düşerken kılıca tekrar çarpmasın
        if (col != null) col.enabled = false;

        // Havuza geri gönder (SlicerTrigger zaten SetActive(false) yapacak ama garanti olsun)
        // Eğer SlicerTrigger bu objeyi "ReturnToPool" yapıyorsa burayı boş bırakabilirsin.
        // Ama genellikle Loot düştükten sonra EnemyPool'a biz haber veririz.
        // SlicerTrigger'ın en sonunda `target.SetActive(false)` olduğu için burası sadece mantıksal işlem yapar.
    }

    IEnumerator FlashRoutine()
    {
        if (renderers == null) yield break;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].material = whiteFlashMaterial;
        }

        yield return new WaitForSeconds(flashSuresi);

        ResetMaterials();
    }

    void ResetMaterials()
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalMaterials.Length)
                renderers[i].material = originalMaterials[i];
        }
    }

    public void ResetMaterialsImmediately()
    {
        StopAllCoroutines();
        ResetMaterials();
    }

    IEnumerator ScalePunchRoutine()
    {
        float duration = 0.15f;
        Vector3 targetScale = baseScale * 1.2f;

        float timer = 0f;
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(targetScale, baseScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = baseScale;
    }

    void DropLoot()
    {
        if (Random.Range(0f, 100f) <= xpDropChance && xpGemPrefab != null)
            SpawnItem(xpGemPrefab, LootItem.LootType.XP);

        if (Random.Range(0f, 100f) <= coinDropChance && coinPrefab != null)
            SpawnItem(coinPrefab, LootItem.LootType.Coin);
    }

    void SpawnItem(GameObject prefab, LootItem.LootType type)
    {
        float rx = Random.Range(-scatterRange, scatterRange);
        float rz = Random.Range(-scatterRange, scatterRange);
        Vector3 pos = transform.position + new Vector3(rx, 0.5f, rz);

        // Loot objesi instantiate ediliyor. İleride bunu da Pool yapabilirsin.
        GameObject loot = Instantiate(prefab, pos, Quaternion.identity);

        LootItem itemScript = loot.GetComponent<LootItem>();
        if (itemScript != null) itemScript.type = type;
    }
}