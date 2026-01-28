using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LanguageItem : MonoBehaviour
{
    [Header("UI Bileþenleri")]
    public TextMeshProUGUI langNameText; // Dil ismi yazan Text
    public GameObject checkmarkObj;      // Tik iþareti (Image)
    public Button btn;                   // Týklama butonu

    private string myLangCode; // "tr", "en", "de"

    // Controller bu fonksiyonu çaðýrýp butonu kuracak
    public void Setup(string code, string name, bool isSelected)
    {
        myLangCode = code;

        // 1. Ýsmi yaz (Hata vermemesi için null kontrolü ekledim)
        if (langNameText != null)
            langNameText.text = name;

        // 2. Seçiliyse tiki aç
        if (checkmarkObj != null)
            checkmarkObj.SetActive(isSelected);

        // 3. Týklama olayýný baðla
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners(); // Eskileri temizle
            btn.onClick.AddListener(OnItemClicked);
        }
    }

    // Týklanýnca çalýþacak fonksiyon
    void OnItemClicked()
    {
        if (LanguageManager.Instance != null)
        {
            // Manager'a "Dili deðiþtir" emri ver
            LanguageManager.Instance.SetLanguage(myLangCode);
        }
    }
}