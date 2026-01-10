using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("--- AUDIO SOURCES ---")]
    public AudioSource musicSource; // Arkaplan müziði için
    public AudioSource sfxSource;   // Efektler (Coin, XP vs.) için

    [Header("--- SES DOSYALARI (CLIPS) ---")]
    public AudioClip backgroundMusic; // Sadece giriþte 1 kez çalacak müzik
    public AudioClip levelUpSound;    // Level atlama paneli sesi
    public AudioClip coinSound;       // Coin alma sesi
    public AudioClip xpSound;         // XP alma sesi

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Oyunu açýnca müziði 1 KERE çal (Loop kapalý)
        PlayMusicOneShot(backgroundMusic);
    }

    // --- MÜZÝK FONKSÝYONU (DÜZELTÝLDÝ: LOOP YOK) ---
    public void PlayMusicOneShot(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = false; // <--- BURASI DEÐÝÞTÝ: Tekrar etmesin
        musicSource.Play();
    }

    // --- EFEKT FONKSÝYONLARI (AYNI) ---

    public void PlayCoinSFX()
    {
        if (coinSound != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(coinSound);
        }
    }

    public void PlayXpSFX()
    {
        if (xpSound != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(xpSound);
        }
    }

    public void PlayLevelUpSFX()
    {
        if (levelUpSound != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(levelUpSound);
        }
    }
}