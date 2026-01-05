using UnityEngine;
using System.Collections.Generic;

public class PlayerSkillController : MonoBehaviour
{
    public static PlayerSkillController Instance;

    [Header("--- YETENEK SLOTLARI ---")]
    public List<UpgradeData> acquiredSkills = new List<UpgradeData>();

    [Header("--- AKTİF YETENEK BİLGİSİ ---")]
    public UpgradeData currentActiveSkill;

    void Awake()
    {
        if (Instance == null) Instance = this;

        // 1. Emniyet: Oyun başlarken liste yoksa oluştur.
        if (acquiredSkills == null) acquiredSkills = new List<UpgradeData>();
    }

    public bool TryAddSkill(UpgradeData newSkill)
    {
        // --- 2. EMNİYET (KURŞUN GEÇİRMEZ) ---
        // Eğer liste bir şekilde hala null ise (Unity hatası), TAM ŞU AN oluştur.
        if (acquiredSkills == null)
        {
            acquiredSkills = new List<UpgradeData>();
        }

        // Gelen veri boş mu? (Nolur nolmaz)
        if (newSkill == null)
        {
            Debug.LogError("HATA: TryAddSkill'e boş (NULL) veri geldi!");
            return false;
        }

        // --- ZATEN VAR MI KONTROLÜ ---
        bool isUpgrade = false;

        // Listenin içindeki boşlukları temizleyerek dönelim (Defensive Coding)
        for (int i = 0; i < acquiredSkills.Count; i++)
        {
            if (acquiredSkills[i] == null) continue; // Boş slot varsa atla, hata verme

            if (acquiredSkills[i].skillScriptID == newSkill.skillScriptID)
            {
                isUpgrade = true;
                break; // Bulduk, döngüden çık
            }
        }

        // --- SLOT KONTROLÜ ---
        if (acquiredSkills.Count >= 3 && !isUpgrade)
        {
            Debug.Log("Slotlar dolu!");
            return false;
        }

        // --- ÇAKIŞMA KONTROLÜ ---
        bool isNewActive = (newSkill.category == UpgradeCategory.Skill && newSkill.skillType == SkillType.Active);

        if (isNewActive && currentActiveSkill != null && currentActiveSkill.skillScriptID != newSkill.skillScriptID)
        {
            Debug.LogWarning("Zaten bir aktif yeteneğin var!");
            return false;
        }

        // --- EKLEME İŞLEMİ ---
        if (!isUpgrade)
        {
            acquiredSkills.Add(newSkill);

            if (isNewActive) currentActiveSkill = newSkill;

            // Scripti Bul ve Çalıştır
            MonoBehaviour skillScript = GetComponent(newSkill.skillScriptID) as MonoBehaviour;

            if (skillScript != null)
            {
                skillScript.enabled = true;
                skillScript.SendMessage("OnLevelUp", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogError("HATA: '" + newSkill.skillScriptID + "' isimli script Kılıcın üstünde yok! Ekledin mi?");
            }

            // UI Güncelle
            if (InGameUIManager.Instance != null)
            {
                InGameUIManager.Instance.AddSkillToHUD(newSkill);
            }
        }
        else
        {
            // Upgrade (Seviye Atlatma)
            SendMessageToScript(newSkill.skillScriptID, "OnLevelUp");
        }

        return true;
    }

    void SendMessageToScript(string scriptName, string methodName)
    {
        MonoBehaviour script = GetComponent(scriptName) as MonoBehaviour;
        if (script != null)
        {
            script.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }
    }
}