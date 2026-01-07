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
    private Material[] originalMaterials; // Orijinal materyalleri saklayacağız
    private Material whiteFlashMaterial;  // Geçici beyaz materyal

    private Rigidbody rb;
    private Vector3 baseScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Renderları ve Orijinal Materyalleri Al
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }

        // --- BEYAZ PARLAMA MATERYALİNİ OLUŞTUR ---
        // Kodla geçici bir materyal yaratıyoruz (Shader ayarı yapmana gerek yok)
        // Eğer URP kullanıyorsan "Universal Render Pipeline/Unlit"
        // Standart kullanıyorsan "Unlit/Color" veya "Mobile/Unlit (Supports Lightmap)"
        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit"); // URP Fallback

        whiteFlashMaterial = new Material(shader);
        whiteFlashMaterial.color = Color.white; // Bembeyaz olsun
    }

    void OnEnable()
    {
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

        // Doğarken renkleri sıfırla (Pool'dan kirlilik kalmasın)
        ResetMaterials();
    }

    public bool TakeDamage(float amount)
    {
        float finalDamage = amount - armor;
        if (finalDamage < 1) finalDamage = 1;

        currentHealth -= finalDamage;

        // --- EFEKTLER ---
        StartCoroutine(FlashRoutine()); // Flash Başlat

        if (rb != null)
        {
            rb.AddForce(-transform.forward * knockbackGucu, ForceMode.Impulse);
        }

        StartCoroutine(ScalePunchRoutine());
        // ----------------

        if (currentHealth <= 0)
        {
            OnEnemySliced();
            return true;
        }
        return false;
    }

    // --- DÜZELTİLEN KISIM BURASI ---
    IEnumerator FlashRoutine()
    {
        // 1. Tüm parçalara Beyaz Materyali giydir
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].material = whiteFlashMaterial;
        }

        // 2. Bekle
        yield return new WaitForSeconds(flashSuresi);

        // 3. Orijinal kıyafetlerini geri giydir
        ResetMaterials();
    }

    void ResetMaterials()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalMaterials.Length)
                renderers[i].material = originalMaterials[i];
        }
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

    public void OnEnemySliced()
    {
        DropLoot();
        if (GameManager.Instance != null) GameManager.Instance.AddKill();
        EnemyPool.Instance.ReturnToPool(this.gameObject);
    }
    // Kesilme anında rengi zorla düzeltmek için bunu çağıracağız
    public void ResetMaterialsImmediately()
    {
        StopAllCoroutines(); // Flash sayacını durdur
        ResetMaterials();    // Rengi hemen orijinale çevir
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

        GameObject loot = Instantiate(prefab, pos, Quaternion.identity);

        LootItem itemScript = loot.GetComponent<LootItem>();
        if (itemScript != null) itemScript.type = type;
    }
}