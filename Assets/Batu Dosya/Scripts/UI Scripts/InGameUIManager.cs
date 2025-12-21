using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUIManager : MonoBehaviour
{
    // Singleton: Oyunun her yerinden "InGameUIManager.Instance..." diye ulaþmak için
    public static InGameUIManager Instance;

    [Header("--- CAN SÝSTEMÝ (KALPLER) ---")]
    public GameObject[] hearts;       // Unity'den Heart_1, Heart_2, Heart_3 buraya sürüklenecek

    [Header("--- ÝSTATÝSTÝKLER (SOL ÜST) ---")]
    public TextMeshProUGUI timeText;  // Süre
    public TextMeshProUGUI coinText;  // Para
    public TextMeshProUGUI killText;  // Kill Sayýsý

    [Header("--- XP BAR & LEVEL (ÜST) ---")]
    public Slider xpSlider;           // Kýrmýzý XP Çubuðu
    public TextMeshProUGUI levelText; // "LVL 0" yazýsý

    [Header("--- YETENEK SLOTLARI (SOL) ---")]
    public Image skill1Icon;          // 1. Kutunun içindeki Ýkon
    public Image skill2Icon;          // 2. Kutunun içindeki Ýkon
    // Ýleride buraya pasif skill slotlarý da eklenebilir

    [Header("--- PANELLER ---")]
    public GameObject levelUpPanel;   // Örs/Level Up ekraný
    public GameObject gameOverPanel;  // Ölüm ekraný

    void Awake()
    {
        // Singleton Kurulumu
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- GÜNCELLEME FONKSÝYONLARI ---

    // 1. CANI GÜNCELLE (KALPLERÝ AÇ/KAPA)
    public void UpdateHealthUI(int currentHealth)
    {
        // Elimizdeki tüm kalpleri kontrol ediyoruz
        for (int i = 0; i < hearts.Length; i++)
        {
            // Eðer döngüdeki sýra, mevcut canýmýzdan küçükse kalbi göster
            if (i < currentHealth)
            {
                hearts[i].SetActive(true);
            }
            else
            {
                // Canýmýz düþtüyse o sýradaki kalbi gizle
                hearts[i].SetActive(false);
            }
        }
    }

    // 2. SÜREYÝ GÜNCELLE (Örn: 65 saniyeyi -> 01:05 yapar)
    public void UpdateTimeUI(float timeInSeconds)
    {
        float minutes = Mathf.FloorToInt(timeInSeconds / 60);
        float seconds = Mathf.FloorToInt(timeInSeconds % 60);

        if (timeText != null)
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // 3. PARAYI GÜNCELLE
    public void UpdateCoinUI(int amount)
    {
        if (coinText != null)
            coinText.text = amount.ToString(); // "000" formatý istersen: amount.ToString("D3")
    }

    // 4. KILL SAYISINI GÜNCELLE
    public void UpdateKillUI(int amount)
    {
        if (killText != null)
            killText.text = amount.ToString();
    }

    // 5. XP VE LEVEL GÜNCELLE
    public void UpdateLevelUI(int level, float currentXP, float maxXP)
    {
        if (levelText != null)
            levelText.text = "LVL : " + level.ToString();

        if (xpSlider != null)
        {
            // Slider deðeri 0 ile 1 arasýndadýr. Oranlayarak buluyoruz.
            float targetValue = currentXP / maxXP;
            xpSlider.value = targetValue;
        }
    }

    // 6. SKILL ÝKONU GÜNCELLE (Yeni skill alýnca çaðýracaðýz)
    public void UpdateSkillIcon(int slotIndex, Sprite icon)
    {
        if (slotIndex == 0 && skill1Icon != null)
        {
            skill1Icon.sprite = icon;
            skill1Icon.color = Color.white; // Baþta þeffafsa görünür yap
        }
        else if (slotIndex == 1 && skill2Icon != null)
        {
            skill2Icon.sprite = icon;
            skill2Icon.color = Color.white;
        }
    }
}