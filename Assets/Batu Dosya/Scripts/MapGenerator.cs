using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Veri Kaynaðý")]
    [SerializeField] private LevelSettings settings;

    [Header("Referanslar")]
    [SerializeField] private Transform player;
    [SerializeField] private PropSpawner propSpawner;
    [SerializeField] private EnemySpawner enemySpawner;

    // Koordinat Takibi (Vector2 yerine Vector2Int kullanýyoruz, daha performanslý ve net)
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int currentGridCoord;
    private Vector2Int lastGridCoord;

    // Hata önleyici kontrol
    private bool isInitialized = false;

    private void Start()
    {
        // Defansif Kodlama: Kritik referanslar eksikse oyunu baþlatma
        if (settings == null || player == null || ObjectPooler.Instance == null)
        {
            Debug.LogError("CRITICAL ERROR: MapGenerator eksik referanslar nedeniyle baþlatýlamadý!");
            this.enabled = false;
            return;
        }

        // Baþlangýçta oyuncunun konumunu hesapla ve ilk güncellemeyi zorla
        UpdatePlayerGridPosition();
        UpdateVisibleChunks();

        lastGridCoord = currentGridCoord;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        UpdatePlayerGridPosition();

        // OPTÝMÝZASYON: Sadece oyuncu yeni bir kareye (Chunk'a) geçtiyse hesaplama yap.
        // Her karede (frame) binlerce döngü çalýþtýrmaktan kurtarýr.
        if (currentGridCoord != lastGridCoord)
        {
            UpdateVisibleChunks();
            lastGridCoord = currentGridCoord;
        }
    }

    private void UpdatePlayerGridPosition()
    {
        int x = Mathf.RoundToInt(player.position.x / settings.chunkSize);
        int z = Mathf.RoundToInt(player.position.z / settings.chunkSize);
        currentGridCoord = new Vector2Int(x, z);
    }

    private void UpdateVisibleChunks()
    {
        // 1. SPAWN DÖNGÜSÜ (Asimetrik)
        for (int z = -settings.backwardDistance; z <= settings.forwardDistance; z++)
        {
            for (int x = -settings.sideDistance; x <= settings.sideDistance; x++)
            {
                Vector2Int coord = new Vector2Int(currentGridCoord.x + x, currentGridCoord.y + z);

                if (!activeChunks.ContainsKey(coord))
                {
                    SpawnChunk(coord);
                }
            }
        }

        // 2. TEMÝZLÝK DÖNGÜSÜ (Buffer'lý)
        CleanupChunks();
    }

    private void SpawnChunk(Vector2Int coord)
    {
        Vector3 spawnPosition = new Vector3(
            coord.x * settings.chunkSize,
            0,
            coord.y * settings.chunkSize
        );

        GameObject newChunk = ObjectPooler.Instance.SpawnFromPool(settings.groundTag, spawnPosition, Quaternion.identity);

        if (newChunk == null)
        {
            // Bu hata çok kritiktir, developer'ý uyar.
            Debug.LogWarning($"POOL EMPTY: '{settings.groundTag}' havuzunda obje kalmadý! Pool Size'ý artýrýn.");
            return;
        }

        // Chunk'ý listeye kaydet
        activeChunks.Add(coord, newChunk);

        // Yöneticileri Tetikle (Null Check ile güvenli hale getirdik)
        propSpawner?.SpawnProps(newChunk, settings.chunkSize);
        enemySpawner?.SpawnEnemies(newChunk, settings.chunkSize);
    }

    private void CleanupChunks()
    {
        // Silinecekleri geçici listede tut
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (KeyValuePair<Vector2Int, GameObject> item in activeChunks)
        {
            Vector2Int chunkCoord = item.Key;

            // MESAFE HESABI (Manhattan Distance yerine Eksen Bazlý Kontrol)

            // "Buffer" (Tampon) ekliyoruz. Spawn mesafesinden X birim daha uzakta silinir.
            // Bu sayede sýnýrda ileri geri gidersen objeler titremez.
            int buffer = settings.despawnBuffer;

            bool tooFarRightLeft = Mathf.Abs(currentGridCoord.x - chunkCoord.x) > (settings.sideDistance + buffer);
            bool tooFarBehind = chunkCoord.y < (currentGridCoord.y - settings.backwardDistance - buffer);
            bool tooFarAhead = chunkCoord.y > (currentGridCoord.y + settings.forwardDistance + buffer);

            if (tooFarRightLeft || tooFarBehind || tooFarAhead)
            {
                chunksToRemove.Add(chunkCoord);
            }
        }

        // Güvenli silme iþlemi
        foreach (Vector2Int coord in chunksToRemove)
        {
            GameObject chunkObj = activeChunks[coord];

            // Önce üzerindekileri temizle
            propSpawner?.RecycleProps(chunkObj);
            enemySpawner?.RecycleEnemies(chunkObj);

            // Sonra zemini havuza yolla
            ObjectPooler.Instance.ReturnToPool(chunkObj);

            // Listeden çýkar
            activeChunks.Remove(coord);
        }
    }

    private void OnDrawGizmos()
    {
        if (settings == null || !settings.showGizmos) return;

        Gizmos.color = settings.gizmoColor;

        Vector3 center = player != null ? player.position : transform.position;

        // Grid'e snap'le (Yapýþ)
        float snapX = Mathf.Round(center.x / settings.chunkSize) * settings.chunkSize;
        float snapZ = Mathf.Round(center.z / settings.chunkSize) * settings.chunkSize;

        // Spawn Alanýný Çiz (Yeþil)
        float width = (settings.sideDistance * 2 + 1) * settings.chunkSize;
        float depth = (settings.forwardDistance + settings.backwardDistance + 1) * settings.chunkSize;
        float zOffset = (settings.forwardDistance - settings.backwardDistance) * settings.chunkSize * 0.5f;

        Vector3 drawCenter = new Vector3(snapX, 0, snapZ + zOffset);
        Gizmos.DrawWireCube(drawCenter, new Vector3(width, 1f, depth));

        // Despawn Sýnýrýný Çiz (Kýrmýzý - Buffer Alaný)
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        float bufferWidth = width + (settings.despawnBuffer * 2 * settings.chunkSize);
        float bufferDepth = depth + (settings.despawnBuffer * 2 * settings.chunkSize);
        Gizmos.DrawWireCube(drawCenter, new Vector3(bufferWidth, 1f, bufferDepth));
    }
}