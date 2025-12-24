using UnityEngine;

public class EnemySpawnerr : MonoBehaviour
{
    [Header("Gerekli Objeler")]
    public GameObject enemyPrefab; // Kesilecek düþman/obje prefabý
    public Transform player;       // PlayerRoot (Kýlýç) buraya sürüklenecek

    [Header("Spawn Ayarlarý")]
    public float onMesafe = 30f;   // Bizden kaç metre ilerde çýksýn? (Hýzýna göre artýr)
    public float yolGenisligi = 4f; // Yolun ne kadar saðýna/soluna daðýlsýn?
    public float spawnSikligi = 1.0f; // Kaç saniyede bir düþman çýksýn?

    private float zamanSayaci;

    void Update()
    {
        if (player == null) return;

        // Zamanlayýcý
        zamanSayaci -= Time.deltaTime;

        if (zamanSayaci <= 0)
        {
            SpawnEnemy();
            zamanSayaci = spawnSikligi; // Sayacý sýfýrla
        }
    }

    void SpawnEnemy()
    {
        // 1. MERKEZ NOKTA BULMA
        // Kýlýcýn (Player) tam "onMesafe" kadar önündeki noktayý buluyoruz.
        // player.forward kullandýðýmýz için kýlýç nereye dönerse orasý "önü" olur.
        Vector3 merkezNokta = player.position + (player.forward * onMesafe);

        // 2. RASTGELE SAÐ/SOL SAPMA
        // Kýlýcýn saðýna/soluna (player.right) rastgele bir mesafe ekliyoruz.
        float rastgeleX = Random.Range(-yolGenisligi, yolGenisligi);
        Vector3 dogumYeri = merkezNokta + (player.right * rastgeleX);

        // 3. YÜKSEKLÝK AYARI
        // Düþmanlar kýlýçla ayný hizada mý olsun, yerde mi? 
        // Eðer havada uçan düþmanlarsa:
        dogumYeri.y = player.position.y;
        // Eðer yerde duran varilllerse (Örn: dogumYeri.y = 0.5f;) yapabilirsin.

        // 4. OLUÞTUR
        // Rotasyonu rastgele yapabilirsin (Quaternion.Euler(0, Random.Range(0,360), 0))
        // Þimdilik Identity (Düz) býrakýyorum.
        Instantiate(enemyPrefab, dogumYeri, Quaternion.identity);
    }
}//313131