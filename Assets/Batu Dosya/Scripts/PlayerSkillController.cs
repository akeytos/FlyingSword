using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerSkillController : MonoBehaviour
{
    public static PlayerSkillController Instance;

    [Header("--- YETENEK SLOTLARI ---")]
    public List<UpgradeData> acquiredSkills = new List<UpgradeData>();

    [Header("--- AKTİF YETENEK DURUMU ---")]
    public UpgradeData currentActiveSkill;
    private float cooldownTimer = 0f;
    private bool isSkillActive = false;

    [Header("--- REFERANSLAR ---")]
    public Transform visualModel;
    public GameObject clonePrefab;
    public GameObject kineticLanceVFX;

    [Header("--- KLON İNCE AYAR (BURAYI KULLAN) ---")]
    public Vector3 cloneRotationFix = new Vector3(0, 0, 0); // Klon yamuk doğuyorsa burayı değiştir
    public Vector3 clonePositionOffset = new Vector3(2f, 0, 0); // Klonun konumu yanlışsa burayı değiştir

    // --- MEKANİK DEĞİŞKENLERİ ---
    private float straightMoveTimer = 0f;
    private Vector3 lastVelocityDir;
    private bool isLanceReady = false;
    private List<GameObject> activeClones = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        HandleCooldowns();
        HandleInput();
        HandlePassiveLogic();
    }

    void HandleCooldowns()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    void HandleInput()
    {
        if (currentActiveSkill != null && cooldownTimer <= 0 && !isSkillActive)
        {
            if (Input.GetMouseButtonDown(1)) UseActiveSkill();
        }
    }

    void UseActiveSkill()
    {
        if (currentActiveSkill.skillName == SkillName.VoidFlicker) StartCoroutine(VoidFlickerRoutine());
        else if (currentActiveSkill.skillName == SkillName.CloneEdges) StartCoroutine(CloneEdgesRoutine());

        if (currentActiveSkill != null) cooldownTimer = currentActiveSkill.cooldown;
    }

    void HandlePassiveLogic()
    {
        bool hasKineticLance = false;
        foreach (var skill in acquiredSkills) { if (skill.skillName == SkillName.KineticLance) hasKineticLance = true; }

        if (hasKineticLance)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Unity 6 için linearVelocity (Eski sürümlerde 'velocity' kullan)
                if (rb.linearVelocity.magnitude > 5f && Vector3.Dot(rb.linearVelocity.normalized, lastVelocityDir) > 0.99f)
                {
                    straightMoveTimer += Time.deltaTime;
                    if (straightMoveTimer > 2.0f && !isLanceReady) ActivateLance(true);
                }
                else { straightMoveTimer = 0; ActivateLance(false); }
                lastVelocityDir = rb.linearVelocity.normalized;
            }
        }
    }

    void ActivateLance(bool active)
    {
        isLanceReady = active;
        if (kineticLanceVFX != null) kineticLanceVFX.SetActive(active);
    }

    // --- KRİTİK DÜZELTME BURADA YAPILDI ---
    public bool TryAddSkill(UpgradeData newSkill)
    {
        bool isUpgrade = false;
        foreach (var s in acquiredSkills) { if (s.skillName == newSkill.skillName) isUpgrade = true; }

        // Yer yoksa veya aynı aktif skilli tekrar almaya çalışıyorsak iptal
        if (acquiredSkills.Count >= 3 && !isUpgrade) return false;
        if (newSkill.type == UpgradeType.ActiveSkill && currentActiveSkill != null && currentActiveSkill.skillName != newSkill.skillName) return false;

        if (!isUpgrade)
        {
            acquiredSkills.Add(newSkill);

            if (newSkill.type == UpgradeType.ActiveSkill)
                currentActiveSkill = newSkill;

            // --- HUD GÜNCELLEMESİ (InGameUIManager ile bağlantı) ---
            if (InGameUIManager.Instance != null)
            {
                InGameUIManager.Instance.AddSkillToHUD(newSkill);
            }
            else
            {
                Debug.LogWarning("InGameUIManager sahnede bulunamadı! Slot güncellenemedi.");
            }
        }
        return true;
    }

    // --- SKILL MANTIKLARI ---

    IEnumerator VoidFlickerRoutine()
    {
        isSkillActive = true;
        if (visualModel != null) visualModel.gameObject.SetActive(false);
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;

        Collider[] enemies = Physics.OverlapSphere(transform.position, 15f);
        int hitCount = 0;
        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy") && hitCount < 5)
            {
                transform.position = enemy.transform.position;
                yield return new WaitForSeconds(0.2f);
                hitCount++;
            }
        }
        yield return new WaitForSeconds(0.1f);
        if (visualModel != null) visualModel.gameObject.SetActive(true);
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = true;
        isSkillActive = false;
    }

    IEnumerator CloneEdgesRoutine()
    {
        if (clonePrefab != null)
        {
            // İnce Ayar (Fix) Uygulaması
            Quaternion finalRotation = transform.rotation * Quaternion.Euler(cloneRotationFix);

            // SAĞDAKİ KLON
            GameObject c1 = Instantiate(clonePrefab, transform.position + transform.right * clonePositionOffset.x + transform.up * clonePositionOffset.y, finalRotation);
            c1.SetActive(true);

            // SOLDAKİ KLON
            GameObject c2 = Instantiate(clonePrefab, transform.position - transform.right * clonePositionOffset.x + transform.up * clonePositionOffset.y, finalRotation);
            c2.SetActive(true);

            // Setup (Takip etmesi için)
            if (c1.GetComponent<CloneController>())
                c1.GetComponent<CloneController>().Setup(this.transform, new Vector3(clonePositionOffset.x, clonePositionOffset.y, 0));

            if (c2.GetComponent<CloneController>())
                c2.GetComponent<CloneController>().Setup(this.transform, new Vector3(-clonePositionOffset.x, clonePositionOffset.y, 0));

            activeClones.Add(c1);
            activeClones.Add(c2);
        }

        float duration = (currentActiveSkill != null) ? currentActiveSkill.duration : 3f;
        yield return new WaitForSeconds(duration);

        foreach (var c in activeClones) { if (c != null) Destroy(c); }
        activeClones.Clear();
    }
}