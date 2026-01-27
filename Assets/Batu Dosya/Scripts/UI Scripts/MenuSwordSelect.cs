using UnityEngine;

public class MenuSwordSelect : MonoBehaviour
{
    // Bu fonksiyonu butonlara baðlayacaðýz
    public void SelectSword(int index)
    {
        // 1. Gelen sayýyý (0, 1, 2...) hafýzaya "SelectedSword" adýyla kaydet.
        PlayerPrefs.SetInt("SelectedSword", index);

        // 2. Kaydý garantiye al (Diske yaz).
        PlayerPrefs.Save();

        Debug.Log("Kýlýç Seçildi ve Kaydedildi: " + index);
    }
}