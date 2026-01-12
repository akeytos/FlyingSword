using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public string key; // Inspector'a "play_btn" yazacaðýn yer
    private TextMeshProUGUI textComp;

    void Start()
    {
        textComp = GetComponent<TextMeshProUGUI>();
        UpdateText();

        // Dil deðiþirse bana haber ver
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged += UpdateText;
    }

    void OnDestroy()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.OnLanguageChanged -= UpdateText;
    }

    public void UpdateText()
    {
        if (textComp != null && LanguageManager.Instance != null)
        {
            textComp.text = LanguageManager.Instance.GetText(key);
        }
    }
}