using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    [Header("--- UI BAÐLANTILARI ---")]
    public GameObject levelUpPanel;
    public UpgradeButton[] upgradeButtons;

    [Header("--- VERÝLER ---")]
    public UpgradeData[] allUpgrades;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowLevelUpOptions()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (levelUpPanel != null) levelUpPanel.SetActive(true);

        // Kartlarý Karýþtýr ve Daðýt
        List<UpgradeData> availableUpgrades = new List<UpgradeData>(allUpgrades);
        for (int i = 0; i < availableUpgrades.Count; i++)
        {
            UpgradeData temp = availableUpgrades[i];
            int randomIndex = Random.Range(i, availableUpgrades.Count);
            availableUpgrades[i] = availableUpgrades[randomIndex];
            availableUpgrades[randomIndex] = temp;
        }

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

    // --- GÜNCELLENEN FONKSÝYON ---
    public void SelectUpgrade(UpgradeData data)
    {
        if (data != null)
        {
            Debug.Log("Seçilen Kart: " + data.upgradeName);

            // 1. ÖZELLÝÐÝ ÝÞLE (Logic)
            if (data.category == UpgradeCategory.Rune)
            {
                if (SwordStats.Instance != null)
                {
                    // SwordStats string olarak statName bekliyor (Örn: "Size")
                    SwordStats.Instance.ApplyRune(data.statName, data.statValue);
                }
            }
            else if (data.category == UpgradeCategory.Skill)
            {
                // PlayerSkillController veya AbilityManager burayý halleder
                // (Senin projende hangisi aktifse o kalmalý)
                Debug.Log("Yetenek seçildi: " + data.upgradeName);
            }

            // 2. UI'A ÝKONU EKLE (Görsel) - BURAYI EKLEDÝM
            if (InGameUIManager.Instance != null)
            {
                InGameUIManager.Instance.AddSkillToHUD(data);
            }
        }

        ClosePanel();
    }

    void ClosePanel()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}