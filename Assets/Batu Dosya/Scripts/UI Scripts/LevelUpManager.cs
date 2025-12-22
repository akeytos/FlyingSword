using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Listeler (List) için gerekli

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    [Header("--- UI BAÐLANTILARI ---")]
    public GameObject levelUpPanel;      // Siyah arka planlý panel
    public UpgradeButton[] upgradeButtons;   // 3 adet butonumuz

    [Header("--- VERÝLER ---")]
    public UpgradeData[] allUpgrades;    // Oyundaki TÜM yetenekler (Statlar + Skiller) buraya sürüklenecek

    void Awake()
    {
        // Singleton yapýsý
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Oyun baþýnda paneli gizle
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
    }

    // BU FONKSÝYONU GAMEMANAGER (LEVEL ATLAYINCA) ÇAÐIRACAK
    public void ShowLevelUpOptions()
    {
        // 1. Oyunu Durdur
        Time.timeScale = 0f;

        // 2. Mouse'u Serbest Býrak (Týklama yapabilmek için)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Paneli Aç
        levelUpPanel.SetActive(true);

        // --- MANTIK: KARIÞTIR VE DAÐIT (DUPLICATE ENGELLEME) ---

        // A) Mevcut yeteneklerin geçici bir kopyasýný oluþtur (Orijinal liste bozulmasýn)
        List<UpgradeData> availableUpgrades = new List<UpgradeData>(allUpgrades);

        // B) Listeyi karýþtýr (Fisher-Yates Shuffle Algoritmasý)
        for (int i = 0; i < availableUpgrades.Count; i++)
        {
            UpgradeData temp = availableUpgrades[i];
            int randomIndex = Random.Range(i, availableUpgrades.Count);
            availableUpgrades[i] = availableUpgrades[randomIndex];
            availableUpgrades[randomIndex] = temp;
        }

        // C) Karýþmýþ listenin ilk 3 elemanýný butonlara daðýt
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            // Eðer listemizde yeterince yetenek varsa butonu doldur
            if (i < availableUpgrades.Count)
            {
                upgradeButtons[i].gameObject.SetActive(true); // Butonu görünür yap
                upgradeButtons[i].SetUpgrade(availableUpgrades[i]);
            }
            else
            {
                // Eðer yetenek sayýsý buton sayýsýndan azsa, boþ butonu gizle
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // KART SEÇÝLÝNCE ÇALIÞIR (Butona týklayýnca burasý tetiklenir)
    public void SelectUpgrade(UpgradeData data)
    {
        if (data != null)
        {
            Debug.Log("Seçilen Kart: " + data.upgradeName);

            // --- TÜR KONTROLÜ VE UYGULAMA ---

            // DURUM 1: STAT ARTIÞI (Can, Hýz vb.)
            if (data.type == UpgradeType.StatBoost)
            {
                // Buraya stat artýrma kodlarý gelecek. Örnek:
                // if (data.upgradeName == "HealthUp") FindObjectOfType<PlayerHealth>().Heal(1);
                Debug.Log("Stat Artýþý Uygulandý: " + data.value);
            }
            // DURUM 2: YETENEK (Void Flicker, Kinetic Lance, Clone Edges)
            else if (data.type == UpgradeType.ActiveSkill || data.type == UpgradeType.PassiveSkill)
            {
                // Skill Controller'a haber verip yeteneði ekletiyoruz
                if (PlayerSkillController.Instance != null)
                {
                    bool basarili = PlayerSkillController.Instance.TryAddSkill(data);

                    if (!basarili)
                    {
                        Debug.LogWarning("Yetenek eklenemedi! Slotlar dolu olabilir.");
                        // Ýstersen burada return diyerek paneli kapatmayabilirsin.
                    }
                }
            }
        }

        // Seçim yapýldý, paneli kapat ve oyuna dön
        ClosePanel();
    }

    void ClosePanel()
    {
        levelUpPanel.SetActive(false);
        Time.timeScale = 1f; // Zamaný tekrar akýt

        // Mouse'u tekrar gizle ve kilitle (Oyun moduna dönüþ)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}