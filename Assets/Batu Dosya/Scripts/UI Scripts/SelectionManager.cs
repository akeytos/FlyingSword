using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SelectionManager : MonoBehaviour
{
    [Header("--- AYARLAR ---")]
    public string gameSceneName = "GameScene";

    // Scriptable Object Listesi
    public SwordData[] allSwords;

    public Sprite lockSprite;

    [Header("--- 3D GÖRÜNÜM ---")]
    public Transform modelPivot;

    [Header("--- ORTA ALAN (UI) ---")]
    public GameObject previewScreen; // Preview_BG (RawImage) buraya
    public GameObject lockedOverlay; // Kilit Resmi (Panel/Image) buraya

    [Header("--- SAĞ TARAF (Bilgiler) ---")]
    public Image mainInfoIconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    [Header("--- Skill (Yetenek) ---")]
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescText;
    public Image skillIconImage;

    [Header("--- Aksesuarlar ---")]
    public Image acc1Image;
    public GameObject acc1Lock;
    public Image acc2Image;
    public GameObject acc2Lock;

    [Header("--- ALT IZGARA ---")]
    public Button[] gridButtons;

    // DEĞİŞKENLER
    private int selectedIndex = 0;
    private GameObject current3DModel;

    void Start()
    {
        // Hafızadaki seçimi getir
        selectedIndex = PlayerPrefs.GetInt("SelectedSword", 0);

        // Hata önleyici: Eğer hafızadaki sayı listeyi aşıyorsa 0'a çek
        if (selectedIndex >= allSwords.Length) selectedIndex = 0;

        UpdateScreen();
    }

    public void SelectSword(int index)
    {
        selectedIndex = index;
        UpdateScreen();
    }

    void UpdateScreen()
    {
        // 1. Güvenlik Kontrolü: Liste boş mu?
        if (allSwords == null || allSwords.Length == 0)
        {
            Debug.LogError("HATA: 'All Swords' listesi boş! Manager objesine Data dosyalarını sürükle.");
            return;
        }

        SwordData data = allSwords[selectedIndex];

        // Izgarayı güncelle
        UpdateGridButtons();

        // ---------------------------------------------------------
        // 2. 3D MODEL YARATMA (Spawn) İŞLEMİ
        // ---------------------------------------------------------

        // Önce eski modeli temizle
        if (current3DModel != null) Destroy(current3DModel);

        // Model Pivot atanmış mı kontrol et
        if (modelPivot == null)
        {
            Debug.LogError("HATA: 'Model Pivot' kutusu boş! Sahnedeki SwordPivot objesini sürükle.");
            return;
        }

        // Eğer kılıç AÇIKSA ve PREFAB VARSA yarat
        if (data.isUnlocked)
        {
            if (data.swordPrefab != null)
            {
                current3DModel = Instantiate(data.swordPrefab, modelPivot);
                current3DModel.transform.localPosition = Vector3.zero;
                current3DModel.transform.localRotation = Quaternion.identity;

                // Ölçek sorunu varsa burayı açıp 100f yapabilirsin:
                // current3DModel.transform.localScale = Vector3.one; 

                // Layer (Katman) düzeltmesi (Preview Kamera görsün diye)
                SetLayerRecursively(current3DModel, modelPivot.gameObject.layer);
            }
            else
            {
                Debug.LogWarning("UYARI: Bu kılıcın ('" + data.swordName + "') Prefab kutusu boş!");
            }
        }
        // ---------------------------------------------------------

        // 3. UI GÜNCELLEME (Ekran vs Kilit)
        if (data.isUnlocked)
        {
            // --- KILIÇ AÇIK ---
            if (previewScreen != null) previewScreen.SetActive(true);   // Arka planı AÇ
            if (lockedOverlay != null) lockedOverlay.SetActive(false);  // Kilidi KAPAT

            // Yazıları doldur
            if (mainInfoIconImage != null) mainInfoIconImage.sprite = data.swordIcon;
            if (nameText) nameText.text = data.swordName;
            if (descText) descText.text = data.description;

            // Skill
            if (skillNameText) skillNameText.text = data.skillName;
            if (skillDescText) skillDescText.text = data.skillDescription;
            if (skillIconImage) skillIconImage.sprite = data.skillIcon;

            // Aksesuarlar
            SetupAccessory(data.hasAccessory1, data.accessory1Icon, acc1Image, acc1Lock);
            SetupAccessory(data.hasAccessory2, data.accessory2Icon, acc2Image, acc2Lock);
        }
        else
        {
            // --- KILIÇ KİLİTLİ ---
            if (previewScreen != null) previewScreen.SetActive(false);  // Arka planı KAPAT
            if (lockedOverlay != null) lockedOverlay.SetActive(true);   // Kilidi AÇ

            if (mainInfoIconImage != null) mainInfoIconImage.sprite = lockSprite;
            if (nameText) nameText.text = "???";
            if (descText) descText.text = "Bu eşya henüz açılmadı.";

            // Skill Gizle
            if (skillNameText) skillNameText.text = "???";
            if (skillDescText) skillDescText.text = "???";
            if (skillIconImage) skillIconImage.sprite = lockSprite;

            LockAccessoryCompletely(acc1Image, acc1Lock);
            LockAccessoryCompletely(acc2Image, acc2Lock);
        }
    }

    // --- YARDIMCI FONKSİYONLAR ---

    void SetupAccessory(bool hasAcc, Sprite icon, Image imgObj, GameObject lockObj)
    {
        if (imgObj == null || lockObj == null) return;

        if (hasAcc)
        {
            lockObj.SetActive(false);
            imgObj.sprite = icon;
            imgObj.gameObject.SetActive(true);
        }
        else
        {
            lockObj.SetActive(true);
            imgObj.sprite = lockSprite;
            imgObj.gameObject.SetActive(true);
        }
    }

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
                if (btnImage != null)
                {
                    if (allSwords[i].isUnlocked)
                        btnImage.sprite = allSwords[i].swordIcon;
                    else
                        btnImage.sprite = lockSprite;
                }
                gridButtons[i].gameObject.SetActive(true);
            }
            else
            {
                gridButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void ConfirmSelection()
    {
        if (allSwords.Length > selectedIndex && allSwords[selectedIndex].isUnlocked)
        {
            PlayerPrefs.SetInt("SelectedSword", selectedIndex);
            PlayerPrefs.Save();
            Debug.Log("Kılıç Kaydedildi: " + allSwords[selectedIndex].swordName);
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.Log("Bu kılıç kilitli!");
        }
    }

    // Layer Düzeltici (Gri ekran sorununu çözer)
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}