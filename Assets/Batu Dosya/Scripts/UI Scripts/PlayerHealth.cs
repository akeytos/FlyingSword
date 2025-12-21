using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("--- CAN AYARLARI ---")]
    public int maxHealth = 3; // Toplam 3 Kalp
    public int currentHealth;

    public bool isDead = false;

    void Start()
    {
        // Oyuna 3 kalp ile baþla
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        // Eksiye düþmesin
        if (currentHealth < 0) currentHealth = 0;

        // UI'a haber ver
        UpdateUI();

        // Öldük mü?
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        // 3 kalpten fazla olamaz
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateUI();
    }

    void UpdateUI()
    {
        if (InGameUIManager.Instance != null)
        {
            // Kalp UI'ýný güncelle
            InGameUIManager.Instance.UpdateHealthUI(currentHealth);
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("ÖLDÜN! GAME OVER.");

        // Game Over Manager'a haber ver (Died Ekranýný Aç)
        if (GameOverManager.Instance != null)
        {
            // ÝSÝM DÜZELTÝLDÝ: ShowGameOver()
            GameOverManager.Instance.ShowGameOver();
        }
    }

    // --- TEST (H TUÞU) ---
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1); // Her basýþta 1 kalp gider
        }
    }
}