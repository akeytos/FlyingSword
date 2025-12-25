using UnityEngine;

public class SwordStats : MonoBehaviour
{
    public static SwordStats Instance;

    [Header("Temel Ýstatistikler")]
    public float moveSpeedMultiplier = 1.0f; // Polished Steel bunu artýrýr
    public float sizeMultiplier = 1.0f;      // Giant's Hilt bunu artýrýr
    public int maxHealth = 3;
    public float cooldownReduction = 0f;     // Cooldown rünü
    public int vampirismCount = 0;           // Kaç düþmanda 1 can? (0 = kapalý)

    private int currentKillsForVampirism = 0;
    private Vector3 originalScale;

    void Awake()
    {
        Instance = this;
        originalScale = transform.localScale;
    }

    public void ApplyRune(string statName, float value)
    {
        switch (statName)
        {
            case "Speed":
                moveSpeedMultiplier += value; // Örn: +0.2 (%20 hýz)
                // Kýlýç hareket kodundaki hýzý güncellemen gerekebilir
                break;

            case "Size":
                sizeMultiplier += value;
                transform.localScale = originalScale * sizeMultiplier;
                break;

            case "Health":
                maxHealth += (int)value;
                GetComponent<PlayerHealth>().maxHealth = maxHealth;
                GetComponent<PlayerHealth>().Heal((int)value);
                break;

            case "Vampirism":
                // Deðer ne kadar küçükse o kadar iyi (Örn: Her 50 killde bir)
                // Ama stacklendikçe sayýyý düþürmek istiyorsak mantýðý ona göre kurarýz.
                // Þimdilik: Value = 50 (50 killde 1 can)
                vampirismCount = (int)value;
                break;

            case "Cooldown":
                cooldownReduction += value; // %10 = 0.1
                break;
        }
        Debug.Log(statName + " Rünü Eklendi! Yeni Deðer: " + value);
    }

    // Düþman ölünce burasý çaðrýlacak (Vampiric Rune için)
    public void OnEnemyKilled()
    {
        if (vampirismCount > 0)
        {
            currentKillsForVampirism++;
            if (currentKillsForVampirism >= vampirismCount)
            {
                GetComponent<PlayerHealth>().Heal(1);
                currentKillsForVampirism = 0;
                Debug.Log("Vampiric Rune Tetiklendi: Can Eklendi!");
            }
        }
    }
}