using UnityEngine;

public class LootItem : MonoBehaviour
{
    public enum LootType { XP, Coin }

    [Header("--- EÞYA TÜRÜ ---")]
    public LootType type;       // Inspector'dan seç: XP mi Coin mi?
    public float amount = 10f;  // Script otomatik dolduracak ama test için deðiþtirebilirsin

    [Header("--- MAGNET AYARLARI ---")]
    public float magnetRange = 5f;    // Kaç metreden çekmeye baþlasýn?
    public float moveSpeed = 12f;     // Sana doðru uçma hýzý
    public float rotationSpeed = 90f; // Yerde dönme hýzý

    private Transform playerTransform;
    private bool isMagnetized = false; // Yakalandý mý?

    void Start()
    {
        // Oyuncuyu bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Doðduðunda havaya hafif bir zýplama efekti
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomForce = new Vector3(Random.Range(-2f, 2f), 4f, Random.Range(-2f, 2f));
            rb.AddForce(randomForce, ForceMode.Impulse);
        }
    }

    void Update()
    {
        // 1. Kendi etrafýnda dön
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        if (playerTransform == null) return;

        // 2. Mesafeyi Ölç
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Menzile girdiysen mýknatýs baþlasýn
        if (distance < magnetRange)
        {
            isMagnetized = true;
        }

        // 3. OYUNCUYA UÇ
        if (isMagnetized)
        {
            // Oyuncuya doðru hareket
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);

            // Yaklaþtýkça hýzlansýn (Vakum hissi)
            moveSpeed += 15f * Time.deltaTime;

            // Çok yaklaþtýysa topla
            if (distance < 0.5f)
            {
                Collect();
            }
        }
    }

    void Collect()
    {
        // --- SES EFEKTLERÝ BURAYA EKLENDÝ ---
        if (AudioManager.Instance != null)
        {
            if (type == LootType.XP)
                AudioManager.Instance.PlayXpSFX();   // XP Sesi
            else if (type == LootType.Coin)
                AudioManager.Instance.PlayCoinSFX(); // Coin Sesi
        }
        // ------------------------------------

        if (GameManager.Instance != null)
        {
            if (type == LootType.XP)
                GameManager.Instance.AddXP(amount);
            else if (type == LootType.Coin)
                GameManager.Instance.AddCoin((int)amount);
        }

        Destroy(gameObject); // Objeyi yok et
    }
}