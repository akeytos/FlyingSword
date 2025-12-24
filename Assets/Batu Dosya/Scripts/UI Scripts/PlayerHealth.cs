using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("--- CAN AYARLARI ---")]
    public int maxHealth = 3;
    public int currentHealth;
    public bool isDead = false;

    [Header("--- HASAR BEKLEME (YENÝ) ---")]
    public float iframeDuration = 1.0f; // Vurulduktan sonra kaç sn ölümsüz olalým?
    private bool isInvincible = false;  // Þu an ölümsüz müyüz?
    public GameObject visualModel;      // Karakterin içindeki model (Yanýp sönmesi için)

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int damageAmount)
    {
        // Eðer zaten öldüysek VEYA þu an ölümsüzlük süresindeysek hasar alma!
        if (isDead || isInvincible) return;

        currentHealth -= damageAmount;

        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        // Öldük mü?
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Ölmediysek, geçici bir süre ölümsüz ol (Yanýp sön)
            StartCoroutine(InvincibilityRoutine());
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.UpdateHealthUI(currentHealth);
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("ÖLDÜN! GAME OVER.");

        // Oyunu durdurabilirsin istersen:
        // Time.timeScale = 0; 

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }
    }

    // --- ÖLÜMSÜZLÜK MANTIÐI (YANIP SÖNME) ---
    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // Modeli hýzlýca açýp kapatarak "Flash" efekti yapýyoruz
        for (float i = 0; i < iframeDuration; i += 0.2f)
        {
            if (visualModel != null) visualModel.SetActive(!visualModel.activeSelf);
            yield return new WaitForSeconds(0.1f);
        }

        // Süre bitince modeli kesin görünür yap ve ölümsüzlüðü kapat
        if (visualModel != null) visualModel.SetActive(true);
        isInvincible = false;
    }

    // TEST
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) TakeDamage(1);
    }
}