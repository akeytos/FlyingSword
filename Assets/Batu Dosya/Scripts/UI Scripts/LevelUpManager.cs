using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Listeler (List) için gerekli kütüphane

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    [Header("--- UI BAÐLANTILARI ---")]
    public GameObject levelUpPanel;          // Siyah arka planlý panel
    public UpgradeButton[] upgradeButtons;   // 3 adet butonumuz

    [Header("--- VERÝLER ---")]
    public UpgradeData[] allUpgrades;        // Oyundaki TÜM yetenekler buraya sürüklenecek

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

    // BU FONKSÝYONU GAMEMANAGER ÇAÐIRACAK
    public void ShowLevelUpOptions()
    {
        // 1. Oyunu Durdur
        Time.timeScale = 0f;

        // >>> EKLENEN KISIM: MOUSE'U SERBEST BIRAK VE GÖSTER <<<
        // Bunu yapmazsak mouse kilitli kalýr ve butonlara týklayamazsýn.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // ------------------------------------------------------

        // 2. Paneli Aç
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
            Debug.Log("Seçilen Yetenek: " + data.upgradeName);

            // --- BURADA KARAKTERE GÜCÜ VERECEÐÝZ ---
            // Þimdilik sadece log veriyor, ileride PlayerStats'a baðlayacaðýz.
            // if(data.type == UpgradeType.DamageBuff) PlayerDamage += data.value;
        }

        // Oyunu Devam Ettir ve Paneli Kapat
        ClosePanel();
    }

    void ClosePanel()
    {
        levelUpPanel.SetActive(false);
        Time.timeScale = 1f; // Zamaný tekrar akýt

        // >>> EKLENEN KISIM: MOUSE'U TEKRAR GÝZLE VE KÝLÝTLE <<<
        // Oyuna dönünce mouse ortada gezmesin, karaktere dönsün.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // ------------------------------------------------------
    }
}