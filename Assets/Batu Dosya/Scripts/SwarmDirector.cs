using System.Collections.Generic;
using UnityEngine;

public class WaveDirector : MonoBehaviour
{
    [Header("--- TEMEL AYARLAR ---")]
    public Transform player;          // PlayerRoot / Pilot
    public LayerMask groundLayer;     // "Ground" layerını seçmen lazım!
    public float spawnRadius = 25f;   // Oyuncudan ne kadar uzakta doğsun?

    [Header("--- DALGA SENARYOSU ---")]
    public List<WavePhase> waves;

    // Boss kontrolü için
    private bool bossSpawned = false;
    private float gameTime;
    private float spawnTimer;

    [System.Serializable]
    public class WavePhase
    {
        public string phaseName;     // Örn: "Isınma", "1. Dakika Swarm", "BOSS"
        public float startTime;      // Bu dalga oyunun kaçıncı saniyesinde başlasın?
        public float spawnRate;      // Kaç saniyede bir düşman gelsin? (0.1 = Swarm, 2.0 = Sakin)

        [Header("Kimler Gelecek?")]
        public List<EnemyWeight> enemyPool; // Bu dalgada hangi düşmanlar var?

        [Header("Boss Dalgası mı?")]
        public GameObject bossPrefab; // Eğer buraya bir şey koyarsan, bu saniyede Boss doğar!
    }

    [System.Serializable]
    public class EnemyWeight
    {
        public string enemyID; // Pool ID (Goblin, Goril)
        [Range(0, 100)] public int chance;
    }

    void Update()
    {
        if (player == null) return;

        gameTime += Time.deltaTime;

        // 1. ŞU AN HANGİ DALGADAYIZ?
        WavePhase currentWave = GetCurrentWave();

        if (currentWave != null)
        {
            // --- BOSS KONTROLÜ ---
            if (currentWave.bossPrefab != null && !bossSpawned)
            {
                SpawnBoss(currentWave.bossPrefab);
                bossSpawned = true; // Sadece 1 kere doğsun
                // Boss gelince diğer düşmanlar dursun istersen buraya return koyabilirsin.
            }
            // Boss dalgasına geçilmediyse flag'i sıfırla (farklı bosslar için)
            else if (currentWave.bossPrefab == null)
            {
                bossSpawned = false;
            }

            // --- NORMAL DÜŞMAN SPAWN ---
            // Eğer spawnRate 999 gibi yüksek bir sayıysa düşman doğmaz (Sadece Boss anı için)
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
        // Listeyi sondan başa tarar, zamanı gelen en son dalgayı bulur.
        for (int i = waves.Count - 1; i >= 0; i--)
        {
            if (gameTime >= waves[i].startTime)
            {
                return waves[i];
            }
        }
        return null;
    }

    void SpawnEnemy(WavePhase wave)
    {
        // Tür Seç
        string enemyID = GetRandomEnemyID(wave);
        if (string.IsNullOrEmpty(enemyID)) return;

        // Pozisyon Bul (YER ALTI SORUNU ÇÖZÜMÜ)
        Vector3 spawnPos = GetValidSpawnPosition();

        // Havuzdan Çek
        GameObject enemy = EnemyPool.Instance.SpawnFromPool(enemyID, spawnPos, Quaternion.identity);

        // Hedef Göster
        if (enemy != null && enemy.TryGetComponent(out AILast ai))
        {
            ai.target = player;
        }
    }

    void SpawnBoss(GameObject bossPrefab)
    {
        // Boss için biraz daha uzağa pozisyon bul
        Vector3 bossPos = GetValidSpawnPosition();

        Instantiate(bossPrefab, bossPos, Quaternion.identity);
        Debug.Log("👹 BOSS SAHNEYE İNDİ!");

        // Müzik değişimi vs. buraya eklenebilir.
    }

    // --- YER ALTI SORUNUNU ÇÖZEN FONKSİYON ---
    Vector3 GetValidSpawnPosition()
    {
        // 1. Rastgele X, Z bul
        Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 airPos = player.position + new Vector3(randomCircle.x, 20f, randomCircle.y); // 20 metre havadan başla

        // 2. Aşağı doğru ışın (Raycast) at
        RaycastHit hit;
        if (Physics.Raycast(airPos, Vector3.down, out hit, 50f, groundLayer))
        {
            // Zemini bulduk! Tam üstüne koyuyoruz.
            return hit.point;
        }

        // Zemin bulunamazsa (harita dışıysa) güvenli olarak oyuncunun Y seviyesini kullan
        Vector3 fallbackPos = airPos;
        fallbackPos.y = player.position.y;
        return fallbackPos;
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