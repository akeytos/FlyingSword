using System.Collections.Generic;
using UnityEngine;

public class WaveDirector : MonoBehaviour
{
    [Header("--- TEMEL AYARLAR ---")]
    public Transform player;
    public LayerMask groundLayer;
    public float spawnRadius = 25f;

    [Header("--- DALGA SENARYOSU ---")]
    public List<WavePhase> waves;

    // Durum Kontrol Değişkenleri
    private bool bossSpawned = false;
    private bool eliteSpawned = false; // [YENİ] Elite doğdu mu?
    private float gameTime;
    private float spawnTimer;
    private float currentWaveTimer; // [YENİ] Şu anki dalganın yerel saati

    private WavePhase lastWave = null;

    [System.Serializable]
    public class WavePhase
    {
        public string phaseName;
        public float startTime;
        public float spawnRate;
        public List<EnemyWeight> enemyPool;

        [Header("--- BOSS AYARLARI ---")]
        public GameObject bossPrefab;

        [Header("--- ELITE (MINI-BOSS) AYARLARI ---")]
        public GameObject elitePrefab; // [YENİ] Elite düşman prefabı
        public float eliteSpawnDelay = 10f; // [YENİ] Dalga başladıktan kaç sn sonra gelsin?

        [Header("--- SÜRÜ AYARI ---")]
        public bool isSwarmWave = false;
    }

    [System.Serializable]
    public class EnemyWeight
    {
        public string enemyID;
        [Range(0, 100)] public int chance;
    }

    void Update()
    {
        if (player == null) return;
        gameTime += Time.deltaTime;

        WavePhase currentWave = GetCurrentWave();

        // --- DALGA DEĞİŞİM KONTROLÜ ---
        if (currentWave != null && currentWave != lastWave)
        {
            // Yeni dalgaya geçtik!

            // Sürü uyarısı
            if (currentWave.isSwarmWave && SwarmAlertManager.Instance != null)
            {
                SwarmAlertManager.Instance.ShowSwarmWarning();
            }

            // Durumları Sıfırla
            bossSpawned = false;
            eliteSpawned = false; // [YENİ] Yeni dalga için elite sıfırlandı
            currentWaveTimer = 0f; // [YENİ] Dalga süresini sıfırla

            // Kaydı güncelle
            lastWave = currentWave;
        }

        if (currentWave != null)
        {
            // Dalga süresini ilerlet
            currentWaveTimer += Time.deltaTime;

            // --- 1. BOSS DOĞUMU ---
            if (currentWave.bossPrefab != null && !bossSpawned)
            {
                SpawnUnit(currentWave.bossPrefab, true); // Boss hemen doğar
                bossSpawned = true;
            }

            // --- 2. ELITE DOĞUMU [YENİ] ---
            // Elite prefabı varsa, daha doğmadıysa ve belirlediğimiz süre geçtiyse
            if (currentWave.elitePrefab != null && !eliteSpawned && currentWaveTimer >= currentWave.eliteSpawnDelay)
            {
                SpawnUnit(currentWave.elitePrefab, false); // Elite doğur
                eliteSpawned = true;
                Debug.Log($"⚔️ ELITE DÜŞMAN SAHNEYE GİRDİ: {currentWave.phaseName}");
            }

            // --- 3. NORMAL DÜŞMAN DOĞUMU ---
            if (currentWave.spawnRate < 100f)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= currentWave.spawnRate)
                {
                    SpawnEnemyFromPool(currentWave);
                    spawnTimer = 0;
                }
            }
        }
    }

    WavePhase GetCurrentWave()
    {
        for (int i = waves.Count - 1; i >= 0; i--)
        {
            if (gameTime >= waves[i].startTime) return waves[i];
        }
        return null;
    }

    // Hem Boss hem Elite için ortak spawn fonksiyonu (Kod tekrarını önlemek için birleştirdim)
    void SpawnUnit(GameObject prefab, bool isBoss)
    {
        Vector3 spawnPos = GetValidSpawnPosition();
        GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Eğer Elite ise belki biraz daha büyük yapabilirsin veya efekt ekleyebilirsin
        if (!isBoss)
        {
            // Örneğin Elite'ler %20 daha büyük olsun (İsteğe bağlı)
            unit.transform.localScale *= 1.2f;
        }
    }

    void SpawnEnemyFromPool(WavePhase wave)
    {
        string enemyID = GetRandomEnemyID(wave);
        if (string.IsNullOrEmpty(enemyID)) return;

        Vector3 spawnPos = GetValidSpawnPosition();
        GameObject enemy = EnemyPool.Instance.SpawnFromPool(enemyID, spawnPos, Quaternion.identity);

        if (enemy != null && enemy.TryGetComponent(out AILast ai))
        {
            ai.target = player;
            if (ai.isFlying)
            {
                enemy.transform.position += Vector3.up * Random.Range(5f, 15f);
            }
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 skyPos = new Vector3(player.position.x + randomCircle.x, 100f, player.position.z + randomCircle.y);

        RaycastHit hit;
        if (Physics.Raycast(skyPos, Vector3.down, out hit, 200f, groundLayer))
        {
            return hit.point;
        }

        Vector3 defaultPos = skyPos;
        defaultPos.y = 0f;
        return defaultPos;
    }

    string GetRandomEnemyID(WavePhase wave)
    {
        if (wave.enemyPool.Count == 0) return null;
        int totalWeight = 0;
        foreach (var e in wave.enemyPool) totalWeight += e.chance;

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var e in wave.enemyPool)
        {
            currentWeight += e.chance;
            if (randomValue < currentWeight) return e.enemyID;
        }
        return wave.enemyPool[0].enemyID;
    }
}