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

    [Header("--- SLOTLAR ---")]
    // Active Skills: Sadece "Sað Týk" yetenekleri (Void Flicker, Clone Edges)
    public List<Image> activeSkillSlots = new List<Image>();

    // Passive Skills: Hem Pasif Yetenekler (Kinetic Lance) hem de Rünler (Speed, Health)
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

    // --- GÜNCELLEME FONKSÝYONLARI ---
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

    // --- BURASI DÜZELTÝLDÝ ---
    public void AddSkillToHUD(UpgradeData newSkill)
    {
        List<Image> targetSlots = null;

        // Önce bu yeteneðin zaten slotlarda olup olmadýðýna bakabiliriz (Upgrade ise)
        // Ama þimdilik basitçe boþ yere ekleyelim.

        // MANTIK: 
        // Eðer bu bir SKILL ise ve türü ACTIVE ise -> Aktif Slotlara
        // Diðer her þey (Pasif Skill, Rünler) -> Pasif Slotlara

        bool isActiveSkill = (newSkill.category == UpgradeCategory.Skill && newSkill.skillType == SkillType.Active);

        if (isActiveSkill)
        {
            targetSlots = activeSkillSlots; // Aktif Skill Listesine bak
        }
        else
        {
            targetSlots = passiveSkillSlots; // Pasif/Kitap Listesine bak
        }

        // Boþ yer bul ve yerleþ
        if (targetSlots != null)
        {
            // ÖNCEKÝ KONTROL: Zaten var mý? (Varsa tekrar ikon koyma, belki level yazýsý artýrýlabilir ama þimdilik geçiyorum)
            foreach (var slot in targetSlots)
            {
                if (slot.sprite == newSkill.icon) return; // Zaten ekli, tekrar ekleme
            }

            // BOÞ SLOT BULMA
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