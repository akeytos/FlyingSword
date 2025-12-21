using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro Kütüphanesi

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("--- 1. AÞAMA: DIED EKRANI ---")]
    public GameObject diedPanel;        // Ýlk açýlan "Died" paneli

    [Header("--- 2. AÞAMA: ABSTRACT EKRANI ---")]
    public GameObject abstractPanel;    // Ýkinci açýlan "Özet" paneli

    [Header("--- ÝSTATÝSTÝK TEXTLERÝ (ABSTRACT) ---")]
    public TextMeshProUGUI killsText;   // "Kills: 100"
    public TextMeshProUGUI timeText;    // "Survival Time: 10:00"
    public TextMeshProUGUI levelText;   // "Level: 5"
    public TextMeshProUGUI coinText;    // "+ 50 Coin"

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 1. ADIM: KARAKTER ÖLÜNCE BU ÇAÐRILIR (PlayerHealth çaðýrýr)
    public void ShowGameOver()
    {
        // Zamaný durdur
        Time.timeScale = 0f;

        // Mouse'u aç
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Sadece Died panelini aç, diðerini kapat
        if (diedPanel != null) diedPanel.SetActive(true);
        if (abstractPanel != null) abstractPanel.SetActive(false);
    }

    // 2. ADIM: ÝLK BUTONA BASINCA ABSTRACT EKRANINA GEÇ
    public void GoToAbstractScreen()
    {
        // Died panelini kapat
        if (diedPanel != null) diedPanel.SetActive(false);

        // Abstract panelini aç
        if (abstractPanel != null) abstractPanel.SetActive(true);

        // --- VERÝLERÝ GAMEMANAGER'DAN ÇEK VE YAZ ---
        if (GameManager.Instance != null)
        {
            // Kill Sayýsý
            if (killsText != null)
                killsText.text = "Kills: " + GameManager.Instance.currentKills.ToString();

            // Zamaný Formatla (Dakika:Saniye)
            if (timeText != null)
            {
                float t = GameManager.Instance.gameTime;
                string formattedTime = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(t / 60), Mathf.FloorToInt(t % 60));
                timeText.text = "Survival Time: " + formattedTime;
            }

            // Level
            if (levelText != null)
                levelText.text = "Level: " + GameManager.Instance.currentLevel.ToString();

            // Coin
            if (coinText != null)
                coinText.text = "+ " + GameManager.Instance.currentCoins.ToString() + " Coin";
        }
    }

    // 3. ADIM: SON BUTONA BASINCA OYUNU YENÝDEN BAÞLAT
    public void RestartGame()
    {
        Time.timeScale = 1f; // Zamaný akýt
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}