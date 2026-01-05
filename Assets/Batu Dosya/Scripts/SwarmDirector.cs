using System.Collections.Generic;
using UnityEngine;

public class WaveDirector : MonoBehaviour
{
    [Header("--- TEMEL AYARLAR ---")]
    public Transform player;
    public LayerMask groundLayer; // Inspector'dan 'Ground' layerını seçmeyi unutma!
    public float spawnRadius = 25f;

    [Header("--- DALGA SENARYOSU ---")]
    public List<WavePhase> waves;

    private bool bossSpawned = false;
    private float gameTime;
    private float spawnTimer;

    [System.Serializable]
    public class WavePhase
    {
        public string phaseName;
        public float startTime;
        public float spawnRate;
        public List<EnemyWeight> enemyPool;
        public GameObject bossPrefab;
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

        if (currentWave != null)
        {
            // --- BOSS DOĞUMU ---
            if (currentWave.bossPrefab != null && !bossSpawned)
            {
                SpawnBoss(currentWave.bossPrefab);
                bossSpawned = true;
            }
            else if (currentWave.bossPrefab == null)
            {
                bossSpawned = false;
            }

            // --- NORMAL DÜŞMAN DOĞUMU ---
            if (currentWave.spawnRate < 100f)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= currentWave.spawnRate)
                {
                    SpawnEnemy(currentWave);
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

    void SpawnEnemy(WavePhase wave)
    {
        string enemyID = GetRandomEnemyID(wave);
        if (string.IsNullOrEmpty(enemyID)) return;

        // 1. Önce zemini bul (Garantili Yöntem)
        Vector3 spawnPos = GetValidSpawnPosition();

        // 2. Havuzdan düşmanı çağır
        GameObject enemy = EnemyPool.Instance.SpawnFromPool(enemyID, spawnPos, Quaternion.identity);

        if (enemy != null && enemy.TryGetComponent(out AILast ai))
        {
            ai.target = player;

            // 3. Eğer UÇAN bir düşmansa, onu yerden kaldır
            if (ai.isFlying)
            {
                enemy.transform.position += Vector3.up * Random.Range(5f, 15f); // 5-15 metre havalandır
            }
            // Uçmuyorsa zaten GetValidSpawnPosition onu yere yapıştırdı.
        }
    }

    void SpawnBoss(GameObject bossPrefab)
    {
        Vector3 bossPos = GetValidSpawnPosition();
        Instantiate(bossPrefab, bossPos, Quaternion.identity);
    }

    // --- KRİTİK DÜZELTME BURADA ---
    Vector3 GetValidSpawnPosition()
    {
        // 1. Oyuncunun etrafında rastgele nokta seç
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;

        // 2. Noktayı ÇOK YUKARIDAN başlat (Oyuncunun yüksekliğinden bağımsız)
        // Y=100 diyerek bulutların üzerinden aşağı bakıyoruz
        Vector3 skyPos = new Vector3(player.position.x + randomCircle.x, 100f, player.position.z + randomCircle.y);

        RaycastHit hit;
        // 3. Aşağı doğru lazer at, sadece Ground layer'ını gör
        if (Physics.Raycast(skyPos, Vector3.down, out hit, 200f, groundLayer))
        {
            return hit.point; // Zemini bulduk, tam üstü
        }

        // 4. Eğer zemin yoksa (harita dışıysa), varsayılan olarak Y=0 (Dünya düzlemi) kullan
        // ASLA player.position.y kullanma, yoksa havada doğarlar!
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