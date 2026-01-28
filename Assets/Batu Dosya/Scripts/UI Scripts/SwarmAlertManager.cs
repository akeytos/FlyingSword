using UnityEngine;
using TMPro;
using System.Collections;

public class SwarmAlertManager : MonoBehaviour
{
    public static SwarmAlertManager Instance;

    [Header("UI Referanslarý")]
    public GameObject warningTextObj; // Text objesinin kendisi (Açýp kapatmak için)
    public TextMeshProUGUI warningLabel; // Yazýyý deðiþtirmek için (Dil ayarý)

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Baþlangýçta gizli olduðundan emin ol
        if (warningTextObj != null) warningTextObj.SetActive(false);
    }

    // --- BU FONKSÝYONU ÇAÐIRACAKSIN ---
    public void ShowSwarmWarning()
    {
        // 1. O anki dile göre metni çek ("swarm_alert" anahtarýný LanguageManager'a eklemiþtik)
        if (LanguageManager.Instance != null && warningLabel != null)
        {
            warningLabel.text = LanguageManager.Instance.GetText("swarm_alert");
        }

        // 2. Yazýyý aç
        if (warningTextObj != null)
        {
            warningTextObj.SetActive(true);

            // Varsa eski blink'i durdur, yenisini baþlat
            StopAllCoroutines();
            StartCoroutine(BlinkRoutine());

            // 3. 4 saniye sonra otomatik kapat
            CancelInvoke("HideWarning");
            Invoke("HideWarning", 4f);
        }
    }

    void HideWarning()
    {
        if (warningTextObj != null) warningTextObj.SetActive(false);
    }

    // --- JUICE: YANIP SÖNME EFEKTÝ ---
    IEnumerator BlinkRoutine()
    {
        if (warningLabel == null) yield break;

        while (warningTextObj.activeSelf)
        {
            warningLabel.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            warningLabel.color = Color.yellow; // Kýrmýzý-Sarý arasý gidip gelir
            yield return new WaitForSeconds(0.2f);
        }
    }
}