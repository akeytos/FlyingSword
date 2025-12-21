using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    [Header("--- AYARLAR ---")]
    public SwordData[] allSwords;       // Kýlýç datalarý
    public Sprite lockSprite;           // Kilit resmi (Sprite)

    [Header("--- 3D GÖRÜNÜM (AKTÝF) ---")]
    public Transform modelPivot;        // Kýlýcýn doðacaðý boþ nokta (Rotator scripti bunda olsun)

    [Header("--- ORTA ALAN (UI) ---")]
    public GameObject lockedPreviewObject; // Kýlýç KÝLÝTLÝYSE çýkacak büyük kilit resmi

    [Header("--- SAÐ TARAF (Bilgiler) ---")]
    public Image mainInfoIconImage;        // "Sword Name" yazýsýnýn solundaki küçük ikon
    public TextMeshProUGUI nameText;       // Kýlýç Ýsmi
    public TextMeshProUGUI descText;       // Kýlýç Açýklamasý

    [Header("--- Skill (Yetenek) ---")]
    public TextMeshProUGUI skillNameText;  // Skill Ýsmi
    public TextMeshProUGUI skillDescText;  // Skill Açýklamasý
    public Image skillIconImage;           // Skill Ýkonu

    [Header("--- Aksesuarlar ---")]
    public Image acc1Image;        // 1. Aksesuarýn Kendi Resmi
    public GameObject acc1Lock;    // 1. Aksesuarýn Kilit Kapaðý

    public Image acc2Image;        // 2. Aksesuarýn Kendi Resmi
    public GameObject acc2Lock;    // 2. Aksesuarýn Kilit Kapaðý

    [Header("--- ALT IZGARA ---")]
    public Button[] gridButtons;   // Aþaðýdaki küçük seçim butonlarý

    // DEÐÝÞKENLER
    private int selectedIndex = 0;
    private GameObject current3DModel; // Sahnede yaratýlan anlýk 3D model

    void Start()
    {
        // Kayýtlý seçimi getir, yoksa 0. kýlýcý seç
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

        // 1. Alt Izgarayý Güncelle
        UpdateGridButtons();

        // =========================================================
        //  >>> 3D MODEL YARATMA KISMI <<<
        // =========================================================

        // Önce sahnede var olan eski modeli temizle
        if (current3DModel != null) Destroy(current3DModel);

        // Kýlýç açýksa ve bir 3D modeli varsa yarat
        if (data.isUnlocked && data.swordPrefab != null)
        {
            current3DModel = Instantiate(data.swordPrefab, modelPivot);

            // Pivotun tam ortasýna yerleþtir
            // current3DModel.transform.localPosition = Vector3.zero;
            // current3DModel.transform.localRotation = Quaternion.identity; 
        }
        // =========================================================

        // KILIÇ AÇIK MI KÝLÝTLÝ MÝ?
        if (data.isUnlocked)
        {
            // --- DURUM 1: KILIÇ AÇIK (UNLOCKED) ---

            // Büyük kilit resmini gizle (yerine 3D model görünecek)
            if (lockedPreviewObject != null) lockedPreviewObject.SetActive(false);

            // Bilgi Ýkonuna kýlýcýn resmini koy
            if (mainInfoIconImage != null) mainInfoIconImage.sprite = data.swordIcon;

            // Yazýlarý doldur
            nameText.text = data.swordName;
            descText.text = data.description;
            skillNameText.text = data.skillName;
            skillDescText.text = data.skillDescription;
            skillIconImage.sprite = data.skillIcon;

            // Aksesuarlarý Ayarla (Varsa göster, yoksa kilit bas)
            SetupAccessory(data.hasAccessory1, data.accessory1Icon, acc1Image, acc1Lock);
            SetupAccessory(data.hasAccessory2, data.accessory2Icon, acc2Image, acc2Lock);
        }
        else
        {
            // --- DURUM 2: KILIÇ KÝLÝTLÝ (LOCKED) ---

            // Büyük kilit resmini aç (3D model zaten yok edildi)
            if (lockedPreviewObject != null) lockedPreviewObject.SetActive(true);

            // Bilgi Ýkonuna kilit resmi koy
            if (mainInfoIconImage != null) mainInfoIconImage.sprite = lockSprite;

            // Yazýlarý gizle
            nameText.text = "???";
            descText.text = "Locked Item";
            skillNameText.text = "???";
            skillDescText.text = "Unknown Skill";
            skillIconImage.sprite = lockSprite;

            // Aksesuarlarý komple kilitle
            LockAccessoryCompletely(acc1Image, acc1Lock);
            LockAccessoryCompletely(acc2Image, acc2Lock);
        }
    }

    // Yardýmcý Fonksiyon: Aksesuar durumunu kontrol eder
    void SetupAccessory(bool hasAcc, Sprite icon, Image imgObj, GameObject lockObj)
    {
        if (hasAcc)
        {
            // Aksesuar bulunduysa: Kilidi aç, ikonu göster
            lockObj.SetActive(false);
            if (imgObj != null)
            {
                imgObj.sprite = icon;
                imgObj.gameObject.SetActive(true);
            }
        }
        else
        {
            // Aksesuar bulunmadýysa: Kilidi kapat VE resme kilit sprite'ý bas
            lockObj.SetActive(true);
            if (imgObj != null)
            {
                imgObj.sprite = lockSprite;
                imgObj.gameObject.SetActive(true);
            }
        }
    }

    // Yardýmcý Fonksiyon: Aksesuarý zorla kilitler (Kýlýç kilitliyken kullanýlýr)
    void LockAccessoryCompletely(Image imgObj, GameObject lockObj)
    {
        if (lockObj != null) lockObj.SetActive(true);
        if (imgObj != null)
        {
            imgObj.sprite = lockSprite;
            imgObj.gameObject.SetActive(true);
        }
    }

    void UpdateGridButtons()
    {
        for (int i = 0; i < gridButtons.Length; i++)
        {
            if (i < allSwords.Length)
            {
                Image btnImage = gridButtons[i].GetComponent<Image>();
                if (allSwords[i].isUnlocked)
                    btnImage.sprite = allSwords[i].swordIcon;
                else
                    btnImage.sprite = lockSprite;
            }
            else
            {
                gridButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // Confirm butonu için
    public void ConfirmSelection()
    {
        if (allSwords[selectedIndex].isUnlocked)
        {
            PlayerPrefs.SetInt("SelectedSword", selectedIndex);
            Debug.Log("Kýlýç Seçildi: " + allSwords[selectedIndex].swordName);

            // Buradan sonra sahne geçiþi yapabilirsin:
            // SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.Log("Bu kýlýç kilitli!");
        }
    }

    // Geri tuþu için
    public void Click_Back()
    {
        // Burada paneli kapatma kodun olabilir
        gameObject.SetActive(false);
    }
}