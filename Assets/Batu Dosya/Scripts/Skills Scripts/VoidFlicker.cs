using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoidFlicker : MonoBehaviour
{
    [Header("Ayarlar")]
    public float cooldown = 5f;
    public float jumpRange = 10f;
    public int maxTargets = 5;
    // public float damage = 50f; // BUNA GEREK KALMADI (Tek atýyoruz)

    private float cooldownTimer = 0f;
    private bool isFlickering = false;

    // Referanslar
    private Renderer[] meshRenderers;
    private Collider col;
    private TrailRenderer trail;

    void Start()
    {
        meshRenderers = GetComponentsInChildren<Renderer>();
        col = GetComponent<Collider>();
        trail = GetComponentInChildren<TrailRenderer>();
    }

    void Update()
    {
        // Soðuma süresini say (SwordStats varsa ondan indirim al, yoksa normal say)
        float reduction = SwordStats.Instance != null ? SwordStats.Instance.cooldownReduction : 0f;

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        // Sað Týk ve Hazýr mý?
        if (Input.GetMouseButtonDown(1) && cooldownTimer <= 0 && !isFlickering)
        {
            StartCoroutine(FlickerSequence());
            // Soðumayý baþlat
            cooldownTimer = cooldown * (1f - reduction);
        }
    }

    IEnumerator FlickerSequence()
    {
        // 1. Hedefleri Bul
        Collider[] enemies = Physics.OverlapSphere(transform.position, jumpRange);
        List<Transform> targets = new List<Transform>();

        foreach (var e in enemies)
        {
            if (e.CompareTag("Enemy")) targets.Add(e.transform);
            if (targets.Count >= maxTargets) break;
        }

        if (targets.Count == 0) yield break;

        isFlickering = true;

        // 2. Kýlýcý Gizle ve Dokunulmaz Yap
        ToggleVisuals(false);
        if (col) col.enabled = false;

        // 3. Düþmanlar arasýnda sek
        Vector3 finalPos = transform.position;

        foreach (var target in targets)
        {
            if (target == null) continue;

            // Iþýnlanma
            transform.position = target.position;

            // --- DEÐÝÞEN KISIM BURASI ---
            // EnemyHealth yerine EnemyStats arýyoruz (Çünkü Loot sistemini buna kurduk)
            EnemyStats stats = target.GetComponent<EnemyStats>();

            if (stats != null)
            {
                // 1. XP ve Parasýný düþür
                stats.OnEnemySliced();
            }

            // 2. Düþmaný yok et (Tek Atýþ)
            Destroy(target.gameObject);
            // -----------------------------

            finalPos = target.position;

            yield return new WaitForSeconds(0.1f);
        }

        // 4. Bitiþ
        transform.position = finalPos;
        ToggleVisuals(true);
        if (col) col.enabled = true;
        isFlickering = false;
    }

    void ToggleVisuals(bool state)
    {
        foreach (var r in meshRenderers) r.enabled = state;
    }

    // Yetenek Upgrade edilince burasý çalýþýr
    public void OnLevelUp()
    {
        maxTargets += 1;
        // damage += 10f; // Hasar artýrmaya gerek yok, sayý artýyor zaten
        Debug.Log("Void Flicker Hedef Sayýsý Arttý!");
    }
}