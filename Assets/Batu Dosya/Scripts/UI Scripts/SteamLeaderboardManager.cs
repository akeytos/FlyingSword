using UnityEngine;
using Steamworks;
using System.Collections.Generic;

public class SteamLeaderboardManager : MonoBehaviour
{
    [Header("Ayarlar")]
    // Steam panelinde verdiğin isimle AYNI olmalı! (Büyük/küçük harf duyarlı)
    public string leaderboardID = "Global_Kills";

    [Header("UI Referansları")]
    public Transform rowContainer; // Content objesi
    public GameObject rowPrefab;   // LeaderboardRow prefabı
    public GameObject loadingSpinner; // Yükleniyor ikonu

    // Steam Özel Değişkenleri
    private SteamLeaderboard_t currentLeaderboard;
    private CallResult<LeaderboardFindResult_t> findResult;
    private CallResult<LeaderboardScoresDownloaded_t> downloadResult;
    private CallResult<LeaderboardScoreUploaded_t> uploadResult;

    void Start()
    {
        // SteamManager çalışmıyorsa dur (Hata almamak için)
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager çalışmıyor! Steam açık mı?");
            return;
        }

        // Callback kurulumları (Steam'den cevap bekleyen fonksiyonlar)
        findResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFound);
        downloadResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnScoresDownloaded);
        uploadResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnScoreUploaded);

        // 1. Adım: Tabloyu Bul
        FindLeaderboard();

        // --- TEST KODU BAŞLANGIÇ ---
        // Bunu sadece ilk test için koyduk. Çalıştığını görünce SİLERSİN.
        Debug.Log("🧪 TEST MODU AKTİF: 3 saniye sonra otomatik 150 puan yollanacak...");
        Invoke("TestUpload", 3f);
        // --- TEST KODU BİTİŞ ---
    }

    // --- TEST FONKSİYONU ---
    void TestUpload()
    {
        UploadScoreToSteam(150);
    }

    // --- 1. LİDERLİK TABLOSUNU BUL ---
    public void FindLeaderboard()
    {
        SteamAPICall_t handle = SteamUserStats.FindLeaderboard(leaderboardID);
        findResult.Set(handle);
        Debug.Log("🔍 Steam'de '" + leaderboardID + "' tablosu aranıyor...");
    }

    private void OnLeaderboardFound(LeaderboardFindResult_t pCallback, bool bIOFailure)
    {
        if (bIOFailure || pCallback.m_bLeaderboardFound == 0)
        {
            Debug.LogError("❌ HATA: Leaderboard bulunamadı! İsmi ('" + leaderboardID + "') Steam panelindekiyle birebir aynı mı?");
            return;
        }

        currentLeaderboard = pCallback.m_hSteamLeaderboard;
        Debug.Log("✅ Leaderboard bulundu! Şimdi skorlar indiriliyor...");

        // Tabloyu bulduk, şimdi içindeki skorları çekelim
        DownloadScores();
    }

    // --- 2. SKORLARI İNDİR VE LİSTELE ---
    public void DownloadScores()
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(true);

        // Global sıralamada ilk 10 kişiyi çek
        SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries(
            currentLeaderboard,
            ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
            1, // Başlangıç (1. sıradaki)
            10 // Bitiş (10. sıradaki)
        );

        downloadResult.Set(handle);
    }

    private void OnScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(false);

        if (bIOFailure)
        {
            Debug.LogError("❌ Skorlar indirilemedi (IO Failure).");
            return;
        }

        // Önce eski listeyi temizle (Duplicate olmasın)
        foreach (Transform child in rowContainer) Destroy(child.gameObject);

        int entryCount = pCallback.m_cEntryCount;
        Debug.Log("📥 İndirilen Skor Sayısı: " + entryCount);

        for (int i = 0; i < entryCount; i++)
        {
            LeaderboardEntry_t entry;
            // Detayları çek (İsim, Skor vs.)
            SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out entry, null, 0);

            // Kullanıcı Avatarını ve İsmini Al
            int imageID = SteamFriends.GetLargeFriendAvatar(entry.m_steamIDUser);
            Texture2D avatar = GetSteamImageAsTexture2D(imageID);
            string username = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);

            // UI Yarat (Prefabı çoğalt)
            GameObject newRow = Instantiate(rowPrefab, rowContainer);
            LeaderboardRow rowScript = newRow.GetComponent<LeaderboardRow>();

            // UI'ı doldur
            rowScript.SetData(entry.m_nGlobalRank, username, entry.m_nScore, avatar);
        }
    }

    // --- 3. SKOR YÜKLEME ---
    public void UploadScoreToSteam(int score)
    {
        if (currentLeaderboard.m_SteamLeaderboard == 0)
        {
            Debug.LogWarning("Tablo henüz bulunamadı, skor yollanamıyor.");
            return;
        }

        Debug.Log("📤 Skor Yükleniyor: " + score);

        // Sadece rekor kırarsa günceller (KeepBest)
        SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(
            currentLeaderboard,
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
            score,
            null,
            0
        );

        uploadResult.Set(handle);
    }

    private void OnScoreUploaded(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_bSuccess == 1)
        {
            Debug.Log("✅ Skor başarıyla yüklendi! Sıralama değişti mi?: " + (pCallback.m_bScoreChanged == 1));
            // Skor yüklenince listeyi yenile ki kendimizi görelim
            DownloadScores();
        }
        else
        {
            Debug.LogError("❌ Skor yüklenirken hata oluştu.");
        }
    }

    // --- YARDIMCI: STEAM AVATARINI UNITY TEXTURE YAPMA ---
    private Texture2D GetSteamImageAsTexture2D(int iImage)
    {
        Texture2D texture = null;
        uint ImageWidth;
        uint ImageHeight;

        if (SteamUtils.GetImageSize(iImage, out ImageWidth, out ImageHeight))
        {
            byte[] Image = new byte[ImageWidth * ImageHeight * 4];
            if (SteamUtils.GetImageRGBA(iImage, Image, (int)(ImageWidth * ImageHeight * 4)))
            {
                texture = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(Image);
                texture.Apply();
            }
        }
        return texture;
    }
}