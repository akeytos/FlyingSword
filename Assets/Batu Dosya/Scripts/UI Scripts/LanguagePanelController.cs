using UnityEngine;
using UnityEngine.UI; // Layout iþlemleri için gerekli

public class LanguagePanelController : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject languageItemPrefab; // Hazýrladýðýmýz buton prefabý
    public Transform contentParent;       // Scroll View içindeki Content objesi

    void OnEnable()
    {
        RefreshList();
        // Dil deðiþirse listeyi anlýk güncelle (Tik iþaretinin yer deðiþtirmesi için)
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
        // 1. Önce eski listeyi temizle (Duplicate olmasýn)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (LanguageManager.Instance == null) return;

        // 2. Yeni listeyi LanguageManager'dan çek
        string currentLang = LanguageManager.Instance.currentLanguage;

        foreach (var lang in LanguageManager.Instance.supportedLanguages)
        {
            // Prefabý oluþtur
            GameObject newItem = Instantiate(languageItemPrefab, contentParent);

            // Scriptine ulaþ ve veriyi gönder
            LanguageItem itemScript = newItem.GetComponent<LanguageItem>();

            if (itemScript != null)
            {
                // Þu anki dil bu mu? (Örn: "tr" == "tr" ise tik koy)
                bool isSelected = (lang.code == currentLang);

                // Butonu kur
                itemScript.Setup(lang.code, lang.name, isSelected);
            }
        }

        // 3. UI Bazen saçmalayýp üst üste binerse diye Layout'u yenile
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
    }

    public void ClosePanel()
    {
        // Paneli kapat (Animation varsa buraya eklenir)
        gameObject.SetActive(false);
    }
}