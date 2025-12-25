using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("--- DROP PREFABLARI ---")]
    public GameObject xpGemPrefab;
    public GameObject coinPrefab;

    [Header("--- DÜÞME ORANLARI (%) ---")]
    [Range(0, 100)] public float xpDropChance = 100f;
    [Range(0, 100)] public float coinDropChance = 20f;

    [Header("--- DEÐERLER ---")]
    public float xpAmount = 20f;
    public int coinAmount = 10;

    [Header("--- SAÇILMA AYARI (YENÝ) ---")]
    public float scatterRange = 1.0f; // Eþyalar ne kadar uzaða saçýlsýn?

    public void OnEnemySliced()
    {
        // 1. XP HESABI
        float zarXP = Random.Range(0f, 100f);
        if (zarXP <= xpDropChance && xpGemPrefab != null)
        {
            SpawnLoot(xpGemPrefab, LootItem.LootType.XP, xpAmount);
        }

        // 2. COIN HESABI
        float zarCoin = Random.Range(0f, 100f);
        if (zarCoin <= coinDropChance && coinPrefab != null)
        {
            SpawnLoot(coinPrefab, LootItem.LootType.Coin, coinAmount);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
        }
    }

    void SpawnLoot(GameObject prefab, LootItem.LootType type, float value)
    {
        // --- ÝÞTE SÝHÝRLÝ DOKUNUÞ BURADA ---
        // Rastgele bir sapma deðeri oluþturuyoruz (X ve Z ekseninde)
        float randomX = Random.Range(-scatterRange, scatterRange);
        float randomZ = Random.Range(-scatterRange, scatterRange);

        // Düþmanýn pozisyonuna bu rastgeleliði ekliyoruz
        // Y ekseninde (Yükseklik) 0.5f yukarýda olsun ki yere gömülmesin
        Vector3 spawnPos = transform.position + new Vector3(randomX, 0.5f, randomZ);

        GameObject loot = Instantiate(prefab, spawnPos, Quaternion.identity);

        LootItem itemScript = loot.GetComponent<LootItem>();
        if (itemScript != null)
        {
            itemScript.type = type;
            itemScript.amount = value;
        }
    }
}