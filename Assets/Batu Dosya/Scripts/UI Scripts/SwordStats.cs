using UnityEngine;
using System.Collections; // Coroutine için gerekli

public class SwordStats : MonoBehaviour
{
    public static SwordStats Instance;

    [Header("Referanslar")]
    public Transform visualModel; // INSPECTOR'DAN KILICIN GÖRSELÝNÝ SÜRÜKLE!
    private SphereCollider rootCollider; // Root üzerindeki çarpýþma alaný

    [Header("Temel Ýstatistikler")]
    public float moveSpeedMultiplier = 1.0f;
    public float sizeMultiplier = 1.0f;
    public int maxHealth = 3;
    public float cooldownReduction = 0f;
    public int vampirismCount = 0;

    private int currentKillsForVampirism = 0;
    private Vector3 originalVisualScale;
    private float originalColliderRadius;

    void Awake()
    {
        Instance = this;
        rootCollider = GetComponent<SphereCollider>();

        // Baþlangýç deðerlerini kaydet
        if (visualModel != null)
            originalVisualScale = visualModel.localScale;

        if (rootCollider != null)
            originalColliderRadius = rootCollider.radius;
    }

    public void ApplyRune(string statName, float value)
    {
        switch (statName)
        {
            case "Speed":
                moveSpeedMultiplier += value;
                // Not: FlightController scriptinde moveSpeed ile bunu çarpman gerekecek
                break;

            case "Size":
                sizeMultiplier += value;
                // Direkt büyütmek yerine yumuþak geçiþ efektini baþlatýyoruz
                StartCoroutine(UpdateSizeSmoothly());
                break;

            case "Health":
                maxHealth += (int)value;
                // PlayerHealth scriptin varsa orayý güncelle
                if (TryGetComponent(out PlayerHealth healthScript))
                {
                    healthScript.maxHealth = maxHealth;
                    healthScript.Heal((int)value);
                }
                break;

            case "Vampirism":
                vampirismCount = (int)value;
                break;

            case "Cooldown":
                cooldownReduction += value;
                break;
        }
        Debug.Log(statName + " Rünü Eklendi! Eklenen Deðer: " + value);
    }

    // --- BÜYÜME EFEKTÝ (JUICE) ---
    IEnumerator UpdateSizeSmoothly()
    {
        if (visualModel == null) yield break;

        // 1. Hedef Deðerleri Hesapla
        Vector3 targetScale = originalVisualScale * sizeMultiplier;
        float targetRadius = originalColliderRadius * sizeMultiplier;

        // 2. Animasyon (Lerp)
        float timer = 0f;
        float duration = 0.3f; // Büyüme ne kadar sürsün?
        Vector3 startScale = visualModel.localScale;
        float startRadius = (rootCollider != null) ? rootCollider.radius : 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Görseli büyüt
            visualModel.localScale = Vector3.Lerp(startScale, targetScale, t);

            // Collider'ý da büyüt (Hitbox artsýn)
            if (rootCollider != null)
                rootCollider.radius = Mathf.Lerp(startRadius, targetRadius, t);

            yield return null;
        }

        // 3. Garanti olsun diye tam deðere eþitle
        visualModel.localScale = targetScale;
        if (rootCollider != null) rootCollider.radius = targetRadius;
    }

    public void OnEnemyKilled()
    {
        if (vampirismCount > 0)
        {
            currentKillsForVampirism++;
            if (currentKillsForVampirism >= vampirismCount)
            {
                if (TryGetComponent(out PlayerHealth healthScript))
                {
                    healthScript.Heal(1);
                }
                currentKillsForVampirism = 0;
            }
        }
    }
}