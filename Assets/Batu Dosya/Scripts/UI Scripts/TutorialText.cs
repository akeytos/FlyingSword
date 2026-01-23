using UnityEngine;
using TMPro;

public class TutorialText : MonoBehaviour
{
    public TextMeshProUGUI titleText; // "KONTROLLER" yazan baþlýk
    public TextMeshProUGUI moveText;  // "HAREKET..." yazan yer
    public TextMeshProUGUI aimText;   // "SALDIRI..." yazan yer

    void Start()
    {
        UpdateTexts();
    }

    void UpdateTexts()
    {
        if (LanguageManager.Instance != null)
        {
            if (titleText) titleText.text = LanguageManager.Instance.GetText("tutorial_controls");
            if (moveText) moveText.text = LanguageManager.Instance.GetText("tutorial_move");
            if (aimText) aimText.text = LanguageManager.Instance.GetText("tutorial_aim");
        }
    }
}