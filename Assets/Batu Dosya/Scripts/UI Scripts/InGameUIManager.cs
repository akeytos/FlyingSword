using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance;

    [Header("--- CAN SÝSTEMÝ ---")]
    public GameObject[] hearts;

    [Header("--- ÝSTATÝSTÝKLER ---")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI killText;

    [Header("--- XP & LEVEL ---")]
    public Slider xpSlider;
    public TextMeshProUGUI levelText;

    [Header("--- SLOTLAR ---")]
    // Active Skills: Sol Üstteki Büyük Kare
    public List<Image> activeSkillSlots = new List<Image>();

    // Passive Skills: Sol Alttaki Ýki Kare
    public List<Image> passiveSkillSlots = new List<Image>();

    // Rune Slots: Saðdaki 4 Küçük Kare (YENÝ EKLENDÝ)
    public List<Image> runeSlots = new List<Image>();

    public Sprite lockedSlotSprite; // Boþ kutu görseli

    [Header("--- PANELLER ---")]
    public GameObject levelUpPanel;
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    // --- UI GÜNCELLEME ---
    public void AddSkillToHUD(UpgradeData newUpgrade)
    {
        List<Image> targetSlots = null;

        // 1. HANGÝ KUTUYA GÝDECEK?
        if (newUpgrade.category == UpgradeCategory.Rune)
        {
            // Eðer kategori RÜN ise -> Saðdaki küçük kutulara
            targetSlots = runeSlots;
        }
        else if (newUpgrade.category == UpgradeCategory.Skill && newUpgrade.skillType == SkillType.Active)
        {
            // Eðer AKTÝF YETENEK ise -> Sol üstteki kutuya
            targetSlots = activeSkillSlots;
        }
        else
        {
            // Geri kalan her þey (Pasif Skill vb.) -> Sol alttaki kutulara
            targetSlots = passiveSkillSlots;
        }

        // 2. BOÞ YER VAR MI KONTROL ET VE EKLE
        if (targetSlots != null)
        {
            // Zaten var mý kontrolü (Ayný ikon varsa tekrar koyma)
            foreach (var slot in targetSlots)
            {
                if (slot.sprite == newUpgrade.icon) return;
            }

            // Boþ slot bul
            foreach (var slot in targetSlots)
            {
                if (slot.sprite == lockedSlotSprite || slot.sprite == null)
                {
                    slot.sprite = newUpgrade.icon;
                    slot.color = Color.white;
                    Debug.Log("UI: " + newUpgrade.upgradeName + " ikonu eklendi.");
                    return;
                }
            }
        }
    }

    // --- DÝÐER FONKSÝYONLAR (AYNEN KALIYOR) ---
    public void UpdateHealthUI(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth) hearts[i].SetActive(true);
            else hearts[i].SetActive(false);
        }
    }
    public void UpdateTimeUI(float timeInSeconds)
    {
        float minutes = Mathf.FloorToInt(timeInSeconds / 60);
        float seconds = Mathf.FloorToInt(timeInSeconds % 60);
        if (timeText != null) timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void UpdateLevelUI(int level, float currentXP, float maxXP)
    {
        if (levelText != null) levelText.text = "LVL : " + level.ToString();
        if (xpSlider != null) xpSlider.value = currentXP / maxXP;
    }
    public void UpdateCoinUI(int amount) { if (coinText != null) coinText.text = amount.ToString(); }
    public void UpdateKillUI(int amount) { if (killText != null) killText.text = amount.ToString(); }

    // Panel Kontrolleri
    public void ShowGameOverPanel() { if (gameOverPanel != null) gameOverPanel.SetActive(true); }
}