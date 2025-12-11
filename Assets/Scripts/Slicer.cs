using UnityEngine;
using EzySlice;

public class SlicerTrigger : MonoBehaviour
{
    public LayerMask sliceableLayer;
    public Material crossSectionMaterial;
    public float cutForce = 100f;

    // YENÝ ÖZELLÝK: Kesme yönünü buradan seçeceðiz
    public enum CutAxis { X_Ekseni_Kirmizi, Y_Ekseni_Yesil, Z_Ekseni_Mavi }
    [Header("Kesme Ayarlarý")]
    [Tooltip("Kýlýcýn yassý yüzeyi hangi yöne bakýyor?")]
    public CutAxis cutPlaneAxis = CutAxis.Y_Ekseni_Yesil;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & sliceableLayer) != 0)
        {
            GameObject target = other.gameObject;
            Vector3 contactPoint = other.ClosestPoint(transform.position);

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
        // Seçilen eksene göre kesme düzlemini (Normal) belirliyoruz
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

    // Hangi eksenin seçildiðini bulan yardýmcý fonksiyon
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
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1f);
        Destroy(slicedObject, 5f);
    }

    // --- DEBUG GÖRSELLEÞTÝRME ---
    // Scene ekranýnda Sarý bir çizgi göreceksin. Bu çizgi kesme düzlemini gösterir.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 direction = GetCutNormal();

        // Kýlýcýn merkezinden çýkan sarý bir çizgi
        Gizmos.DrawRay(transform.position, direction * 1.0f);

        // Çizginin ucuna küçük bir küre (yönün net anlaþýlmasý için)
        Gizmos.DrawWireSphere(transform.position + direction * 1.0f, 0.05f);
    }
}