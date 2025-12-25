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
        // 1. OYUNU DURDUR
        Time.timeScale = 0f;

        // 2. Mouse'u Serbest Býrak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Paneli Aç
        if (levelUpPanel != null) levelUpPanel.SetActive(true);

        // --- KARTLARI KARIÞTIR VE DAÐIT ---
        List<UpgradeData> availableUpgrades = new List<UpgradeData>(allUpgrades);

        // Karýþtýr (Fisher-Yates Shuffle)
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
                // Burada ileride "Slot doluysa aktif skilli gösterme" gibi filtreler eklenebilir.
                upgradeButtons[i].SetUpgrade(availableUpgrades[i]);
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // KARTA TIKLANINCA ÇALIÞIR (GÜNCELLENEN KISIM)
    public void SelectUpgrade(UpgradeData data)
    {
        if (data != null)
        {
            Debug.Log("Seçilen Kart: " + data.upgradeName);

            // --- YENÝ SÝSTEM ENTEGRASYONU ---

            // DURUM 1: RÜN (Stat Deðiþimi)
            if (data.category == UpgradeCategory.Rune)
            {
                if (SwordStats.Instance != null)
                {
                    // Hýzý, Caný, Boyutu vs. SwordStats yönetsin
                    SwordStats.Instance.ApplyRune(data.statName, data.statValue);
                }
                else
                {
                    Debug.LogWarning("SwordStats bulunamadý! Kýlýca scripti ekledin mi?");
                }
            }
            // DURUM 2: YETENEK (Skill) - BURASI DEÐÝÞTÝ
            else if (data.category == UpgradeCategory.Skill)
            {
                // ARTIK AbilityManager YOK, PlayerSkillController VAR
                if (PlayerSkillController.Instance != null)
                {
                    // Yetenek ekleme iþini PlayerSkillController yapsýn
                    bool basarili = PlayerSkillController.Instance.TryAddSkill(data);

                    if (!basarili)
                    {
                        // Slot doluysa vs. burada log düþer
                        Debug.LogWarning("Yetenek Eklenemedi.");
                    }
                }
                else
                {
                    Debug.LogWarning("PlayerSkillController bulunamadý! Kýlýca scripti ekledin mi?");
                }
            }
        }

        // Seçim bitti, paneli kapat
        ClosePanel();
    }

    void ClosePanel()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        // ZAMANI TEKRAR AKIT
        Time.timeScale = 1f;

        // Mouse'u tekrar kilitle (Oyun moduna dönüþ)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}