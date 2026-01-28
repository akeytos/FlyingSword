using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro K�t�phanesi

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("--- 1. A�AMA: DIED EKRANI ---")]
    public GameObject diedPanel;        // �lk a��lan "Died" paneli

    [Header("--- 2. A�AMA: ABSTRACT EKRANI ---")]
    public GameObject abstractPanel;    // �kinci a��lan "�zet" paneli

    [Header("--- �STAT�ST�K TEXTLER� (ABSTRACT) ---")]
    public TextMeshProUGUI killsText;   // "Kills: 100"
    public TextMeshProUGUI timeText;    // "Survival Time: 10:00"
    public TextMeshProUGUI levelText;   // "Level: 5"
    public TextMeshProUGUI coinText;    // "+ 50 Coin"

    private int cachedKills;
    private int cachedLevel;
    private int cachedCoins;
    private float cachedTime;
    private bool hasCachedStats;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 1. ADIM: KARAKTER �L�NCE BU �A�RILIR (PlayerHealth �a��r�r)
    public void ShowGameOver()
    {
        CacheStats();

        // Zaman� durdur
        Time.timeScale = 0f;

        // Mouse'u a�
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Sadece Died panelini a�, di�erini kapat
        if (diedPanel != null) diedPanel.SetActive(true);
        if (abstractPanel != null) abstractPanel.SetActive(false);
    }

    // 2. ADIM: �LK BUTONA BASINCA ABSTRACT EKRANINA GE�
    public void GoToAbstractScreen()
    {
        // Died panelini kapat
        if (diedPanel != null) diedPanel.SetActive(false);

        // Abstract panelini a�
        if (abstractPanel != null) abstractPanel.SetActive(true);

        ResolveAbstractTexts();

        // --- VERİLERİ YAZ ---
        int kills = hasCachedStats ? cachedKills : 0;
        int level = hasCachedStats ? cachedLevel : 0;
        int coins = hasCachedStats ? cachedCoins : 0;
        float time = hasCachedStats ? cachedTime : 0f;

        string killsLabel = GetLocalized("abstract_kills", "Kills");
        string timeLabel = GetLocalized("abstract_time", "Survival Time");
        string levelLabel = GetLocalized("abstract_level", "Level");
        string coinLabel = GetLocalized("abstract_coin", "Coin");

        if (killsText != null)
            killsText.text = $"{killsLabel}: {kills}";

        if (timeText != null)
        {
            string formattedTime = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(time / 60), Mathf.FloorToInt(time % 60));
            timeText.text = $"{timeLabel}: {formattedTime}";
        }

        if (levelText != null)
            levelText.text = $"{levelLabel}: {level}";

        if (coinText != null)
            coinText.text = $"+ {coins} {coinLabel}";
    }

    void CacheStats()
    {
        if (GameManager.Instance == null)
        {
            hasCachedStats = false;
            return;
        }

        cachedKills = GameManager.Instance.currentKills;
        cachedLevel = GameManager.Instance.currentLevel;
        cachedCoins = GameManager.Instance.currentCoins;
        cachedTime = GameManager.Instance.gameTime;
        hasCachedStats = true;
    }

    void ResolveAbstractTexts()
    {
        if (abstractPanel == null) return;

        if (killsText != null && timeText != null && levelText != null && coinText != null) return;

        var texts = abstractPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            if (killsText == null && t.name.Contains("Kills")) killsText = t;
            else if (timeText == null && t.name.Contains("Time")) timeText = t;
            else if (levelText == null && t.name.Contains("Level")) levelText = t;
            else if (coinText == null && t.name.Contains("Coin")) coinText = t;
        }
    }

    string GetLocalized(string key, string fallback)
    {
        if (LanguageManager.Instance != null)
            return LanguageManager.Instance.GetText(key);
        return fallback;
    }

    // 3. ADIM: SON BUTONA BASINCA OYUNU YEN�DEN BA�LAT
    public void RestartGame()
    {
        Time.timeScale = 1f; // Zaman� ak�t
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Sahneyi yeniden y�kle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}