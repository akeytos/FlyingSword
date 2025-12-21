using UnityEngine;

public enum UpgradeType
{
    HealthBuff,    // Can arttýrma
    SpeedBuff,     // Hýz arttýrma
    DamageBuff,    // Hasar arttýrma
    CoinMagnet     // Para toplama alaný (örnek)
}

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Sword Slide/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("--- GÖRSEL ---")]
    public string upgradeName;       // Ekranda çýkacak isim (Örn: "Hýzlý Ayaklar")
    [TextArea]
    public string description;       // Açýklama (Örn: "%10 Hýz Kazandýrýr")
    public Sprite icon;              // Kartýn üzerindeki resim

    [Header("--- ETKÝ ---")]
    public UpgradeType type;         // Bu kart ne iþe yarýyor?
    public float value;              // Ne kadar etkiliyor? (Örn: 10, 50, 5.5 vs.)
}