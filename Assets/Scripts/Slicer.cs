using UnityEngine;
using EzySlice;
using System.Collections;

public class SlicerTrigger : MonoBehaviour
{
    [Header("--- KESME AYARLARI ---")]
    public LayerMask sliceableLayer;
    public Material crossSectionMaterial;
    public float cutForce = 500f;
    public float swordDamage = 10f;

    public enum CutAxis { X_Ekseni_Kirmizi, Y_Ekseni_Yesil, Z_Ekseni_Mavi }
    public CutAxis cutPlaneAxis = CutAxis.Y_Ekseni_Yesil;

    [Header("--- HİSSİYAT (JUICE) AYARLARI ---")]
    public GameObject hitVFX;
    public bool useHitStop = true;
    public float hitStopDuration = 0.05f;

    private bool isStopping = false;

    private void OnTriggerEnter(Collider other)
    {
        bool isSliceableLayer = ((1 << other.gameObject.layer) & sliceableLayer) != 0;
        bool isEnemyTag = other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy");

        if (isSliceableLayer || isEnemyTag)
        {
            GameObject target = other.gameObject;
            Vector3 contactPoint = other.ClosestPoint(transform.position);

            // --- CAN VE ZIRH KONTROLÜ ---
            EnemyStats stats = target.GetComponent<EnemyStats>();
            if (stats == null) stats = target.GetComponentInParent<EnemyStats>();

            if (stats != null)
            {
                bool isDead = stats.TakeDamage(swordDamage, false);

                if (!isDead)
                {
                    // Ölmediyse Sekme (Recoil)
                    SwordMasterController swordCtrl = GetComponentInParent<SwordMasterController>();
                    if (swordCtrl != null) swordCtrl.ApplyRecoil(25f);

                    if (hitVFX != null) Instantiate(hitVFX, contactPoint, Quaternion.identity);
                    return;
                }

                // Öldüyse Rengi Düzelt (Beyaz Kafa Fix)
                stats.ResetMaterialsImmediately();
            }

            GameObject sliceRoot = stats != null ? stats.gameObject : other.transform.root.gameObject;

            // --- KESME İŞLEMİ ---
            // Efektler
            if (hitVFX != null) Instantiate(hitVFX, contactPoint, Quaternion.identity);
            if (useHitStop && !isStopping) StartCoroutine(HitStopRoutine());

            // Mesh Filtreleme
            MeshFilter meshFilter = sliceRoot.GetComponentInChildren<MeshFilter>();
            if (meshFilter != null)
            {
                SliceObject(meshFilter.gameObject, contactPoint);
                Destroy(sliceRoot);
                return;
            }

            SkinnedMeshRenderer skinnedMesh = sliceRoot.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh != null)
            {
                SliceCharacter(skinnedMesh, sliceRoot, contactPoint);
                return;
            }
        }
    }

    IEnumerator HitStopRoutine()
    {
        isStopping = true;
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = originalTimeScale;
        isStopping = false;
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

    void SetupSlicedComponent(GameObject slicedObject)
    {
        slicedObject.layer = LayerMask.NameToLayer("Default");
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();

        // --- DEĞİŞİKLİK BURADA: MeshCollider Geri Geldi ---
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true; // Fizik için şart

        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 2f);
        Destroy(slicedObject, 4f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 direction = GetCutNormal();
        Gizmos.DrawRay(transform.position, direction * 1.0f);
        Gizmos.DrawWireSphere(transform.position + direction * 1.0f, 0.05f);
    }
}