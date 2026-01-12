using UnityEngine;

public class LanguagePanelController : MonoBehaviour
{
    public GameObject languageItemPrefab; // Hazýrladýðýmýz buton prefabý
    public Transform contentParent;       // Scroll View içindeki Content objesi

    void OnEnable()
    {
        RefreshList();
        // Dil deðiþirse listeyi (tikleri) güncelle
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged += RefreshList;
    }

    void OnDisable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged -= RefreshList;
    }

    void RefreshList()
    {
        // 1. Önce eski listeyi temizle
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (LanguageManager.Instance == null) return;

        // 2. Yeni listeyi oluþtur
        string currentLang = LanguageManager.Instance.currentLanguage;

        foreach (var lang in LanguageManager.Instance.supportedLanguages)
        {
            // Prefabý oluþtur
            GameObject newItem = Instantiate(languageItemPrefab, contentParent);

            // Scriptine ulaþ ve ayarla
            LanguageItem itemScript = newItem.GetComponent<LanguageItem>();
            bool isSelected = (lang.code == currentLang);

            itemScript.Setup(lang.code, lang.name, isSelected);
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}