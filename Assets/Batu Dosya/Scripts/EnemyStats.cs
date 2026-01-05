using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("--- DROP PREFABLARI ---")]
    public GameObject xpGemPrefab;
    public GameObject coinPrefab;

    [Header("--- DÜÞME ORANLARI (%) ---")]
    public float xpDropChance = 100f;
    public float coinDropChance = 20f;

    [Header("--- TEMEL STATLAR ---")]
    public float maxHealth = 10f;
    private float currentHealth;

    [Header("--- ELITE & ARMOR AYARLARI ---")] // [YENÝ]
    public bool isElite = false; // Tiklersen dev olur
    public float armor = 0f;     // Gelen hasarý azaltýr (Örn: 5 hasar gelirse 2 zýrh düþer, 3 yer)
    public float eliteScaleMultiplier = 2.0f; // Elite olunca kaç kat büyüsün?
    public float eliteHealthMultiplier = 5.0f; // Elite olunca caný kaç kat artsýn?

    public float scatterRange = 1.0f;

    void OnEnable()
    {
        // [YENÝ] Elite Kontrolü
        if (isElite)
        {
            transform.localScale = Vector3.one * eliteScaleMultiplier; // Büyüt
            currentHealth = maxHealth * eliteHealthMultiplier;         // Caný katla
        }
        else
        {
            transform.localScale = Vector3.one; // Normale döndür (Pool'dan kirlilik kalmasýn)
            currentHealth = maxHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        // [YENÝ] Zýrh Hesabý
        float finalDamage = amount - armor;
        if (finalDamage < 1) finalDamage = 1; // En az 1 hasar yesin, ölümsüz olmasýn

        currentHealth -= finalDamage;

        // Vuruþ efekti, ses vb. buraya eklenebilir

        if (currentHealth <= 0)
        {
            OnEnemySliced();
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
        // Elite düþmanlar belki daha fazla loot atar? Þimdilik standart býrakýyorum.
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