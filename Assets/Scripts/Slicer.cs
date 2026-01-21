using UnityEngine;
using EzySlice;
using System.Collections;

public class SlicerTrigger : MonoBehaviour
{
    [Header("--- KESME AYARLARI ---")]
    public LayerMask sliceableLayer;
    public Material crossSectionMaterial;
    public float cutForce = 500f;
    public float swordDamage = 50f; // Tek atması için yüksek verdim

    public enum CutAxis { X_Ekseni_Kirmizi, Y_Ekseni_Yesil, Z_Ekseni_Mavi }
    public CutAxis cutPlaneAxis = CutAxis.Y_Ekseni_Yesil;

    [Header("--- HİSSİYAT (JUICE) ---")]
    public GameObject hitVFX;
    public AudioClip hitSound;
    public float hitStopDuration = 0.1f; // Donma süresi (0.1 - 0.15 iyidir)

    [Header("--- KAMERA ---")]
    public bool useCameraShake = true;

    private AudioSource audioSource;
    private bool isProcessingKill = false; // Aynı anda 2 kere kesmesin diye koruma

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isProcessingKill) return; // Zaten birini kesiyorsak karışma

        if (((1 << other.gameObject.layer) & sliceableLayer) != 0)
        {
            GameObject target = other.gameObject;
            Vector3 contactPoint = other.ClosestPoint(transform.position);

            EnemyStats stats = target.GetComponent<EnemyStats>();
            if (stats == null) stats = target.GetComponentInParent<EnemyStats>();

            if (stats != null)
            {
                // Hasar ver (EnemyStats artık ölse bile flash yapıyor)
                bool isDead = stats.TakeDamage(swordDamage);

                // --- EFEKTLER (SES & VFX) ---
                if (hitSound != null)
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(hitSound);
                }
                if (hitVFX != null) Instantiate(hitVFX, contactPoint, Quaternion.identity);

                if (!isDead)
                {
                    // ÖLMEDİYSE: Sekme (Recoil)
                    SwordMasterController swordCtrl = GetComponentInParent<SwordMasterController>();
                    if (swordCtrl != null) swordCtrl.ApplyRecoil(25f);

                    if (useCameraShake && CameraShake.Instance != null)
                        CameraShake.Instance.Shake(0.1f, 0.2f);
                }
                else
                {
                    // ÖLDÜYSE: Hemen kesme! Sinematik ölüm başlat.
                    StartCoroutine(DeathSequence(stats, target, contactPoint));
                }
            }
            else
            {
                // Düşman değilse (Kutu vs.) direkt kes
                SliceTarget(target, contactPoint);
            }
        }
    }

    // --- SİNEMATİK ÖLÜM RUTİNİ ---
    IEnumerator DeathSequence(EnemyStats stats, GameObject target, Vector3 contactPoint)
    {
        isProcessingKill = true;

        // 1. ZAMANI DURDUR (HIT STOP)
        // Bu sırada düşman BEYAZ görünüyor (EnemyStats öyle ayarladı)
        if (useCameraShake && CameraShake.Instance != null)
            CameraShake.Instance.Shake(hitStopDuration, 0.4f); // Sert salla

        Time.timeScale = 0f;

        // Gerçek dünyada bekle (Oyun donukken biz bekliyoruz)
        yield return new WaitForSecondsRealtime(hitStopDuration);

        Time.timeScale = 1f; // Zamanı geri aç

        // 2. RENGİ DÜZELT
        // Kesmeden hemen önce rengi normale çevir ki kafa beyaz kalmasın
        if (stats != null) stats.ResetMaterialsImmediately();

        // 3. ŞİMDİ KES
        SliceTarget(target, contactPoint);

        // Stats içindeki yok etme fonksiyonunu çağır (Loot düşsün diye)
        if (stats != null) stats.OnEnemySliced();
        else Destroy(target); // Stats yoksa direkt yok et

        isProcessingKill = false;
    }

    // --- KESME FONKSİYONLARI ---
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
    }

    // (SliceObject, SliceCharacter, SetupSlicedComponent fonksiyonları aynı kalacak, elleme)
    // Buraya önceki kodlardan o fonksiyonları yapıştırabilirsin.

    // ... SliceCharacter ...
    // ... SliceObject ...
    // ... GetCutNormal ...
    // ... SetupSlicedComponent ...

    // Kısalık olsun diye tekrar yazmadım, önceki SlicerTrigger'ın alt kısmını buraya yapıştır.

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
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 2f);
        Destroy(slicedObject, 4f);
    }
}