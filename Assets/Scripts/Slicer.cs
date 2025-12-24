using UnityEngine;
using EzySlice;
using System.Collections;

public class SlicerTrigger : MonoBehaviour
{
    [Header("--- KESME AYARLARI ---")]
    public LayerMask sliceableLayer;
    public Material crossSectionMaterial;
    public float cutForce = 500f; // Fýrlatma gücünü artýrdým (Daha iyi daðýlsýnlar)

    // Kesme yönünü buradan seçeceðiz
    public enum CutAxis { X_Ekseni_Kirmizi, Y_Ekseni_Yesil, Z_Ekseni_Mavi }
    public CutAxis cutPlaneAxis = CutAxis.Y_Ekseni_Yesil;

    [Header("--- HÝSSÝYAT (JUICE) AYARLARI ---")]
    public GameObject hitVFX;        // Kan veya Kývýlcým Prefabý (Buraya sürükle)
    public bool useHitStop = true;   // Zaman dondurma olsun mu?
    public float hitStopDuration = 0.05f; // Ne kadar süre donsun? (0.05 - 0.1 idealdir)

    // Anlýk çok fazla HitStop olmasýn diye kontrol
    private bool isStopping = false;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Kesilebilir katman mý?
        if (((1 << other.gameObject.layer) & sliceableLayer) != 0)
        {
            GameObject target = other.gameObject;
            Vector3 contactPoint = other.ClosestPoint(transform.position);

            // --- HÝSSÝYAT EFEKTLERÝ ---
            // 1. Kan/Efekt Çýkar
            if (hitVFX != null)
            {
                Instantiate(hitVFX, contactPoint, Quaternion.identity);
            }

            // 2. Zamaný Dondur (Vuruþ Tokluðu)
            if (useHitStop && !isStopping)
            {
                StartCoroutine(HitStopRoutine());
            }
            // ---------------------------

            // --- XP VE PARA SÝSTEMÝ ---
            EnemyStats stats = target.GetComponent<EnemyStats>();
            if (stats == null) stats = target.GetComponentInParent<EnemyStats>();

            if (stats != null)
            {
                stats.OnEnemySliced();
            }
            // ---------------------------

            // --- KESME ÝÞLEMÝ ---
            MeshFilter meshFilter = target.GetComponentInChildren<MeshFilter>();
            if (meshFilter != null)
            {
                SliceObject(meshFilter.gameObject, contactPoint);
                Destroy(target);
                return;
            }

            SkinnedMeshRenderer skinnedMesh = target.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh != null)
            {
                SliceCharacter(skinnedMesh, target, contactPoint);
                return;
            }
        }
    }

    // --- ZAMANI DONDURMA (HIT STOP) ---
    IEnumerator HitStopRoutine()
    {
        isStopping = true;

        // Zamaný durdur
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Gerçek dünyada bekle (Oyun zamaný durduðu için WaitForSeconds çalýþmaz)
        yield return new WaitForSecondsRealtime(hitStopDuration);

        // Zamaný geri getir
        Time.timeScale = originalTimeScale;
        isStopping = false;
    }

    // --- (Diðer fonksiyonlar aynen duruyor) ---
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
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;

        // Parçalar daha sert fýrlasýn diye gücü kullandýk
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 2f);

        Destroy(slicedObject, 4f); // 4 saniye sonra parçalar kaybolsun (Performans için)
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 direction = GetCutNormal();
        Gizmos.DrawRay(transform.position, direction * 1.0f);
        Gizmos.DrawWireSphere(transform.position + direction * 1.0f, 0.05f);
    }
}