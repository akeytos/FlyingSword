using UnityEngine;
using Steamworks;

// Bu script Steamworks.NET'in çalýþmasý için zorunludur.
// Standart SteamManager yapýsýdýr.
class SteamManager : MonoBehaviour
{
    private static SteamManager s_instance;
    private static bool s_EverInitialized;

    private bool m_bInitialized;

    public static bool Initialized
    {
        get
        {
            return s_instance.m_bInitialized;
        }
    }

    private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

    private void Awake()
    {
        // Singleton Mantýðý: Sadece 1 tane SteamManager olmalý
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;

        if (s_EverInitialized)
        {
            throw new System.Exception("SteamManager'ý birden fazla kez yaratmaya çalýþtýn!");
        }

        DontDestroyOnLoad(gameObject);

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
        }

        // OYUNU STEAM ÜZERÝNDEN MÝ AÇTIN KONTROLÜ
        // Eðer exe'ye çift týklayýp açtýysan, bu kod oyunu kapatýp Steam üzerinden tekrar açar.
        // Bu sayede Steam özellikleri çalýþýr.
        try
        {
            if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[Steamworks.NET] Steam kütüphanesi bulunamadý! steam_appid.txt dosyasýný kök klasöre koydun mu? Hata: " + e);
            Application.Quit();
            return;
        }

        // STEAM API BAÞLAT
        m_bInitialized = SteamAPI.Init();
        if (!m_bInitialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI.Init() baþarýsýz! Steam açýk mý? AppID doðru mu?");
            return;
        }

        s_EverInitialized = true;
    }

    // Bu kýsým oyun açýk olduðu sürece Steam'den gelen mesajlarý dinler
    private void Update()
    {
        if (!m_bInitialized)
        {
            return;
        }

        // Callback'leri çalýþtýr (Achievement geldi mi? Leaderboard indi mi?)
        SteamAPI.RunCallbacks();
    }

    private void OnDestroy()
    {
        if (s_instance != this)
        {
            return;
        }

        s_instance = null;

        if (!m_bInitialized)
        {
            return;
        }

        // Oyundan çýkýnca Steam baðlantýsýný kes
        SteamAPI.Shutdown();
    }
}