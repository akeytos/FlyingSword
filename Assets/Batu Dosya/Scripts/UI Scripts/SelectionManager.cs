using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    [Header("--- AYARLAR ---")]
    public SwordData[] allSwords;      // Oluþturduðun tüm kýlýç datalarýný buraya at
    public Transform modelPivot;       // 3D Kýlýcýn doðacaðý boþ nokta
    public Sprite lockSprite;          // O kýrmýzý yazýlý "Lock" resmi

    [Header("--- UI REFERANSLARI (Sol Taraf) ---")]
    // 3D Model buraya kodla gelecek, UI'da bir þey yapmana gerek yok.

    [Header("--- UI REFERANSLARI (Sað Taraf) ---")]
    public TextMeshProUGUI nameText;       // Sword Name
    public TextMeshProUGUI descText;       // A word about character

    [Header("--- Skill ve Aksesuar ---")]
    public TextMeshProUGUI skillNameText;  // Skill Name
    public TextMeshProUGUI skillDescText;  // Skill Explanation
    public Image skillIconImage;           // Skill Image kutusu
    public GameObject accessory1Lock;      // Aksesuar 1 üzerindeki Lock resmi
    public GameObject accessory2Lock;      // Aksesuar 2 üzerindeki Lock resmi

    [Header("--- ALT IZGARA (Grid) ---")]
    public Button[] gridButtons;           // Aþaðýdaki kare butonlarýn hepsi

    // DEÐÝÞKENLER
    private int selectedIndex = 0;
    private GameObject current3DModel;

    void Start()
    {
        // Kayýtlý kýlýcý getir, yoksa 0 (ilk baþtaki) gelsin
        selectedIndex = PlayerPrefs.GetInt("SelectedSword", 0);
        UpdateScreen();
    }

    public void SelectSword(int index)
    {
        selectedIndex = index;
        UpdateScreen();
    }

    void UpdateScreen()
    {
        SwordData data = allSwords[selectedIndex];

        // 1. YAZILARI GÜNCELLE
        nameText.text = data.swordName;
        descText.text = data.description;
        skillNameText.text = data.skillName;
        skillDescText.text = data.skillDescription;

        // 2. RESÝMLERÝ GÜNCELLE
        skillIconImage.sprite = data.skillIcon;

        // 3. AKSESUAR KÝLÝTLERÝNÝ AYARLA (True ise kilit kalkar)
        accessory1Lock.SetActive(!data.hasAccessory1);
        accessory2Lock.SetActive(!data.hasAccessory2);

        // 4. 3D MODELÝ YARAT
        if (current3DModel != null) Destroy(current3DModel); // Eskisini sil

        if (data.swordPrefab != null)
        {
            // Yeni modeli pivot noktasýnda oluþtur
            current3DModel = Instantiate(data.swordPrefab, modelPivot);
            // Pozisyonu sýfýrla ki tam ortaya gelsin
            current3DModel.transform.localPosition = Vector3.zero;
            current3DModel.transform.localRotation = Quaternion.identity;
        }

        // 5. ALTTAKÝ BUTONLARI GÜNCELLE (Kilitli mi deðil mi?)
        UpdateGridButtons();
    }

    void UpdateGridButtons()
    {
        for (int i = 0; i < gridButtons.Length; i++)
        {
            if (i < allSwords.Length)
            {
                // Butonun içindeki Image'ý al
                Image btnImage = gridButtons[i].GetComponent<Image>();

                if (allSwords[i].isUnlocked)
                {
                    // Açýksa kýlýcýn kendi ikonunu koy
                    btnImage.sprite = allSwords[i].swordIcon;
                }
                else
                {
                    // Kapalýysa Kýrmýzý LOCK resmini koy
                    btnImage.sprite = lockSprite;
                }
            }
            else
            {
                // Eðer data yoksa butonu gizle
                gridButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void ConfirmSelection()
    {
        if (allSwords[selectedIndex].isUnlocked)
        {
            PlayerPrefs.SetInt("SelectedSword", selectedIndex);
            Debug.Log("SEÇÝM KAYDEDÝLDÝ: " + allSwords[selectedIndex].swordName);
            // Burada paneli kapatma kodunu çaðýrabilirsin
        }
        else
        {
            Debug.Log("BU KILIÇ KÝLÝTLÝ! SEÇEMEZSÝN.");
        }
    }
}