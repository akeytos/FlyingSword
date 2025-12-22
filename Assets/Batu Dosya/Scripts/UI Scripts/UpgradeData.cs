using UnityEngine;

// Yetenek Türü: Aktif mi (Sað Týk), Pasif mi (Otomatik)?
public enum UpgradeType
{
    StatBoost,   // Can, Hýz, Hasar artýþý (Eski sistem)
    ActiveSkill, // Void Flicker, Clone Edges (Sað Týk)
    PassiveSkill // Kinetic Lance (Otomatik mekanik)
}

// Hangi Büyü? (Kodun tanýmasý için)
public enum SkillName
{
    None,           // Büyü deðilse (Düz stat artýþýysa)
    KineticLance,
    VoidFlicker,
    CloneEdges
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Sword Slide Skills Data/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("--- GENEL BÝLGÝLER ---")]
    public string upgradeName;       // Ekranda yazacak isim
    [TextArea] public string description; // Açýklama
    public Sprite icon;              // Resim

    [Header("--- TÜR AYARLARI ---")]
    public UpgradeType type;         // Stat mý, Skill mi?
    public SkillName skillName;      // Hangi skill? (Sadece Skill ise seç)

    [Header("--- SKILL AYARLARI (Sadece Skill Ýse) ---")]
    public float cooldown = 5f;      // Bekleme süresi (Void Flicker için)
    public float duration = 3f;      // Etki süresi (Clone Edges için)

    [Header("--- STAT AYARLARI (Sadece Stat Ýse) ---")]
    public float value = 10f;        // Ne kadar artýracak? (Can +1, Hýz +10 vs.)
}