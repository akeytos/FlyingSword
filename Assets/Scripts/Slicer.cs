using UnityEngine;
using EzySlice;

public class Slicer : MonoBehaviour
{
    public LayerMask sliceableLayer;
    public Material crossSectionMaterial;
    public float cutForce = 100f;

    void FixedUpdate()
    {
        // Kýlýcýn ucundan ileriye doðru hayali bir çizgi (Ray) çekiyoruz.
        // Artýk script kýlýcýn üzerinde olduðu için kýlýç nereye dönerse Ray oraya gider.

        // Debug Lazer: Oyunda Scene penceresinde kýrmýzý çizgi görmeni saðlar
        Debug.DrawRay(transform.position, transform.forward * 2f, Color.red);

        // Raycast atýyoruz
        bool hasHit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 2.0f, sliceableLayer);

        if (hasHit)
        {
            GameObject target = hit.transform.gameObject;

            // --- KESME MANTIÐI ---

            // 1. MeshFilter var mý? (Kutu vb.)
            MeshFilter meshFilter = target.GetComponentInChildren<MeshFilter>();
            if (meshFilter != null)
            {
                SliceObject(meshFilter.gameObject, hit.point);
                Destroy(target); // Ana objeyi sil
                return;
            }

            // 2. Karakter mi? (SkinnedMesh)
            SkinnedMeshRenderer skinnedMesh = target.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh != null)
            {
                SliceCharacter(skinnedMesh, target, hit.point);
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
        // Kýlýcýn "Yukarý" yönünü kesme düzlemi olarak kullan
        SlicedHull hull = target.Slice(transform.position, transform.up);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);

            SetupSlicedComponent(upperHull);
            SetupSlicedComponent(lowerHull);
        }
    }

    void SetupSlicedComponent(GameObject slicedObject)
    {
        slicedObject.layer = LayerMask.NameToLayer("Default");

        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        BoxCollider collider = slicedObject.AddComponent<BoxCollider>(); // Performans için BoxCollider

        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1f);

        Destroy(slicedObject, 5f); // 5 saniye sonra silinsin
    }
}