using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel Referanslarý")]
    public GameObject centerMenuPanel;    // Hiyerarþideki "CenterMenu"yu buraya sürükle
    public GameObject selectionPanel;     // Hiyerarþideki "SelectionPanel"i buraya sürükle
    public GameObject leaderboardPanel;   // Hiyerarþideki "LeaderboardPanel"i buraya sürükle

    // --- 1. PLAY BUTONU (Artýk direkt baþlatmaz, Seçim ekranýný açar) ---
    // CenterMenu içindeki PLAY butonuna bunu ver
    public void Click_Play()
    {
        centerMenuPanel.SetActive(false);
        selectionPanel.SetActive(true);
        if (leaderboardPanel) leaderboardPanel.SetActive(false);
    }

    // --- 2. START GAME (SelectionPanel'in içindeki buton buna baðlanacak) ---
    // SelectionPanel içindeki START butonuna bunu ver
    public void Click_StartGame()
    {
        // Senin Loading sistemini aynen koruyoruz
        LoadingManager.nextSceneName = "levelBlockout + UI";
        SceneManager.LoadScene("Loading");
    }

    // --- 3. LEADERBOARD BUTONU ---
    public void Click_Leaderboard()
    {
        centerMenuPanel.SetActive(false);
        selectionPanel.SetActive(false);
        if (leaderboardPanel) leaderboardPanel.SetActive(true);

        // SteamManager varsa skorlarý çek (Yoksa hata vermez, geçer)
        if (SteamLeaderboardManager.Instance != null)
        {
            SteamLeaderboardManager.Instance.DownloadScores();
        }
    }

    // --- 4. ORTAK GERÝ DÖNME TUÞU (Back) ---
    // SelectionPanel ve LeaderboardPanel içindeki Geri/X tuþlarýna bunu ver
    public void Click_BackToMenu()
    {
        selectionPanel.SetActive(false);
        if (leaderboardPanel) leaderboardPanel.SetActive(false);

        centerMenuPanel.SetActive(true);
    }

    // --- DÝÐER BUTONLAR (Eski kodundan koruduklarým) ---

    public void Click_Quit()
    {
        Debug.Log("Oyundan çýkýldý");
        Application.Quit();
    }

    public void Click_Inventory()
    {
        Debug.Log("Envanter açýlacak...");
    }

    public void Click_Shop()
    {
        Debug.Log("Dükkan açýlacak...");
    }

    public void Click_Settings()
    {
        Debug.Log("Ayarlar paneli açýlýyor...");
    }

    public void Click_Discord()
    {
        Application.OpenURL("https://discord.gg/swordslide");
    }

    public void Click_Steam()
    {
        Application.OpenURL("https://store.steampowered.com/app/swordslide");
    }
}