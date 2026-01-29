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

        if (((1 << other.gameObject.layer) & sliceableLayer) != 0)
        {
            GameObject target = other.gameObject;
            Vector3 contactPoint = other.ClosestPoint(transform.position);

            EnemyStats stats = target.GetComponent<EnemyStats>();
            if (stats == null) stats = target.GetComponentInParent<EnemyStats>();

            if (stats != null)
            {
                // Canı azalt ve öldü mü kontrol et
                bool isDead = stats.TakeDamage(swordDamage);

                // Ses her türlü çıksın (Vurma sesi)
                if (hitSound != null)
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(hitSound);
                }

                if (!isDead)
                {
                    // --- BURASI NORMAL VURUŞ ---
                    // Sadece kıvılcım (hitVFX) çıkar, kan çıkmaz.
                    if (hitVFX != null)
                        Instantiate(hitVFX, contactPoint, transform.rotation);

                    SwordMasterController swordCtrl = GetComponentInParent<SwordMasterController>();
                    if (swordCtrl != null) swordCtrl.ApplyRecoil(25f);

                    if (useCameraShake && CameraShake.Instance != null)
                        CameraShake.Instance.Shake(0.1f, 0.2f);
                }
                else
                {
                    // --- BURASI ÖLÜM ANI ---
                    // Kan burada çıkacak (DeathSequence içinde)
                    StartCoroutine(DeathSequence(stats, target, contactPoint));
                }
            }
            else
            {
                // Düşman değilse (Kutu vs.) normal efekt
                if (hitVFX != null) Instantiate(hitVFX, contactPoint, transform.rotation);
                SliceTarget(target, contactPoint);
            }
        }
    }

    IEnumerator DeathSequence(EnemyStats stats, GameObject target, Vector3 contactPoint)
    {
        isProcessingKill = true;

        // 1. ZAMANI DURDUR (Hit Stop - Sinematik Etki)
        if (useCameraShake && CameraShake.Instance != null)
            CameraShake.Instance.Shake(hitStopDuration, 0.4f);

        Time.timeScale = 0f;

        // --- 🩸 KAN EFEKTİ SADECE BURADA 🩸 ---
        if (bloodVFX != null)
        {
            // Kanı tam kılıcın değdiği noktada ve kılıcın açısıyla oluşturuyoruz
            GameObject blood = Instantiate(bloodVFX, contactPoint, transform.rotation);

            // Kan 5 saniye sonra silinsin, sahneyi şişirmesin
            Destroy(blood, 5f);
        }
        // -------------------------------------

        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;

        if (stats != null) stats.ResetMaterialsImmediately();

        // Şimdi kesme işlemini yap
        bool kesimBasarili = SliceTarget(target, contactPoint);

        if (!kesimBasarili)
        {
            Debug.LogWarning("⚠️ Kesim başarısız, normal ölüm.");
        }

        if (stats != null) stats.OnEnemySliced();
        else target.SetActive(false);

        isProcessingKill = false;
    }

    // --- MATRIX YÖNTEMİ (KESİN ÇÖZÜM - Boyut Bozulmaz) ---
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
                    Debug.LogError($"🚨 HATA: '{skin.name}' Read/Write kapalı!");
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
        else
        {
            return false;
        }
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

        float volume = collider.bounds.size.x * collider.bounds.size.y * collider.bounds.size.z;
        rb.mass = Mathf.Max(1f, volume * 5f);

        rb.AddExplosionForce(cutForce, collider.bounds.center, 2f);
        rb.AddTorque(Random.insideUnitSphere * 500f);

        rb.maxLinearVelocity = 20f;

        Destroy(slicedObject, 4f);
    }
}