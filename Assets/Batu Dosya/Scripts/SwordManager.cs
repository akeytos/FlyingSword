using UnityEngine;

public class SwordManager : MonoBehaviour
{
    public Transform swordPivot; // Kýlýcýn yaratýlacaðý "El" noktasý
    public GameObject[] swordPrefabs; // Kýlýç modellerinin listesi

    void Start()
    {
        // 1. Hafýzadan seçilen numarayý oku. (Eðer kayýt yoksa varsayýlan 0 olsun)
        int savedIndex = PlayerPrefs.GetInt("SelectedSword", 0);

        // 2. O kýlýcý kuþandýr
        EquipSword(savedIndex);
    }

    public void EquipSword(int index)
    {
        // Güvenlik: Eðer listede olmayan bir sayý gelirse hata vermesin, 0. kýlýcý versin.
        if (index >= swordPrefabs.Length || index < 0) index = 0;

        // A. Eline bak, eski bir kýlýç varsa temizle
        if (swordPivot.childCount > 0)
        {
            foreach (Transform child in swordPivot)
            {
                Destroy(child.gameObject);
            }
        }

        // B. Yeni kýlýcý yarat (Prefab'daki konum ayarlarýyla gelir)
        Instantiate(swordPrefabs[index], swordPivot);
    }
}