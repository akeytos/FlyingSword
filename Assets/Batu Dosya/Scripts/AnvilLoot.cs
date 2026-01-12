using UnityEngine;
using System.Collections;
using TMPro; // Eðer tepesinde yazý çýksýn istiyorsan gerekli

public class AnvilLoot : MonoBehaviour
{
    [Header("--- AYARLAR ---")]
    public KeyCode interactKey = KeyCode.E; // Hangi tuþla açýlsýn?
    public bool destroyAfterUse = true;     // Kullandýktan sonra yok olsun mu?

    [Header("--- UI (Opsiyonel) ---")]
    public GameObject interactPopup; // Tepesinde çýkacak "E" tuþu görseli veya yazýsý

    [Header("--- LOOT AYARLARI ---")]
    public GameObject[] lootPrefabs;     // Coin, XP vs.
    public int dropCount = 5;            // Kaç adet düþsün?
    public float scatterForce = 8f;      // Saçýlma gücü
    public float upwardForce = 6f;       // Zýplama gücü

    [Header("--- EFEKTLER ---")]
    public AudioClip anvilHitSound;      // Açýlma sesi
    public ParticleSystem hitEffect;     // Efekt

    private bool isOpened = false;
    private bool isPlayerInRange = false; // Oyuncu dibimizde mi?

    void Start()
    {
        // Baþlangýçta "E" yazýsýný gizle
        if (interactPopup != null) interactPopup.SetActive(false);
    }

    void Update()
    {
        // Eðer oyuncu alandaysa VE henüz açýlmadýysa VE tuþa bastýysa
        if (isPlayerInRange && !isOpened && Input.GetKeyDown(interactKey))
        {
            OpenAnvil();
        }
    }

    // --- ALAN KONTROLÜ ---
    void OnTriggerEnter(Collider other)
    {
        if (isOpened) return;

        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // "E" yazýsýný göster
            if (interactPopup != null) interactPopup.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // "E" yazýsýný gizle
            if (interactPopup != null) interactPopup.SetActive(false);
        }
    }

    // --- AÇILMA ÝÞLEMÝ ---
    public void OpenAnvil()
    {
        isOpened = true;

        // Yazýyý hemen gizle
        if (interactPopup != null) interactPopup.SetActive(false);

        // 1. Ses
        if (AudioManager.Instance != null && anvilHitSound != null)
        {
            AudioSource.PlayClipAtPoint(anvilHitSound, transform.position);
        }

        // 2. Efekt
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // 3. Animasyon ve Loot
        StartCoroutine(AnvilRoutine());
    }

    IEnumerator AnvilRoutine()
    {
        // --- SALLANMA EFEKTÝ ---
        Vector3 originalPos = transform.position;
        float shakeDuration = 0.2f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-0.1f, 0.1f);
            float z = Random.Range(-0.1f, 0.1f);
            transform.position = originalPos + new Vector3(x, 0, z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;

        // --- PATLAMA (LOOT) ---
        SpawnLoot();

        // --- YOK OLMA ---
        if (destroyAfterUse)
        {
            float shrinkDuration = 0.5f;
            Vector3 startScale = transform.localScale;
            elapsed = 0f;

            while (elapsed < shrinkDuration)
            {
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / shrinkDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }
    }

    void SpawnLoot()
    {
        if (lootPrefabs == null || lootPrefabs.Length == 0) return;

        for (int i = 0; i < dropCount; i++)
        {
            GameObject selectedLoot = lootPrefabs[Random.Range(0, lootPrefabs.Length)];
            Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
            GameObject lootObj = Instantiate(selectedLoot, spawnPos, Quaternion.identity);

            Rigidbody rb = lootObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomDir = Random.insideUnitSphere;
                randomDir.y = 0;
                randomDir.Normalize();
                Vector3 force = (randomDir * scatterForce) + (Vector3.up * upwardForce);
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}