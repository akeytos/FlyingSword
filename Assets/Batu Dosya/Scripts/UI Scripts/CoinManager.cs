using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    [Header("--- UI BAÐLANTILARI ---")]
    public TextMeshProUGUI coinText; 


    void Start()
    {
        UpdateCoinUI(); 
    }

 
    public void UpdateCoinUI()
    {
        
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

       
        if (coinText != null)
            coinText.text = currentCoins.ToString();
    }

    public void AddCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        currentCoins += amount;

        // Yeni parayý kaydet
        PlayerPrefs.SetInt("TotalCoins", currentCoins);
        PlayerPrefs.Save();

        // Ekraný güncelle
        UpdateCoinUI();
    }

    public bool SpendCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            PlayerPrefs.SetInt("TotalCoins", currentCoins);
            PlayerPrefs.Save();
            UpdateCoinUI();
            return true; // Satýn alma baþarýlý
        }
        else
        {
            Debug.Log("Yetersiz Bakiye!");
            return false; // Para yetmedi
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddCoins(100);
            Debug.Log("Hile Yapýldý: +100 Coin");
        }
    }
}