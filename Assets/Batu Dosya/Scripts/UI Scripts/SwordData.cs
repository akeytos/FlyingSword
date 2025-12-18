using UnityEngine;

[CreateAssetMenu(fileName = "New Sword", menuName = "Game/Sword Data")]
public class SwordData : ScriptableObject
{
    [Header("1. Genel Bilgiler")]
    public string swordName;
    [TextArea] public string description;
    public Sprite swordIcon;
    public bool isUnlocked;

    // 3D Model (Þimdilik dursun)
    public GameObject swordPrefab;

    [Header("3. Skill (Yetenek)")]
    public string skillName;
    [TextArea] public string skillDescription;
    public Sprite skillIcon;

    [Header("4. Aksesuarlar")]
    public bool hasAccessory1;      // Açýk mý?
    public Sprite accessory1Icon;   // 1. Aksesuarýn resmi ne?

    public bool hasAccessory2;      // Açýk mý?
    public Sprite accessory2Icon;   // 2. Aksesuarýn resmi ne?
}