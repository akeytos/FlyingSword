using UnityEngine;

public class EnemySpawnerLast : MonoBehaviour
{
    [Header("Gerekli Objeler")]
    public GameObject enemyPrefab; // Düþman prefabýný buraya sürükle
    public Transform player;       // PlayerRoot (Senin karakterin) buraya

    [Header("Spawn Ayarlarý")]
    public float onMesafe = 20f;    // Kaç metre ileride doðsun
    public float yolGenisligi = 5f; // Saða sola daðýlma
    public float spawnSikligi = 1.5f; // Saniye

    private float zamanSayaci;

    void Update()
    {
        // Eðer oyuncu veya prefab yoksa hata verme, bekle
        if (player == null || enemyPrefab == null) return;

        zamanSayaci -= Time.deltaTime;

        if (zamanSayaci <= 0)
        {
            SpawnEnemy();
            zamanSayaci = spawnSikligi;
        }
    }

    void SpawnEnemy()
    {
        // 1. Pozisyon Hesapla
        // Oyuncunun baktýðý yönün (forward) 'onMesafe' kadar ilerisi
        Vector3 spawnPos = player.position + (player.forward * onMesafe);

        // Saða sola rastgele daðýt
        spawnPos += player.right * Random.Range(-yolGenisligi, yolGenisligi);

        // Yükseklik oyuncuyla ayný olsun
        spawnPos.y = player.position.y;

        // 2. Yarat
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // 3. Hedefi Göster (ARTIK AILast SCRIPTINI ARIYORUZ)
        if (newEnemy.GetComponent<AILast>())
        {
            newEnemy.GetComponent<AILast>().target = player;
        }
    }
}