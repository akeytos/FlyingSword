using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    [Header("--- UI BAÐLANTILARI ---")]
    public GameObject levelUpPanel;      // Siyah arka planlý panel
    public UpgradeButton[] upgradeButtons;   // 3 adet butonumuz

    [Header("--- VERÝLER ---")]
    public UpgradeData[] allUpgrades;    // ScriptableObject yeteneklerin hepsi buraya

    void Awake()
    {
        // Singleton (Tekil Yapý)
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Oyun baþýnda paneli gizle ve zamanýn aktýðýndan emin ol
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // GAMEMANAGER BU FONKSÝYONU ÇAÐIRACAK
    public void ShowLevelUpOptions()
    {
        // 1. OYUNU DURDUR (En önemli kýsým)
        Time.timeScale = 0f;

        // 2. Mouse'u Serbest Býrak (Týklama yapabilmek için)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Paneli Aç
        if (levelUpPanel != null) levelUpPanel.SetActive(true);

        // --- KARTLARI KARIÞTIR VE DAÐIT ---
        // Mevcut yeteneklerin kopyasýný al
        List<UpgradeData> availableUpgrades = new List<UpgradeData>(allUpgrades);

        // Karýþtýr (Shuffle)
        for (int i = 0; i < availableUpgrades.Count; i++)
        {
            UpgradeData temp = availableUpgrades[i];
            int randomIndex = Random.Range(i, availableUpgrades.Count);
            availableUpgrades[i] = availableUpgrades[randomIndex];
            availableUpgrades[randomIndex] = temp;
        }

        // Butonlara Daðýt
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < availableUpgrades.Count)
            {
                upgradeButtons[i].gameObject.SetActive(true);
                upgradeButtons[i].SetUpgrade(availableUpgrades[i]);
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // KARTA TIKLANINCA ÇALIÞIR
    public void SelectUpgrade(UpgradeData data)
    {
        if (data != null)
        {
            Debug.Log("Seçilen Kart: " + data.upgradeName);

            // --- 1. STAT ARTIÞLARI (CAN, HIZ VS.) ---
            if (data.type == UpgradeType.StatBoost)
            {
                ApplyStatUpgrade(data);
            }
            // --- 2. YETENEKLER (SKILLS) ---
            else if (data.type == UpgradeType.ActiveSkill || data.type == UpgradeType.PassiveSkill)
            {
                if (PlayerSkillController.Instance != null)
                {
                    bool eklendi = PlayerSkillController.Instance.TryAddSkill(data);
                    if (!eklendi) Debug.LogWarning("Skill slotlarý dolu veya hata oluþtu!");
                }
            }
        }

        // Seçim bitti, paneli kapat
        ClosePanel();
    }

    // Statlarý yöneten özel fonksiyon
    void ApplyStatUpgrade(UpgradeData data)
    {
        // Örnek: Kartýn adý "MaxHealth" ise can ver
        if (data.upgradeName == "MaxHealth" || data.upgradeName == "HealthUp")
        {
            // Sahnedeki oyuncuyu bul ve can ver (PlayerHealth scriptine baðlý)
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                // Deðer kadar (örn: 1 kalp) iyileþtir
                playerHealth.Heal((int)data.value);
                Debug.Log("Can Artýrýldý!");
            }
        }
        // Ýleride buraya "SpeedUp", "DamageUp" gibi else if'ler ekleyebilirsin
    }

    void ClosePanel()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        // ZAMANI TEKRAR AKIT (Çok Önemli)
        Time.timeScale = 1f;

        // Mouse'u tekrar kilitle (FPS modu için)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}