using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton (Her yerden ulaþmak için)

    [Header("--- OYUN DURUMU ---")]
    public bool isGameActive = true;
    public float gameTime = 0f;

    [Header("--- ÝSTATÝSTÝKLER ---")]
    public int currentLevel = 0;
    public int currentCoins = 0;
    public int currentKills = 0;

    [Header("--- XP AYARLARI ---")]
    public float currentXP = 0f;
    public float maxXP = 100f;        // Ýlk level için gereken XP
    public float xpMultiplier = 1.2f; // Her levelde gereken XP ne kadar artsýn? (%20 artar)

    void Awake()
    {
        // Singleton Kurulumu
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Oyun baþlarken UI'ý sýfýrla
        currentXP = 0;
        currentLevel = 0;
        // Eðer kayýtlý para varsa onu çek, yoksa 0 (Þimdilik 0)
        currentCoins = 0;

        UpdateAllUI();
    }

    void Update()
    {
        if (!isGameActive) return;

        // 1. ZAMANI SAY
        gameTime += Time.deltaTime;

        // UI'a zamaný gönder (Her saniye)
        if (InGameUIManager.Instance != null)
            InGameUIManager.Instance.UpdateTimeUI(gameTime);

        // --- TEST ÝÇÝN (X tuþuna basýnca XP ver) ---
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddXP(20);
            AddCoin(10);
            AddKill();
        }
    }

    // --- DIÞARIDAN ÇAÐRILACAK FONKSÝYONLAR ---

    public void AddXP(float amount)
    {
        currentXP += amount;

        // Level Atladýk mý?
        if (currentXP >= maxXP)
        {
            LevelUp();
        }

        // UI Güncelle
        if (InGameUIManager.Instance != null)
            InGameUIManager.Instance.UpdateLevelUI(currentLevel, currentXP, maxXP);
    }

    void LevelUp()
    {
        // Artan XP'yi bir sonraki levela devret (Örn: 100 lazýmdý, 110 aldýk, 10 cebe kalsýn)
        currentXP -= maxXP;

        currentLevel++;

        // Bir sonraki level daha zor olsun (XP Çubuðunu büyüt)
        maxXP = maxXP * xpMultiplier;

        // Level atlayýnca genelde ses çalar, efekt çýkar vs.
        Debug.Log("LEVEL ATLADIN! Yeni Level: " + currentLevel);

        // >>>>>> YENÝ EKLENEN KISIM <<<<<<
        // LevelUpManager'a haber ver, paneli açsýn
        if (LevelUpManager.Instance != null)
        {
            LevelUpManager.Instance.ShowLevelUpOptions();
        }
        else
        {
            Debug.LogWarning("LevelUpManager sahnede bulunamadý! Scripti bir objeye attýn mý?");
        }
        // --------------------------------

        // EÐER XP HALA YETÝYORSA (Çok büyük XP geldiyse) TEKRAR LEVEL ATLA
        if (currentXP >= maxXP) LevelUp();
    }

    public void AddCoin(int amount)
    {
        currentCoins += amount;
        if (InGameUIManager.Instance != null)
            InGameUIManager.Instance.UpdateCoinUI(currentCoins);
    }

    public void AddKill()
    {
        currentKills++;
        if (InGameUIManager.Instance != null)
            InGameUIManager.Instance.UpdateKillUI(currentKills);
    }

    // UI'ý toplu güncelleme (Baþlangýçta vs.)
    void UpdateAllUI()
    {
        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.UpdateCoinUI(currentCoins);
            InGameUIManager.Instance.UpdateKillUI(currentKills);
            InGameUIManager.Instance.UpdateLevelUI(currentLevel, currentXP, maxXP);
        }
    }
}