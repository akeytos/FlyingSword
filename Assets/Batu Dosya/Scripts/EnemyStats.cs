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

    [Header("--- HASAR HİSSİYATI (JUICE) ---")] // [YENİ BÖLÜM] ✨
    public float knockbackGucu = 15f; // Geri tepme gücü
    public float flashSuresi = 0.1f;  // Beyaz kalma süresi

    public float scatterRange = 1.0f;

    // İç Referanslar
    private Renderer[] renderers; // Tüm parçaların renklerini değiştirmek için
    private Color[] originalColors; // Orijinal renkleri hafızada tutmak için
    private Rigidbody rb;
    private Vector3 baseScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Düşmanın üzerindeki ve altındaki tüm boyanabilir parçaları bul
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                originalColors[i] = renderers[i].material.color;
        }
    }

    void OnEnable()
    {
        // Elite Kontrolü
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

        transform.localScale = baseScale; // Boyutu ayarla

        // Renkleri sıfırla (Pool'dan kirlilik kalmasın)
        ResetColors();
    }

    // Hasar alma fonksiyonu
    public bool TakeDamage(float amount)
    {
        float finalDamage = amount - armor;
        if (finalDamage < 1) finalDamage = 1;

        currentHealth -= finalDamage;

        // --- HİSSİYAT EFEKTLERİ BAŞLIYOR --- ✨

        // 1. FLASH (Beyaz Parlama)
        StartCoroutine(FlashRoutine());

        // 2. KNOCKBACK (Geri Tepme)
        if (rb != null)
        {
            // Düşmanın baktığı yönün tersine (arkaya) kuvvet uygula
            rb.AddForce(-transform.forward * knockbackGucu, ForceMode.Impulse);
        }

        // 3. SCALE PUNCH (Anlık Şişme/Titreme)
        StartCoroutine(ScalePunchRoutine());

        // -----------------------------------

        if (currentHealth <= 0)
        {
            OnEnemySliced();
            return true; // ÖLDÜ
        }
        else
        {
            return false; // ÖLMEDİ (DEVAM)
        }
    }

    IEnumerator FlashRoutine()
    {
        // Hepsini Beyaz Yap
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = Color.white;
        }

        yield return new WaitForSeconds(flashSuresi);

        // Orijinal Renklerine Döndür
        ResetColors();
    }

    IEnumerator ScalePunchRoutine()
    {
        // Hafifçe şişir (Örn: %20 büyüt)
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

    void ResetColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalColors.Length)
                renderers[i].material.color = originalColors[i];
        }
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
        float rx = Random.Range(-scatterRange, scatterRange);
        float rz = Random.Range(-scatterRange, scatterRange);
        Vector3 pos = transform.position + new Vector3(rx, 0.5f, rz);

        GameObject loot = Instantiate(prefab, pos, Quaternion.identity);

        LootItem itemScript = loot.GetComponent<LootItem>();
        if (itemScript != null) itemScript.type = type;
    }
}