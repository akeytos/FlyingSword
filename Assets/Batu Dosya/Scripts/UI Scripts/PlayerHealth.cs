using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("--- CAN AYARLARI ---")]
    public int maxHealth = 5;
    public int currentHealth;
    public bool isDead = false;

    [Header("--- HASAR BEKLEME (YEN�) ---")]
    public float iframeDuration = 1.8f; // Vurulduktan sonra kac sn olumsuz olalim?
    private bool isInvincible = false;  // �u an �l�ms�z m�y�z?
    public GameObject visualModel;      // Karakterin i�indeki model (Yan�p s�nmesi i�in)

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int damageAmount)
    {
        // E�er zaten �ld�ysek VEYA �u an �l�ms�zl�k s�resindeysek hasar alma!
        if (isDead || isInvincible) return;

        currentHealth -= damageAmount;

        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        // �ld�k m�?
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // �lmediysek, ge�ici bir s�re �l�ms�z ol (Yan�p s�n)
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
        Debug.Log("�LD�N! GAME OVER.");

        // Oyunu durdurabilirsin istersen:
        // Time.timeScale = 0; 

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }
    }

    // --- �L�MS�ZL�K MANTI�I (YANIP S�NME) ---
    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // Modeli h�zl�ca a��p kapatarak "Flash" efekti yap�yoruz
        for (float i = 0; i < iframeDuration; i += 0.2f)
        {
            if (visualModel != null) visualModel.SetActive(!visualModel.activeSelf);
            yield return new WaitForSeconds(0.1f);
        }

        // S�re bitince modeli kesin g�r�n�r yap ve �l�ms�zl��� kapat
        if (visualModel != null) visualModel.SetActive(true);
        isInvincible = false;
    }

    // TEST
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) TakeDamage(1);
    }
}