using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("--- AUDIO SOURCES ---")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("--- SES DOSYALARI (CLIPS) ---")]
    public AudioClip backgroundMusic;
    public AudioClip levelUpSound;
    public AudioClip coinSound;
    public AudioClip xpSound;
    public AudioClip enemyDeathSound; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        PlayMusicOneShot(backgroundMusic);
    }

    public void PlayMusicOneShot(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = false;
        musicSource.Play();
    }

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

    // --- YENÝ EKLENEN FONKSÝYON ---
    public void PlayEnemyDeathSFX()
    {
        if (enemyDeathSound != null)
        {
            // Ölüm seslerinde pitch'i biraz daha kalýnlaþtýrabiliriz (0.8f - 1.2f)
            // Bu sayede her düþman ayný tonda ölmez, kafa þiþirmez.
            sfxSource.pitch = Random.Range(0.8f, 1.2f);
            sfxSource.PlayOneShot(enemyDeathSound);
        }
    }
}