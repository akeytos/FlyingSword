using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelSettings", menuName = "SwordSlide/Level Settings")]
public class LevelSettings : ScriptableObject
{
    [Header("Dünya Ayarlarý")]
    [Tooltip("Bir zemin parçasýnýn Unity birim cinsinden boyutu (Örn: 50).")]
    public float chunkSize = 50f;

    [Tooltip("Zemin objesinin ObjectPooler'daki etiketi.")]
    public string groundTag = "Ground";

    [Header("Görüþ Mesafesi")]
    [Tooltip("Ýleriye doðru kaç parça önden yüklensin? (Yüksek hýzlar için artýrýn)")]
    [Range(1, 20)] public int forwardDistance = 6;

    [Tooltip("Arkamýzda kaç parça kalsýn?")]
    [Range(1, 10)] public int backwardDistance = 2;

    [Tooltip("Yanlara doðru kaç parça geniþlik olsun?")]
    [Range(0, 5)] public int sideDistance = 1;

    [Header("Optimizasyon (Hysteresis)")]
    [Tooltip("Silme iþlemi, spawn sýnýrýndan kaç chunk sonra yapýlsýn?")]
    [Range(1, 5)] public int despawnBuffer = 2;

    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;
}