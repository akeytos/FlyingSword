using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("--- UI BAÐLANTILARI ---")]
    public GameObject pausePanel;       // Siyah arka planlý ana panel

    [Header("--- ORTA BÝLGÝLER ---")]
    public TextMeshProUGUI mapNameText; // "Map Name" yazýsý
    public TextMeshProUGUI levelText;   // "Level 0" yazýsý

    [Header("--- SAÐ ÝSTATÝSTÝKLER ---")]
    public TextMeshProUGUI damageText;  // "Damage : 0"
    public TextMeshProUGUI speedText;   // "Speed : 5"
    public TextMeshProUGUI healthText;  // "Health : 2"
    public TextMeshProUGUI killsText;   // "Kills : 10"

    [Header("--- DURUM ---")]
    public bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        // ESC Tuþuna basýnca
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // --- HATA VEREN KISIM DÜZELTÝLDÝ ---
            // Eðer GameOverManager varsa ve panellerden biri açýksa (Died veya Abstract), Pause açma.
            if (GameOverManager.Instance != null)
            {
                // diedPanel veya abstractPanel açýk mý?
                if (GameOverManager.Instance.diedPanel.activeSelf || GameOverManager.Instance.abstractPanel.activeSelf)
                    return;
            }

            // Eðer Level atlama ekraný açýksa Pause açma
            if (LevelUpManager.Instance != null && LevelUpManager.Instance.levelUpPanel.activeSelf) return;
            // ------------------------------------

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        UpdatePauseUI(); // Verileri güncelle

        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Zamaný durdur

        // Mouse'u göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Zamaný akýt

        // Mouse'u gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UpdatePauseUI()
    {
        if (GameManager.Instance != null)
        {
            if (levelText != null) levelText.text = "Level " + GameManager.Instance.currentLevel.ToString();
            if (killsText != null) killsText.text = "Kills : " + GameManager.Instance.currentKills.ToString();
        }

        PlayerHealth player = FindObjectOfType<PlayerHealth>();
        if (player != null)
        {
            if (healthText != null) healthText.text = "Health : " + player.currentHealth.ToString();
        }

        if (damageText != null) damageText.text = "Damage : 10";
        if (speedText != null) speedText.text = "Speed : 5";
        if (mapNameText != null) mapNameText.text = "Dark Forest";
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenMap() { Debug.Log("Map butonuna basýldý"); }
    public void OpenSettings() { Debug.Log("Settings butonuna basýldý"); }
}