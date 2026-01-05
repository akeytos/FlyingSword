using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [Header("--- UI BAÐLANTILARI ---")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    [Header("--- YENÝ: TÜR BÝLGÝSÝ ---")]
    public TextMeshProUGUI typeText; // Kartýn üstünde "Active Skill" veya "Rune" yazacak ufak yazý

    [Header("--- RENK AYARLARI ---")]
    public Color activeSkillColor = Color.yellow; // Aktif yetenekse sarý yazsýn
    public Color passiveSkillColor = Color.green; // Pasifse yeþil
    public Color runeColor = Color.cyan;          // Rün ise mavi

    private UpgradeData currentData;

    // Manager buraya veri gönderecek
    public void SetUpgrade(UpgradeData data)
    {
        currentData = data;

        // 1. Temel Görselleri Güncelle
        nameText.text = data.upgradeName;
        descText.text = data.description;
        if (data.icon != null) iconImage.sprite = data.icon;

        // 2. Tür Bilgisini Yazdýr (YENÝ SÝSTEM)
        // Eðer typeText atamadýysan hata vermesin diye kontrol ediyoruz
        if (typeText != null)
        {
            if (data.category == UpgradeCategory.Skill)
            {
                // Skill ise: Aktif mi Pasif mi?
                if (data.skillType == SkillType.Active)
                {
                    typeText.text = "ACTIVE SKILL"; // "SAÐ TIK"
                    typeText.color = activeSkillColor;
                }
                else
                {
                    typeText.text = "PASSIVE SKILL"; // "OTOMATÝK"
                    typeText.color = passiveSkillColor;
                }
            }
            else // RUNE
            {
                // Rune ise
                typeText.text = "STAT RUNE";
                typeText.color = runeColor;
            }
        }
    }

    // Butona týklanma olayý
    public void OnClick()
    {
        if (currentData != null)
        {
            // Manager'a seçilen veriyi gönder
            LevelUpManager.Instance.SelectUpgrade(currentData);
        }
    }
}