using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance;

    [Header("--- CAN SÝSTEMÝ (KALPLER) ---")]
    public GameObject[] hearts;

    [Header("--- ÝSTATÝSTÝKLER ---")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI killText;

    [Header("--- XP BAR & LEVEL ---")]
    public Slider xpSlider;
    public TextMeshProUGUI levelText;

    // --- ÝSÝMLER DEÐÝÞTÝ ---
    [Header("--- AKTÝF YETENEK SLOTLARI (Active Skills) ---")]
    // Buraya Void Flicker, Clone Edges gibi kullandýðýn yeteneklerin kutularýný sürükle
    public List<Image> activeSkillSlots = new List<Image>();

    [Header("--- PASÝF YETENEK SLOTLARI (Passive Skills) ---")]
    // Buraya Kitaplar, Kalkan, Güç artýþý gibi pasiflerin kutularýný sürükle
    public List<Image> passiveSkillSlots = new List<Image>();

    public Sprite lockedSlotSprite; // Boþ/Kilitli kutu resmi

    [Header("--- PANELLER ---")]
    public GameObject levelUpPanel;
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    // --- GÜNCELLEME FONKSÝYONLARI (Standart) ---
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

    public void UpdateCoinUI(int amount)
    {
        if (coinText != null) coinText.text = amount.ToString();
    }

    public void UpdateKillUI(int amount)
    {
        if (killText != null) killText.text = amount.ToString();
    }

    public void UpdateLevelUI(int level, float currentXP, float maxXP)
    {
        if (levelText != null) levelText.text = "LVL : " + level.ToString();
        if (xpSlider != null) xpSlider.value = currentXP / maxXP;
    }

    // --- YENÝLENMÝÞ SLOT MANTIÐI ---
    public void AddSkillToHUD(UpgradeData newSkill)
    {
        List<Image> targetSlots = null;

        // 1. Ýsimlendirmeyi düzelttik: Active vs Passive
        if (newSkill.type == UpgradeType.ActiveSkill)
        {
            targetSlots = activeSkillSlots; // Aktif Skill Listesine bak
        }
        else
        {
            targetSlots = passiveSkillSlots; // Pasif/Kitap Listesine bak
        }

        // 2. Boþ yer bul ve yerleþ
        if (targetSlots != null)
        {
            foreach (var slot in targetSlots)
            {
                // Slot boþsa (Resmi kilitse veya null ise)
                if (slot.sprite == lockedSlotSprite || slot.sprite == null)
                {
                    slot.sprite = newSkill.icon; // Ýkonu koy
                    slot.color = Color.white;    // Görünür yap
                    Debug.Log("UI: " + newSkill.upgradeName + " slota eklendi.");
                    return;
                }
            }
        }
    }
}