using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Slider kullanacağımız için bu gerekli!

public class MainMenuManager : MonoBehaviour
{
    [Header("--- PANEL REFERANSLARI ---")]
    public GameObject centerMenuPanel;    // Ana Menü (Ortadaki Play vs.)
    public GameObject selectionPanel;     // Kılıç Seçim Ekranı
    public GameObject leaderboardPanel;   // Skor Tablosu
    public GameObject settingsPanel;      // Ayarlar Paneli
    public GameObject languageSelectionPanel; // [YENİ] Dil Seçim Listesi Paneli

    [Header("--- AYARLAR (SETTINGS) ---")]
    public Slider musicSlider;            // Müzik Sesi Slider'ı
    public Slider sfxSlider;              // Efekt Sesi Slider'ı

    void Start()
    {
        // Başlangıçta paneller kapalı olsun
        if (settingsPanel) settingsPanel.SetActive(false);
        if (languageSelectionPanel) languageSelectionPanel.SetActive(false);

        // Ana menü açık olsun
        if (centerMenuPanel) centerMenuPanel.SetActive(true);

        // --- SES AYARLARINI YÜKLE ---
        // Oyun açıldığında Sliderlar, AudioManager'daki ses seviyesinde dursun
        if (AudioManager.Instance != null)
        {
            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.Instance.musicSource.volume;
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.sfxSource.volume;
                sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }
        }
    }

    // --- 1. SOL ÜST BUTONLAR (SETTINGS & DİL) ---

    // DİL BUTONU (A/文 İkonu)
    public void Click_Language()
    {
        // [GÜNCELLENDİ] Artık direkt değiştirmek yerine listeyi açıyoruz
        if (languageSelectionPanel != null)
        {
            languageSelectionPanel.SetActive(true);
        }
    }

    // AYARLAR BUTONU (Çark İkonu)
    public void Click_Settings()
    {
        // Paneli aç/kapa yap (Toggle)
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);
        }
    }

    // SETTINGS PANELİNDEKİ "KAPAT/X" BUTONU
    public void Click_CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // --- 2. SES AYARLARI (SLIDERLAR) ---

    public void OnMusicVolumeChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.musicSource.volume = val;
        }
    }

    public void OnSfxVolumeChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.sfxSource.volume = val;
        }
    }

    // --- 3. ANA MENÜ AKIŞI ---

    public void Click_Play()
    {
        centerMenuPanel.SetActive(false);
        selectionPanel.SetActive(true);

        // Diğer paneller açıksa kapat
        if (leaderboardPanel) leaderboardPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (languageSelectionPanel) languageSelectionPanel.SetActive(false);
    }

    public void Click_StartGame()
    {
        LoadingManager.nextSceneName = "levelBlockout + UI";
        SceneManager.LoadScene("Loading");
    }

    public void Click_Leaderboard()
    {
        centerMenuPanel.SetActive(false);
        selectionPanel.SetActive(false);
        if (leaderboardPanel) leaderboardPanel.SetActive(true);

        if (SteamLeaderboardManager.Instance != null)
        {
            SteamLeaderboardManager.Instance.DownloadScores();
        }
    }

    public void Click_BackToMenu()
    {
        selectionPanel.SetActive(false);
        if (leaderboardPanel) leaderboardPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (languageSelectionPanel) languageSelectionPanel.SetActive(false);

        centerMenuPanel.SetActive(true);
    }

    // --- 4. DİĞER LİNKLER ---

    public void Click_Quit()
    {
        Debug.Log("Oyundan çıkıldı");
        Application.Quit();
    }

    public void Click_Inventory() { Debug.Log("Envanter..."); }

    public void Click_Shop() { Debug.Log("Dükkan..."); }

    public void Click_Discord()
    {
        Application.OpenURL("https://discord.gg/swordslide");
    }

    public void Click_Steam()
    {
        Application.OpenURL("https://store.steampowered.com/app/swordslide");
    }
}