using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [Header("--- BUTON ÝÇÝNDEKÝLER ---")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    private UpgradeData currentData; // O an bu butonda hangi yetenek var?

    // Manager buraya veri gönderecek
    public void SetUpgrade(UpgradeData data)
    {
        currentData = data;

        // Görselleri güncelle
        nameText.text = data.upgradeName;
        descText.text = data.description;
        if (data.icon != null) iconImage.sprite = data.icon;
    }

    // Butona týklanýnca bu çalýþacak (On Click)
    public void OnClick()
    {
        if (currentData != null)
        {
            // Manager'a "Ben seçildim!" de
            LevelUpManager.Instance.SelectUpgrade(currentData);
        }
    }
}