using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("--- DROP PREFABLARI (Sürükle) ---")]
    public GameObject xpGemPrefab; // XP Kristal Prefabý
    public GameObject coinPrefab;  // Altýn Prefabý

    [Header("--- DÜÞME ORANLARI (%) ---")]
    // Senin istediðin gibi: XP her zaman (%100) düþecek.
    [Range(0, 100)] public float xpDropChance = 100f;

    // Senin istediðin gibi: Altýn %20 þansla düþecek.
    [Range(0, 100)] public float coinDropChance = 20f;

    [Header("--- DEÐERLER ---")]
    public float xpAmount = 20f; // Taþ baþýna gelen XP
    public int coinAmount = 10;  // Para baþýna gelen miktar

    // Kýlýç düþmaný kestiðinde bu fonksiyon çalýþacak
    public void OnEnemySliced()
    {
        // 1. XP HESABI (%100 olduðu için hep düþecek)
        float zarXP = Random.Range(0f, 100f);
        if (zarXP <= xpDropChance && xpGemPrefab != null)
        {
            SpawnLoot(xpGemPrefab, LootItem.LootType.XP, xpAmount);
        }

        // 2. COIN HESABI (%20 þansla düþecek)
        float zarCoin = Random.Range(0f, 100f);
        if (zarCoin <= coinDropChance && coinPrefab != null)
        {
            SpawnLoot(coinPrefab, LootItem.LootType.Coin, coinAmount);
        }

        // Ýstatistik için Kill sayýsýný artýr
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
        }
    }

    void SpawnLoot(GameObject prefab, LootItem.LootType type, float value)
    {
        // Düþmanýn öldüðü yerden biraz yukarýda doðsun
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

        GameObject loot = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Doðan objenin özelliklerini ayarla
        LootItem itemScript = loot.GetComponent<LootItem>();
        if (itemScript != null)
        {
            itemScript.type = type;
            itemScript.amount = value;
        }
    }
}