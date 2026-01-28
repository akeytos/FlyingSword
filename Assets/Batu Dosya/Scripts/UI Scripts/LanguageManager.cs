using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public string currentLanguage = "en"; // Varsayılan İngilizce
    private Dictionary<string, LocalizationData> dictionary = new Dictionary<string, LocalizationData>();

    // --- DESTEKLENEN DİLLER LİSTESİ ---
    [System.Serializable]
    public class LanguageOption
    {
        public string code; // "tr", "en", "de"
        public string name; // "Türkçe", "English", "Deutsch"
    }
    public List<LanguageOption> supportedLanguages = new List<LanguageOption>();

    // "Dil değişti" habercisi
    public delegate void LanguageChangeHandler();
    public event LanguageChangeHandler OnLanguageChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --- LİSTE TANIMLAMA ---
            supportedLanguages.Clear();
            supportedLanguages.Add(new LanguageOption { code = "en", name = "English" });
            supportedLanguages.Add(new LanguageOption { code = "tr", name = "Türkçe" });
            supportedLanguages.Add(new LanguageOption { code = "de", name = "Deutsch" }); // Almanca eklendi
            
            LoadLanguage();
            SetupDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- SÖZLÜK KURULUMU (TR - EN - DE) ---
    void SetupDictionary()
    {
        // Format: AddWord(KEY, TR, EN, DE);

        // --- ANA MENÜ ---
        AddWord("play_btn", "OYNA", "PLAY", "SPIELEN");
        AddWord("inventory_btn", "ENVANTER", "INVENTORY", "INVENTAR");
        AddWord("shop_btn", "DÜKKAN", "SHOP", "LADEN");
        AddWord("exit_btn", "ÇIKIŞ", "EXIT", "BEENDEN");
        AddWord("choose_btn", "SEÇİM", "CHOOSE", "AUSWÄHLEN");

        // --- LEADERBOARD ---
        AddWord("leaderboard_header", "SIRALAMA", "LEADERBOARD", "RANGLISTE");
        AddWord("rank_col", "SIRA", "RANK", "RANG");
        AddWord("player_col", "OYUNCU", "PLAYER", "SPIELER");
        AddWord("kills_col", "LEŞ", "KILLS", "KILLS");

        // --- SETTINGS (AYARLAR) ---
        AddWord("settings_title", "AYARLAR", "SETTINGS", "EINSTELLUNGEN");
        AddWord("music_vol", "MÜZİK", "MUSIC", "MUSIK");
        AddWord("sfx_vol", "SES EFEKT", "SFX", "EFFEKTE");
        AddWord("close_btn", "KAPAT", "CLOSE", "SCHLIESSEN");

        // --- OYUN İÇİ UYARILAR ---
        AddWord("game_over", "OYUN BİTTİ", "GAME OVER", "SPIEL VORBEI");
        AddWord("level_up", "SEVİYE ATLADIN!", "LEVEL UP!", "STUFE AUFGESTIEGEN!");
        AddWord("swarm_alert", "!!! SÜRÜ GELİYOR !!!", "!!! SWARM INCOMING !!!", "!!! SCHWARM IM ANMARSCH !!!");

        // --- DEATH SCREEN ---
        AddWord("died", "ÖLDÜN!", "DIED!", "GESTORBEN!");
        AddWord("died_desc", "Cesaretin takdire şayandı ama yetmedi.", "Your courage was admirable but not enough.", "Dein Mut war bewundernswert, aber nicht genug.");
        AddWord("confirm", "ONAYLA", "CONFIRM", "BESTÄTIGEN");

        // --- PAUSE MENÜSÜ ---
        AddWord("pause_header", "DURAKLATILDI", "PAUSED", "PAUSIERT");
        AddWord("keep_playing_btn", "DEVAM ET", "KEEP PLAYING", "WEITERSPIELEN");
        AddWord("map_btn", "HARİTA", "MAP", "KARTE");
        AddWord("restart_btn", "YENİDEN BAŞLAT", "RESTART", "NEUSTART");
        AddWord("main_menu_btn", "ANA MENÜ", "MAIN MENU", "HAUPTMENÜ");
        AddWord("settings_btn", "AYARLAR", "SETTINGS", "EINSTELLUNGEN");

        // --- PAUSE DETAYLAR ---
        AddWord("weapons_header", "SİLAHLAR", "WEAPONS", "WAFFEN");
        AddWord("books_header", "KİTAPLAR", "BOOKS", "BÜCHER");
        AddWord("statistic_header", "İSTATİSTİK", "STATISTIC", "STATISTIK");
        AddWord("damage_stat", "HASAR", "DAMAGE", "SCHADEN");
        AddWord("speed_stat", "HIZ", "SPEED", "GESCHWINDIGKEIT");
        AddWord("health_stat", "CAN", "HEALTH", "GESUNDHEIT");
        AddWord("kills_stat", "LEŞ", "KILLS", "KILLS");
        AddWord("map_name_lbl", "HARİTA ADI", "MAP NAME", "KARTENNAME");
        AddWord("level_lbl", "SEVİYE", "LEVEL", "STUFE");
        AddWord("inventory", "ENVANTER", "INVENTORY", "INVENTAR");

        // --- SWORD SELECTION DETAYLAR --- 
        AddWord("selection_header", "SİLAH SEÇİMİ", "SWORD SELECTION", "WAFFENAUSWAHL");
        AddWord("swordname", "Kılıç ismi", "Sword Name", "Schwertname");
        AddWord("sworddesc", "Kılıç açıklaması", "Sword Desc", "Schwertbeschreibung");
        AddWord("skilltitle", "YETENEKLER", "SKILLS", "FÄHIGKEITEN");
        AddWord("accessory", "AKSESUAR", "ACCESSORY", "ZUBEHÖR");

        // --- ABSTRACT (ÖZET) ---
        AddWord("abstract_header", "ÖZET", "ABSTRACT", "ZUSAMMENFASSUNG");
        AddWord("abstract_kills", "LEŞ", "KILLS", "KILLS");
        AddWord("abstract_time", "HAYATTA KALMA SÜRESİ", "SURVIVAL TIME", "ÜBERLEBENSZEIT");
        AddWord("abstract_level", "SEVİYE", "LEVEL", "STUFE");
        AddWord("abstract_coin", "COIN", "COIN", "MÜNZEN");

        // --- NASIL OYNANIR ---
        AddWord("tutorial_controls", "KONTROLLER", "CONTROLS", "STEUERUNG");
        AddWord("tutorial_move", "HAREKET: W,A,S,D veya YÖN TUŞLARI", "MOVE: W,A,S,D or ARROWS", "BEWEGUNG: W,A,S,D oder PFEILTASTEN");
        AddWord("tutorial_aim", "SALDIRI: OTOMATİK", "ATTACK: AUTO", "ANGRIFF: AUTOMATISCH");

        // --- YÜKLENİYOR ---
        AddWord("loading_text", "YÜKLENİYOR...", "LOADING...", "LADEN...");

    }

    // --- ARTIK 3. PARAMETRE OLARAK ALMANCAYI DA ALIYOR ---
    void AddWord(string key, string tr, string en, string de)
    {
        LocalizationData data = new LocalizationData { tr = tr, en = en, de = de };
        if (!dictionary.ContainsKey(key)) 
        {
            dictionary.Add(key, data);
        }
        else
        {
            // Eğer key zaten varsa güncelle (Güvenlik önlemi)
            dictionary[key] = data;
        }
    }

    public string GetText(string key)
    {
        if (dictionary.ContainsKey(key))
        {
            if (currentLanguage == "tr") return dictionary[key].tr;
            if (currentLanguage == "de") return dictionary[key].de; // Almanca kontrolü
            return dictionary[key].en; // Varsayılan EN
        }
        return key;
    }

    public void ToggleLanguage()
    {
        // Toggle artık 3 dil arasında dönecek: EN -> TR -> DE -> EN
        if (currentLanguage == "en") SetLanguage("tr");
        else if (currentLanguage == "tr") SetLanguage("de");
        else SetLanguage("en");
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

    // Veri yapısına 'de' eklendi
    class LocalizationData 
    { 
        public string tr; 
        public string en; 
        public string de; 
    }
}