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
                bool isDead = stats.TakeDamage(swordDamage);

                if (hitSound != null)
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(hitSound);
                }

                if (hitVFX != null)
                    Instantiate(hitVFX, contactPoint, transform.rotation);

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

        bool kesimBasarili = SliceTarget(target, contactPoint);

        if (!kesimBasarili)
        {
            Debug.LogWarning("⚠️ Kesim başarısız, normal ölüm devreye giriyor.");
        }

        if (stats != null) stats.OnEnemySliced();
        else target.SetActive(false);

        isProcessingKill = false;
    }

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

    // --- İŞTE O NÜKLEER ÇÖZÜM BURADA ---
    bool SliceCharacter(SkinnedMeshRenderer skinned, GameObject originalRoot, Vector3 contactPoint)
    {
        Mesh bakedMesh = new Mesh();
        skinned.BakeMesh(bakedMesh);

        // 1. ADIM: Vertex Scaling (Nokta Boyutlandırma)
        // Transform ile uğraşmıyoruz. Direkt Mesh'in noktalarını "Dünya Boyutu" ile çarpıyoruz.
        // Böylece obje Scale(1,1,1) olsa bile içindeki mesh doğru boyutta oluyor.
        Vector3 worldScale = skinned.transform.lossyScale;
        Vector3[] vertices = bakedMesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            // Her noktayı o anki dünya scale'i ile çarpıp yerine koyuyoruz.
            vertices[i] = Vector3.Scale(vertices[i], worldScale);
        }

        bakedMesh.vertices = vertices;
        bakedMesh.RecalculateBounds(); // Kutuyu güncelle
        bakedMesh.RecalculateNormals(); // Işıklandırmayı güncelle

        // 2. ADIM: Geçici Obje Yarat (Scale 1,1,1)
        GameObject tempObj = new GameObject("TempSliceTarget");
        tempObj.transform.position = skinned.transform.position;
        tempObj.transform.rotation = skinned.transform.rotation;
        tempObj.transform.localScale = Vector3.one; // ARTIK BU HEP 1 OLACAK!

        MeshFilter mf = tempObj.AddComponent<MeshFilter>();
        mf.mesh = bakedMesh;
        MeshRenderer mr = tempObj.AddComponent<MeshRenderer>();
        mr.materials = skinned.materials;

        // 3. ADIM: Kes
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
                // Parçalar artık Scale(1,1,1) doğacak çünkü input objemiz (1,1,1) idi.
                // Vertexler zaten büyütüldüğü için görünüm BİREBİR AYNI olacak.
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

        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 2f);
        rb.AddTorque(Random.insideUnitSphere * 500f);

        Destroy(slicedObject, 4f);
    }
}