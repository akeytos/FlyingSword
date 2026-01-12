using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public string currentLanguage = "en"; // Varsayýlan Ýngilizce
    private Dictionary<string, LocalizationData> dictionary = new Dictionary<string, LocalizationData>();

    // --- YENÝ EKLENEN: DESTEKLENEN DÝLLER LÝSTESÝ ---
    [System.Serializable]
    public class LanguageOption
    {
        public string code; // "tr", "en"
        public string name; // "Türkçe", "English"
    }
    public List<LanguageOption> supportedLanguages = new List<LanguageOption>();
    // ------------------------------------------------

    // "Dil deðiþti" habercisi
    public delegate void LanguageChangeHandler();
    public event LanguageChangeHandler OnLanguageChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --- LÝSTEYÝ BURADA TANIMLIYORUZ ---
            supportedLanguages.Clear();
            supportedLanguages.Add(new LanguageOption { code = "en", name = "English" });
            supportedLanguages.Add(new LanguageOption { code = "tr", name = "Türkçe" });
            // Ýleride Almanca eklemek istersen:
            // supportedLanguages.Add(new LanguageOption { code = "de", name = "Deutsch" });
            // ------------------------------------

            LoadLanguage();
            SetupDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- SÖZLÜK: SENÝN EKLEDÝÐÝN TÜM KELÝMELER BURADA DURUYOR ---
    void SetupDictionary()
    {
        // --- ANA MENÜ ---
        AddWord("play_btn", "OYNA", "PLAY");
        AddWord("inventory_btn", "ENVANTER", "INVENTORY");
        AddWord("shop_btn", "DÜKKAN", "SHOP");
        AddWord("exit_btn", "ÇIKIÞ", "EXIT");
        AddWord("choose_btn", "SEÇÝM", "CHOOSE");

        // --- LEADERBOARD ---
        AddWord("leaderboard_header", "SIRALAMA", "LEADERBOARD");
        AddWord("rank_col", "SIRA", "RANK");
        AddWord("player_col", "OYUNCU", "PLAYER");
        AddWord("kills_col", "LEÞ", "KILLS");

        // --- SETTINGS (AYARLAR) ---
        AddWord("settings_title", "AYARLAR", "SETTINGS");
        AddWord("music_vol", "MÜZÝK", "MUSIC");
        AddWord("sfx_vol", "SES EFEKT", "SFX");
        AddWord("close_btn", "KAPAT", "CLOSE");

        // --- OYUN ÝÇÝ UYARILAR ---
        AddWord("game_over", "OYUN BÝTTÝ", "GAME OVER");
        AddWord("level_up", "SEVÝYE ATLADIN!", "LEVEL UP!");
        AddWord("swarm_alert", "!!! SÜRÜ GELÝYOR !!!", "!!! SWARM INCOMING !!!");

        // --- DEATH SCREEN ---
        AddWord("died", "ÖLDÜN!", "DIED!");
        AddWord("died_desc", "Cesaretin takdire þayandý ama yetmedi.", "Your courage was admirable but not enough.");
        AddWord("confirm", "ONAYLA", "CONFIRM");

        // --- PAUSE MENÜSÜ ---
        AddWord("pause_header", "DURAKLATILDI", "PAUSED");
        AddWord("keep_playing_btn", "DEVAM ET", "KEEP PLAYING");
        AddWord("map_btn", "HARÝTA", "MAP");
        AddWord("restart_btn", "YENÝDEN BAÞLAT", "RESTART");
        AddWord("main_menu_btn", "ANA MENÜ", "MAIN MENU");
        AddWord("settings_btn", "AYARLAR", "SETTINGS");

        // --- PAUSE DETAYLAR ---
        AddWord("weapons_header", "SÝLAHLAR", "WEAPONS");
        AddWord("books_header", "KÝTAPLAR", "BOOKS");
        AddWord("statistic_header", "ÝSTATÝSTÝK", "STATISTIC");
        AddWord("damage_stat", "HASAR", "DAMAGE");
        AddWord("speed_stat", "HIZ", "SPEED");
        AddWord("health_stat", "CAN", "HEALTH");
        AddWord("kills_stat", "LEÞ", "KILLS");
        AddWord("map_name_lbl", "HARÝTA ADI", "MAP NAME");
        AddWord("level_lbl", "SEVÝYE", "LEVEL");
        AddWord("inventory", "ENVANTER", "INVENTORY");

        // --- SWORD SELECTION DETAYLAR --- 
        AddWord("selection_header", "SÝLAH SEÇÝMÝ", "SWORD SELECTION");
        AddWord("swordname", "Kýlýç ismi", "Sword Name");
        AddWord("sworddesc", "Kýlýç açýklamasý", "Sword Desc");
        AddWord("skilltitle", "YETENEKLER", "SKILLS");
        AddWord("accessory", "AKSESUAR", "ACCESSORY");
    }

    void AddWord(string key, string tr, string en)
    {
        LocalizationData data = new LocalizationData { tr = tr, en = en };
        if (!dictionary.ContainsKey(key)) dictionary.Add(key, data);
    }

    public string GetText(string key)
    {
        if (dictionary.ContainsKey(key))
            return (currentLanguage == "tr") ? dictionary[key].tr : dictionary[key].en;
        return key;
    }

    public void ToggleLanguage()
    {
        string newLang = (currentLanguage == "tr") ? "en" : "tr";
        SetLanguage(newLang);
    }

    public void SetLanguage(string langCode)
    {
        currentLanguage = langCode;
        PlayerPrefs.SetString("GameLanguage", langCode);
        PlayerPrefs.Save();
        if (OnLanguageChanged != null) OnLanguageChanged.Invoke();
    }

    void LoadLanguage()
    {
        if (PlayerPrefs.HasKey("GameLanguage"))
            currentLanguage = PlayerPrefs.GetString("GameLanguage");
        else
            currentLanguage = "en";
    }

    class LocalizationData { public string tr; public string en; }
}