using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Ölünce Ne Versin?")]
    public float xpValue = 20f;  // Kaç XP?
    public int coinValue = 5;    // Kaç Para?

    // Bu fonksiyonu Düþman kesildiði an çaðýracaðýz
    public void OnEnemySliced()
    {
        // GameManager sahnede var mý kontrol et
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddXP(xpValue);     // XP Ver
            GameManager.Instance.AddCoin(coinValue); // Para Ver
            GameManager.Instance.AddKill();          // Kill Sayýsýný Artýr
        }
        else
        {
            Debug.LogWarning("GameManager bulunamadý! Sahnede GameManager objesi var mý?");
        }

        // NOT: Destroy(gameObject) yapmýyoruz çünkü Slice sistemin 
        // muhtemelen objeyi zaten parçalayýp yok ediyor.
    }
}