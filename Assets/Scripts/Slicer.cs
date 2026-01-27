using UnityEngine;
using EzySlice;
using System.Collections;

public class SlicerTrigger : MonoBehaviour
{
    [Header("--- KESME AYARLARI ---")]
    public LayerMask sliceableLayer;
    public Material crossSectionMaterial;
    public float cutForce = 1000f; // Parçalar sert fırlasın diye arttırdım
    public float swordDamage = 50f;

    public enum CutAxis { X_Ekseni_Kirmizi, Y_Ekseni_Yesil, Z_Ekseni_Mavi }
    public CutAxis cutPlaneAxis = CutAxis.Y_Ekseni_Yesil;

    [Header("--- HİSSİYAT (JUICE) ---")]
    public GameObject hitVFX;
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
                // Hasar ver
                bool isDead = stats.TakeDamage(swordDamage);

                // --- GÜNCELLENEN KISIM: SES & VFX ---
                if (hitSound != null)
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(hitSound);
                }

                // ARTIK VFX KILICIN ROTASYONUNA GÖRE ÇIKIYOR!
                if (hitVFX != null)
                {
                    Instantiate(hitVFX, contactPoint, transform.rotation);
                }

                if (!isDead)
                {
                    SwordMasterController swordCtrl = GetComponentInParent<SwordMasterController>();
                    if (swordCtrl != null) swordCtrl.ApplyRecoil(25f);

                    if (useCameraShake && CameraShake.Instance != null)
                        CameraShake.Instance.Shake(0.1f, 0.2f);
                }
                else
                {
                    StartCoroutine(DeathSequence(stats, target, contactPoint));
                }
            }
            else
            {
                // Kutu vs. gibi stat'ı olmayan objeler için de VFX yönü düzeltildi
                if (hitVFX != null) Instantiate(hitVFX, contactPoint, transform.rotation);
                SliceTarget(target, contactPoint);
            }
        }
    }

    IEnumerator DeathSequence(EnemyStats stats, GameObject target, Vector3 contactPoint)
    {
        isProcessingKill = true;

        if (useCameraShake && CameraShake.Instance != null)
            CameraShake.Instance.Shake(hitStopDuration, 0.4f);

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;

        if (stats != null) stats.ResetMaterialsImmediately();
        SliceTarget(target, contactPoint);

        if (stats != null) stats.OnEnemySliced();
        else Destroy(target);

        isProcessingKill = false;
    }

    void SliceTarget(GameObject target, Vector3 contactPoint)
    {
        MeshFilter meshFilter = target.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null)
        {
            SliceObject(meshFilter.gameObject, contactPoint);
            return;
        }

        SkinnedMeshRenderer skinnedMesh = target.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMesh != null)
        {
            SliceCharacter(skinnedMesh, target, contactPoint);
            return;
        }

        Debug.LogError("HATA: Kesilecek parçada MeshFilter veya SkinnedMeshRenderer bulunamadı!");
    }

    void SliceCharacter(SkinnedMeshRenderer skinned, GameObject originalRoot, Vector3 contactPoint)
    {
        Mesh bakedMesh = new Mesh();
        skinned.BakeMesh(bakedMesh);
        GameObject tempObj = new GameObject("TempSliceTarget");
        tempObj.transform.position = skinned.transform.position;
        tempObj.transform.rotation = skinned.transform.rotation;
        tempObj.transform.localScale = skinned.transform.localScale;

        MeshFilter mf = tempObj.AddComponent<MeshFilter>();
        mf.mesh = bakedMesh;
        MeshRenderer mr = tempObj.AddComponent<MeshRenderer>();
        mr.materials = skinned.materials;

        SliceObject(tempObj, contactPoint);
        Destroy(originalRoot);
        Destroy(tempObj);
    }

    public void SliceObject(GameObject target, Vector3 contactPoint)
    {
        Vector3 cutNormal = GetCutNormal();
        SlicedHull hull = target.Slice(transform.position, cutNormal);
        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);

            SetupSlicedComponent(upperHull);
            SetupSlicedComponent(lowerHull);
        }
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

    // --- GÜNCELLENEN KISIM: PARÇALARA FİZİK VERME ---
    void SetupSlicedComponent(GameObject slicedObject)
    {
        slicedObject.layer = LayerMask.NameToLayer("Default");
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;

        // 1. PATLAMA: Dışa doğru fırlat
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 2f);

        // 2. DÖNME (KAOS): Parçalar fırıl fırıl dönsün!
        rb.AddTorque(Random.insideUnitSphere * 500f);

        Destroy(slicedObject, 4f);
    }
}