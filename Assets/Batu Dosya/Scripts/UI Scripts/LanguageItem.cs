using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LanguageItem : MonoBehaviour
{
    [Header("UI Bileþenleri")]
    public TextMeshProUGUI langNameText; // Dil ismi yazan yer
    public GameObject checkmarkObj;      // Tik iþareti (Image)
    public Button btn;                   // Buton bileþeni

    private string myLangCode; // "tr" veya "en"

    public void Setup(string code, string name, bool isSelected)
    {
        myLangCode = code;
        langNameText.text = name;
        checkmarkObj.SetActive(isSelected); // Seçiliyse tiki aç

        // Butona týklanýnca ne olsun?
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            // LanguageManager'a dili deðiþtir diyoruz
            LanguageManager.Instance.SetLanguage(myLangCode);
        });
    }
}