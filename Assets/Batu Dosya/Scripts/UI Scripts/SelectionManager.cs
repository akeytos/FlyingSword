using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    [Header("--- AYARLAR ---")]
    public SwordData[] allSwords;       // K�l�� datalar�
    public Sprite lockSprite;           // Kilit resmi (Sprite)

    [Header("--- 3D G�R�N�M (AKT�F) ---")]
    public Transform modelPivot;        // K�l�c�n do�aca�� bo� nokta (Rotator scripti bunda olsun)

    [Header("--- ORTA ALAN (UI) ---")]
    public GameObject lockedPreviewObject; // K�l�� K�L�TL�YSE ��kacak b�y�k kilit resmi

    [Header("--- SA� TARAF (Bilgiler) ---")]
    public Image mainInfoIconImage;        // "Sword Name" yaz�s�n�n solundaki k���k ikon
    public TextMeshProUGUI nameText;       // K�l�� �smi
    public TextMeshProUGUI descText;       // K�l�� A��klamas�

    [Header("--- Skill (Yetenek) ---")]
    public TextMeshProUGUI skillNameText;  // Skill �smi
    public TextMeshProUGUI skillDescText;  // Skill A��klamas�
    public Image skillIconImage;           // Skill �konu

    [Header("--- Aksesuarlar ---")]
    public Image acc1Image;        // 1. Aksesuar�n Kendi Resmi
    public GameObject acc1Lock;    // 1. Aksesuar�n Kilit Kapa��

    public Image acc2Image;        // 2. Aksesuar�n Kendi Resmi
    public GameObject acc2Lock;    // 2. Aksesuar�n Kilit Kapa��

    [Header("--- ALT IZGARA ---")]
    public Button[] gridButtons;   // A�a��daki k���k se�im butonlar�

    // DE���KENLER
    private int selectedIndex = 0;
    private GameObject current3DModel; // Sahnede yarat�lan anl�k 3D model

    void Start()
    {
        // Kay�tl� se�imi getir, yoksa 0. k�l�c� se�
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

        // 1. Alt Izgaray� G�ncelle
        UpdateGridButtons();

        // =========================================================
        //  >>> 3D MODEL YARATMA KISMI <<<
        // =========================================================

        // �nce sahnede var olan eski modeli temizle
        if (current3DModel != null) Destroy(current3DModel);

        // K�l�� a��ksa ve bir 3D modeli varsa yarat
        if (data.isUnlocked && data.swordPrefab != null)
        {
            current3DModel = Instantiate(data.swordPrefab, modelPivot);

            // Pivotun tam ortas�na yerle�tir
            // current3DModel.transform.localPosition = Vector3.zero;
            // current3DModel.transform.localRotation = Quaternion.identity; 
        }
        // =========================================================

        // KILI� A�IK MI K�L�TL� M�?
        if (data.isUnlocked)
        {
            // --- DURUM 1: KILI� A�IK (UNLOCKED) ---

            // Preview bir RawImage ise kapatma (3D preview kaybolmasin)
            if (lockedPreviewObject != null)
            {
                if (!lockedPreviewObject.TryGetComponent<RawImage>(out _))
                    lockedPreviewObject.SetActive(false);
                else
                    lockedPreviewObject.SetActive(true);
            }

            // Bilgi �konuna k�l�c�n resmini koy
            if (mainInfoIconImage != null) mainInfoIconImage.sprite = data.swordIcon;

            // Yaz�lar� doldur
            nameText.text = data.swordName;
            descText.text = data.description;
            skillNameText.text = data.skillName;
            skillDescText.text = data.skillDescription;
            skillIconImage.sprite = data.skillIcon;

            // Aksesuarlar� Ayarla (Varsa g�ster, yoksa kilit bas)
            SetupAccessory(data.hasAccessory1, data.accessory1Icon, acc1Image, acc1Lock);
            SetupAccessory(data.hasAccessory2, data.accessory2Icon, acc2Image, acc2Lock);
        }
        else
        {
            // --- DURUM 2: KILI� K�L�TL� (LOCKED) ---

            // B�y�k kilit resmini a� (3D model zaten yok edildi)
            if (lockedPreviewObject != null) lockedPreviewObject.SetActive(true);

            // Bilgi �konuna kilit resmi koy
            if (mainInfoIconImage != null) mainInfoIconImage.sprite = lockSprite;

            // Yaz�lar� gizle
            nameText.text = "???";
            descText.text = "Locked Item";
            skillNameText.text = "???";
            skillDescText.text = "Unknown Skill";
            skillIconImage.sprite = lockSprite;

            // Aksesuarlar� komple kilitle
            LockAccessoryCompletely(acc1Image, acc1Lock);
            LockAccessoryCompletely(acc2Image, acc2Lock);
        }
    }

    // Yard�mc� Fonksiyon: Aksesuar durumunu kontrol eder
    void SetupAccessory(bool hasAcc, Sprite icon, Image imgObj, GameObject lockObj)
    {
        if (hasAcc)
        {
            // Aksesuar bulunduysa: Kilidi a�, ikonu g�ster
            lockObj.SetActive(false);
            if (imgObj != null)
            {
                imgObj.sprite = icon;
                imgObj.gameObject.SetActive(true);
            }
        }
        else
        {
            // Aksesuar bulunmad�ysa: Kilidi kapat VE resme kilit sprite'� bas
            lockObj.SetActive(true);
            if (imgObj != null)
            {
                imgObj.sprite = lockSprite;
                imgObj.gameObject.SetActive(true);
            }
        }
    }

    // Yard�mc� Fonksiyon: Aksesuar� zorla kilitler (K�l�� kilitliyken kullan�l�r)
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

    // Confirm butonu i�in
    public void ConfirmSelection()
    {
        if (allSwords[selectedIndex].isUnlocked)
        {
            PlayerPrefs.SetInt("SelectedSword", selectedIndex);
            Debug.Log("K�l�� Se�ildi: " + allSwords[selectedIndex].swordName);

            // Buradan sonra sahne ge�i�i yapabilirsin:
            // SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.Log("Bu k�l�� kilitli!");
        }
    }

    // Geri tu�u i�in
    public void Click_Back()
    {
        // Burada paneli kapatma kodun olabilir
        gameObject.SetActive(false);
    }
}