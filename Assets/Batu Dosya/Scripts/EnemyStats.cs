using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("--- DROP PREFABLARI ---")]
    public GameObject xpGemPrefab;
    public GameObject coinPrefab;

    [Header("--- DÜÞME ORANLARI (%) ---")]
    public float xpDropChance = 100f;
    public float coinDropChance = 20f;

    [Header("--- DEÐERLER ---")]
    public float maxHealth = 10f; // Can eklendi
    private float currentHealth;

    public float scatterRange = 1.0f;

    // Her doðuþta caný yenile
    void OnEnable()
    {
        currentHealth = maxHealth;
    }

    // Hasar alma fonksiyonu (Kýlýç buraya vuracak)
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            OnEnemySliced();
        }
    }

    public void OnEnemySliced()
    {
        // 1. LOOT DÜÞÜR
        DropLoot();

        // 2. KILL SAYACINI ARTIR
        if (GameManager.Instance != null) GameManager.Instance.AddKill(); // Eðer manager varsa

        // 3. HAVUZA GERÝ DÖN (DESTROY YOK!)
        EnemyPool.Instance.ReturnToPool(this.gameObject);
    }

    void DropLoot()
    {
        // XP
        if (Random.Range(0f, 100f) <= xpDropChance && xpGemPrefab != null)
            SpawnItem(xpGemPrefab, LootItem.LootType.XP);

        // COIN
        if (Random.Range(0f, 100f) <= coinDropChance && coinPrefab != null)
            SpawnItem(coinPrefab, LootItem.LootType.Coin);
    }

    void SpawnItem(GameObject prefab, LootItem.LootType type)
    {
        float rx = Random.Range(-scatterRange, scatterRange);
        float rz = Random.Range(-scatterRange, scatterRange);
        Vector3 pos = transform.position + new Vector3(rx, 0.5f, rz);

        GameObject loot = Instantiate(prefab, pos, Quaternion.identity);

        // LootItem ayarlarý (Sendeki scriptle uyumlu)
        LootItem itemScript = loot.GetComponent<LootItem>();
        if (itemScript != null) itemScript.type = type;
    }
}