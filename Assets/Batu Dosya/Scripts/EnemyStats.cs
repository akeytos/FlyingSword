using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Listeler için gerekli

public class EnemyStats : MonoBehaviour
{
    [Header("--- DROP PREFABLARI ---")]
    public GameObject xpGemPrefab;
    public GameObject coinPrefab;
    public float xpDropChance = 100f;
    public float coinDropChance = 20f;

    [Header("--- TEMEL STATLAR ---")]
    public float maxHealth = 10f;
    private float currentHealth;

    [Header("--- ELITE & ARMOR ---")]
    public bool isElite = false;
    public float armor = 0f;
    public float eliteScaleMultiplier = 2.0f;
    public float eliteHealthMultiplier = 5.0f;

    [Header("--- EFEKTLER ---")]
    public float knockbackGucu = 15f;
    public float flashSuresi = 0.1f;
    public Material whiteFlashMaterial; // Inspector'dan atadığın beyaz materyal

    // --- YENİ YAPI: Renderer ve Orijinal Materyal Listesi ---
    // Her renderer'ın kendi orijinal materyal dizesini saklayacağız
    private Dictionary<Renderer, Material[]> originalMaterialsDict = new Dictionary<Renderer, Material[]>();
    private Renderer[] allRenderers;

    private Rigidbody rb;
    private Vector3 baseScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Tüm parçaları bul
        allRenderers = GetComponentsInChildren<Renderer>();

        // Her parçanın orijinal materyallerini (Çoğul!) hafızaya al
        foreach (Renderer rend in allRenderers)
        {
            if (rend != null)
            {
                // .sharedMaterials kullanarak orijinal referansları alıyoruz
                originalMaterialsDict[rend] = rend.sharedMaterials;
            }
        }
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

        // Doğduğunda tertemiz giyinsin
        ResetMaterialsImmediately();
    }

    public bool TakeDamage(float amount)
    {
        float finalDamage = amount - armor;
        if (finalDamage < 1) finalDamage = 1;

        currentHealth -= finalDamage;

        // --- FLASH (HEPSİNİ BEYAZ YAP) ---
        if (whiteFlashMaterial != null)
        {
            ApplyFlashMaterial();
        }

        if (currentHealth <= 0)
        {
            if (rb != null) rb.AddForce(-transform.forward * knockbackGucu, ForceMode.Impulse);
            return true; // ÖLDÜ
        }
        else
        {
            StartCoroutine(FlashRoutine());
            if (rb != null) rb.AddForce(-transform.forward * knockbackGucu, ForceMode.Impulse);
            StartCoroutine(ScalePunchRoutine());
            return false; // ÖLMEDİ
        }
    }

    // --- YENİ: TÜM SLOTLARI BOYAYAN FONKSİYON ---
    void ApplyFlashMaterial()
    {
        foreach (Renderer rend in allRenderers)
        {
            if (rend == null) continue;

            // Orijinalinde kaç tane materyal varsa (örn: 3 tane),
            // o kadar sayıda beyaz materyal dizisi oluşturuyoruz.
            int matCount = originalMaterialsDict[rend].Length;
            Material[] flashMats = new Material[matCount];

            for (int i = 0; i < matCount; i++)
            {
                flashMats[i] = whiteFlashMaterial; // Hepsini beyaza boya
            }

            rend.materials = flashMats; // Yeni beyaz seti giydir
        }
    }

    IEnumerator FlashRoutine()
    {
        yield return new WaitForSeconds(flashSuresi);
        ResetMaterials();
    }

    // --- YENİ: ESKİ HALİNE DÖNDÜRME ---
    void ResetMaterials()
    {
        foreach (Renderer rend in allRenderers)
        {
            if (rend != null && originalMaterialsDict.ContainsKey(rend))
            {
                // Hafızadaki orijinal seti geri yükle
                rend.materials = originalMaterialsDict[rend];
            }
        }
    }

    public void ResetMaterialsImmediately()
    {
        StopAllCoroutines();
        ResetMaterials();
    }

    // ... (ScalePunch, OnEnemySliced, DropLoot aynı kalacak) ...
    // Aşağısı önceki kodun aynısı:

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

    void DropLoot()
    {
        if (Random.Range(0f, 100f) <= xpDropChance && xpGemPrefab != null)
            SpawnItem(xpGemPrefab, LootItem.LootType.XP);

        if (Random.Range(0f, 100f) <= coinDropChance && coinPrefab != null)
            SpawnItem(coinPrefab, LootItem.LootType.Coin);
    }

    void SpawnItem(GameObject prefab, LootItem.LootType type)
    {
        float rx = Random.Range(-1f, 1f);
        float rz = Random.Range(-1f, 1f);
        Vector3 pos = transform.position + new Vector3(rx, 0.5f, rz);
        GameObject loot = Instantiate(prefab, pos, Quaternion.identity);
        LootItem itemScript = loot.GetComponent<LootItem>();
        if (itemScript != null) itemScript.type = type;
    }
}