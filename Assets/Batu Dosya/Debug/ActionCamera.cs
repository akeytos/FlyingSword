using UnityEngine;

public class ActionCamera : MonoBehaviour
{
    [Header("🎥 Takip Ayarları")]
    public Transform target;           // Player (Kılıç) objesini buraya sürükle
    public Vector3 offset = new Vector3(0, 2.5f, -6f); // Kamera kılıcın neresinde dursun?
    public float followSpeed = 10f;    // Pozisyon takibi ne kadar hızlı? (Düşük = Daha yumuşak)
    public float rotationSpeed = 5f;   // Dönüş takibi ne kadar hızlı?

    [Header("💨 Hız Efektleri (Dynamic FOV)")]
    public float minFOV = 60f;         // Dururkenki açı
    public float maxFOV = 85f;         // Hızlanınca çıkacağı açı
    public float zoomSpeed = 2f;       // FOV değişim hızı

    // Kılıcın hızını ölçmek için referans
    private Rigidbody targetRb;

    void Start()
    {
        if (target) targetRb = target.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!target) return;

        HandleMovement();
        HandleRotation();
        HandleDynamicFOV();
    }

    void HandleMovement()
    {
        // Hedeflenen pozisyon: Kılıcın arkası + Offset
        // TransformPoint: Local koordinatı World koordinatına çevirir (Kılıç dönünce arkası da döner)
        Vector3 targetPosition = target.TransformPoint(offset);

        // Vector3.Lerp ile yumuşak geçiş yapıyoruz.
        // Bu sayede kılıç ani hareket etse bile kamera "yaylı" gibi arkadan gelir.
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.fixedDeltaTime);
    }

    void HandleRotation()
    {
        // Kameranın bakacağı yön: Kılıcın ileri yönü ile kılıcın olduğu yer arasındaki fark
        // Ama biz direkt kılıcın rotasyonunu da yumuşatarak alabiliriz.

        // Kılıcın baktığı yöne bak ama yumuşakça dön
        Quaternion targetRotation = Quaternion.LookRotation(target.forward, Vector3.up);

        // Hafif yukarı bakması için (offset'e göre) düzeltme gerekebilir ama LookRotation genelde yeter.
        // Kılıç çok takla atıyorsa, kameranın Z eksenini (Roll) limitlemek gerekebilir.

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    void HandleDynamicFOV()
    {
        if (!targetRb) return;

        // Kılıcın hızı arttıkça FOV artar (Hız hissi için en önemli ayar!)
        float speedPercent = Mathf.Clamp01(targetRb.linearVelocity.magnitude / 30f); // 30f senin max hızın olsun (Unity 6 öncesi için .velocity)
        float targetFOV = Mathf.Lerp(minFOV, maxFOV, speedPercent);

        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.fixedDeltaTime * zoomSpeed);
    }
}