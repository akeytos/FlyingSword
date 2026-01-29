using UnityEngine;
using EzySlice;
using System.Collections;

public class SlicerTrigger : MonoBehaviour
{
    [Header("--- KESME AYARLARI ---")]
    public LayerMask sliceableLayer;
    public Material crossSectionMaterial;
    public float cutForce = 1000f;
    public float swordDamage = 50f;

    public enum CutAxis { X_Ekseni_Kirmizi, Y_Ekseni_Yesil, Z_Ekseni_Mavi }
    public CutAxis cutPlaneAxis = CutAxis.Y_Ekseni_Yesil;

    [Header("--- EFEKTLER (JUICE) ---")]
    public GameObject hitVFX;       // Normal vuruşta çıkan (Kıvılcım vs.)
    public GameObject bloodVFX;     // SADECE ÖLÜNCE ÇIKAN (Kan Patlaması) 🩸
    public AudioClip hitSound;
    public float hitStopDuration = 0.1f;

    [Header("--- KAMERA ---")]
    public bool useCameraShake = true;

    private AudioSource audioSource;
    private bool isProcessingKill = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isProcessingKill) return;

        // Kılıç, kesilebilir bir şeye değdi mi?
        if (((1 << other.gameObject.layer) & sliceableLayer) != 0)
        {
            GameObject target = other.gameObject;
            Vector3 contactPoint = other.ClosestPoint(transform.position);

            EnemyStats stats = target.GetComponent<EnemyStats>();
            if (stats == null) stats = target.GetComponentInParent<EnemyStats>();

            if (stats != null)
            {
                // Hasar ver ve öldü mü kontrol et
                bool isDead = stats.TakeDamage(swordDamage);

                // Ses efekti
                if (hitSound != null)
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(hitSound);
                }

                if (!isDead)
                {
                    // --- ÖLMEDİ (NORMAL VURUŞ) ---
                    if (hitVFX != null)
                        Instantiate(hitVFX, contactPoint, transform.rotation);

                    SwordMasterController swordCtrl = GetComponentInParent<SwordMasterController>();
                    if (swordCtrl != null) swordCtrl.ApplyRecoil(25f);

                    if (useCameraShake && CameraShake.Instance != null)
                        CameraShake.Instance.Shake(0.1f, 0.2f);
                }
                else
                {
                    // --- ÖLDÜ! (FATALITY) --- 
                    // Buradaki amaç düşmanı heykel gibi dondurmak.

                    // 1. ANIMATOR'I KAPAT (Titremeyi engeller) 🛑
                    Animator anim = target.GetComponent<Animator>();
                    if (anim != null) anim.enabled = false;

                    // 2. SCRIPT'LERİ KAPAT (Yürümeyi/Saldırmayı engeller)
                    MonoBehaviour[] scripts = target.GetComponents<MonoBehaviour>();
                    foreach (var s in scripts)
                    {
                        // SlicerTrigger ve EnemyStats hariç hepsini kapa
                        if (s != stats && s.GetType().Name != "EnemyStats")
                            s.enabled = false;
                    }

                    // 3. FİZİĞİ DURDUR (Kaymayı engeller)
                    Rigidbody rb = target.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero; // Unity 6 (Eskilerde .velocity)
                        rb.angularVelocity = Vector3.zero;
                        rb.isKinematic = true;
                    }

                    // 4. COLLIDER KAPAT (Kılıç tekrar çarpmasın)
                    Collider col = target.GetComponent<Collider>();
                    if (col != null) col.enabled = false;

                    // Ölüm Sekansını Başlat
                    StartCoroutine(DeathSequence(stats, target, contactPoint));
                }
            }
            else
            {
                // Düşman değil (Kutu, varil vs.)
                if (hitVFX != null) Instantiate(hitVFX, contactPoint, transform.rotation);
                SliceTarget(target, contactPoint);
            }
        }
    }

    IEnumerator DeathSequence(EnemyStats stats, GameObject target, Vector3 contactPoint)
    {
        isProcessingKill = true;

        // 1. ZAMANI DURDUR (Hit Stop)
        if (useCameraShake && CameraShake.Instance != null)
            CameraShake.Instance.Shake(hitStopDuration, 0.4f);

        Time.timeScale = 0f;

        // --- 🩸 KAN EFEKTİ (SADECE BURADA) 🩸 ---
        if (bloodVFX != null)
        {
            GameObject blood = Instantiate(bloodVFX, contactPoint, transform.rotation);
            Destroy(blood, 5f);
        }
        // ----------------------------------------

        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;

        if (stats != null) stats.ResetMaterialsImmediately();

        // KESME İŞLEMİ
        bool kesimBasarili = SliceTarget(target, contactPoint);

        if (!kesimBasarili)
        {
            Debug.LogWarning("⚠️ Kesim başarısız, obje yok ediliyor.");
            target.SetActive(false); // Kesemezsek bile yok et
        }
        else
        {
            if (stats != null) stats.OnEnemySliced();
            else target.SetActive(false);
        }

        isProcessingKill = false;
    }

    // --- MATRIX YÖNTEMİ (KESİN ÇÖZÜM - SCALE BOZULMAZ) ---
    bool SliceTarget(GameObject target, Vector3 contactPoint)
    {
        SkinnedMeshRenderer[] allSkins = target.GetComponentsInChildren<SkinnedMeshRenderer>();
        SkinnedMeshRenderer bestSkin = null;

        if (allSkins != null && allSkins.Length > 0)
        {
            float maxVerts = 0;
            foreach (var skin in allSkins)
            {
                if (!skin.enabled || !skin.gameObject.activeInHierarchy) continue;

                if (skin.sharedMesh != null && !skin.sharedMesh.isReadable)
                {
                    Debug.LogError($"🚨 HATA: '{skin.name}' Read/Write kapalı! Inspector'dan açmalısın.");
                    continue;
                }

                if (skin.sharedMesh != null && skin.sharedMesh.vertexCount > maxVerts)
                {
                    maxVerts = skin.sharedMesh.vertexCount;
                    bestSkin = skin;
                }
            }
        }

        if (bestSkin != null)
        {
            return SliceCharacter(bestSkin, target, contactPoint);
        }

        MeshFilter meshFilter = target.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null)
        {
            if (meshFilter.sharedMesh.isReadable)
                return SliceObject(meshFilter.gameObject, contactPoint);
        }

        return false;
    }

    bool SliceCharacter(SkinnedMeshRenderer skinned, GameObject originalRoot, Vector3 contactPoint)
    {
        Mesh bakedMesh = new Mesh();
        skinned.BakeMesh(bakedMesh);

        // Vertexleri Dünya Koordinatına sabitle (Scale sorunu çözümü)
        Vector3[] verts = bakedMesh.vertices;
        Matrix4x4 localToWorld = skinned.transform.localToWorldMatrix;

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = localToWorld.MultiplyPoint3x4(verts[i]);
        }

        bakedMesh.vertices = verts;
        bakedMesh.RecalculateBounds();
        bakedMesh.RecalculateNormals();

        GameObject tempObj = new GameObject("TempSliceTarget");
        tempObj.transform.position = Vector3.zero;
        tempObj.transform.rotation = Quaternion.identity;
        tempObj.transform.localScale = Vector3.one;

        MeshFilter mf = tempObj.AddComponent<MeshFilter>();
        mf.mesh = bakedMesh;
        MeshRenderer mr = tempObj.AddComponent<MeshRenderer>();
        mr.materials = skinned.materials;

        bool basarili = SliceObject(tempObj, contactPoint);

        Destroy(tempObj);

        if (basarili)
        {
            originalRoot.SetActive(false);
            return true;
        }
        return false;
    }

    public bool SliceObject(GameObject target, Vector3 contactPoint)
    {
        Vector3 cutNormal = GetCutNormal();
        SlicedHull hull = target.Slice(transform.position, cutNormal);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);

            if (upperHull != null && lowerHull != null)
            {
                SetupSlicedComponent(upperHull);
                SetupSlicedComponent(lowerHull);
                return true;
            }
        }
        return false;
    }

    Vector3 GetCutNormal()
    {
        switch (cutPlaneAxis)
        {
            case CutAxis.X_Ekseni_Kirmizi: return transform.right;
            case CutAxis.Y_Ekseni_Yesil: return transform.up;
            case CutAxis.Z_Ekseni_Mavi: return transform.forward;
            default: return transform.up;
        }
    }

    void SetupSlicedComponent(GameObject slicedObject)
    {
        slicedObject.layer = LayerMask.NameToLayer("Default");
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;

        // --- DEĞİŞİKLİK BURADA ---

        // 1. AĞIRLIĞI AZALT: Eskiden hacimle çarpıyorduk, şimdi tüy gibi hafif (1kg) yapıyoruz.
        rb.mass = 1f;

        // 2. PATLAMA GÜCÜNÜ ARTTIR: Inspector'daki cutForce'u kullanıyor.
        // Patlamayı tam objenin göbeğinden veriyoruz.
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 2f);

        // DÖNME EFEKTİ (Bunu da biraz arttırdım, fırıldak gibi dönsün)
        rb.AddTorque(Random.insideUnitSphere * 1000f);

        // 3. HIZ SINIRINI KALDIRDIK: (maxLinearVelocity satırını sildim)
        // Artık roket gibi gidebilirler.

        Destroy(slicedObject, 4f);
    }
}