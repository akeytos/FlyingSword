using UnityEngine;

public class EnemySpawnerr : MonoBehaviour
{
    [Header("Gerekli Objeler")]
    [SerializeField] private GameObject enemyPrefab; // Kesilecek dusman/obje prefab
    [SerializeField] private Transform player;       // PlayerRoot (kilic) buraya suruklenecek

    [Header("Spawn Ayarlari")]
    [Min(0f)] [SerializeField] private float onMesafe = 30f;     // Bizden kac metre ileride ciksin?
    [Min(0f)] [SerializeField] private float yolGenisligi = 4f;  // Yolun ne kadar sagina/soluna dagilsin?
    [Min(0.01f)] [SerializeField] private float spawnSikligi = 1.0f; // Kac saniyede bir dusman ciksin?

    private float zamanSayaci;

    private void OnEnable()
    {
        // Oyun baslarken ilk spawn gecikmesiz olsun
        zamanSayaci = 0f;
    }

    void Update()
    {
        if (player == null || enemyPrefab == null) return;

        // Zamanlayici
        zamanSayaci -= Time.deltaTime;

        if (zamanSayaci <= 0f)
        {
            SpawnEnemy();
            zamanSayaci = Mathf.Max(0.01f, spawnSikligi); // Sayaci sifirla
        }
    }

    private void SpawnEnemy()
    {
        // 1. MERKEZ NOKTA BULMA
        // Kilicin (Player) tam "onMesafe" kadar onundeki noktayi buluyoruz.
        // player.forward kullandigimiz icin kilic nereye donerse orasi "onu" olur.
        Vector3 merkezNokta = player.position + (player.forward * onMesafe);

        // 2. RASTGELE SAG/SOL SAPMA
        // Kilicin sagina/soluna (player.right) rastgele bir mesafe ekliyoruz.
        float rastgeleX = Random.Range(-yolGenisligi, yolGenisligi);
        Vector3 dogumYeri = merkezNokta + (player.right * rastgeleX);

        // 3. YUKSEKLIK AYARI
        // Dusmanlar kilicla ayni hizada mi olsun, yerde mi?
        // Eger havada ucan dusmanlarsa:
        dogumYeri.y = player.position.y;
        // Eger yerde duran varilllerse (ornegin: dogumYeri.y = 0.5f) yapabilirsin.

        // 4. OLUSTUR
        // Rotasyonu rastgele yapabilirsin (Quaternion.Euler(0, Random.Range(0,360), 0))
        // Simdilik Identity (Duz) birakiyorum.
        Instantiate(enemyPrefab, dogumYeri, Quaternion.identity);
    }

    private void OnValidate()
    {
        // Inspector degeri sifirin altina inmesin
        spawnSikligi = Mathf.Max(0.01f, spawnSikligi);
        onMesafe = Mathf.Max(0f, onMesafe);
        yolGenisligi = Mathf.Max(0f, yolGenisligi);
    }
}