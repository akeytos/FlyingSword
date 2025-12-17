using UnityEngine;

[CreateAssetMenu(fileName = "New Sword", menuName = "SwordSlideUI/Sword Data")]
public class SwordData : ScriptableObject
{
    [Header("1. Genel Bilgiler")]
    public string swordName;             // Kýlýç Adý
    [TextArea] public string description; // Kýlýç hakkýnda yazý
    public Sprite swordIcon;             // UI'da görünecek ikon
    public bool isUnlocked;              // Kilitli mi diye kontrol

    [Header("2. 3D Model")]
    public GameObject swordPrefab;       // Ekranda dönecek olan 3D model

    [Header("3. Skill (Yetenek) Alaný")]
    public string skillName;             // Skill Adý
    [TextArea] public string skillDescription; // Skill Açýklamasý
    public Sprite skillIcon;             // Skill Ýkonu

    [Header("4. Aksesuarlar")]
    public bool hasAccessory1;           // Aksesuar açýk mý?
    public bool hasAccessory2;           // Aksesuar açýk mý?
}