using UnityEngine;

public enum UpgradeTier { Common, Rare, Epic, Legendary }
public enum UpgradeCategory { Rune, Skill } // Rün mü, Yetenek mi?
public enum SkillType { Passive, Active }   // Yetenekse: Pasif mi Aktif mi?

[CreateAssetMenu(fileName = "Rün-Yetenek Config", menuName = "RUN-YETENEK/Run-Yetenek Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Genel Bilgiler")]
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;
    public UpgradeTier tier;
    public UpgradeCategory category;

    [Header("Eðer YETENEK ise doldur")]
    public SkillType skillType; // Aktif mi Pasif mi?
    public string skillScriptID; // Örn: "KineticLance", "VoidFlicker" (Script adýyla eþleþmeli)

    [Header("Eðer RÜN ise doldur")]
    public float statValue; // Örn: %10 artýþ için 10 veya 0.1
    public string statName; // Örn: "Size", "Speed", "Vampirism"
}