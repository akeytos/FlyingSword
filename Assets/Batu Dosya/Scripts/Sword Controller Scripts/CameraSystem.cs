using UnityEngine;

public class CameraSystem : MonoBehaviour
{
    public Transform target; // Buraya Player_PILOT'u sürükleyeceksin

    [Header("🎥 Takip Ayarları")]
    public float followSpeed = 5f; // Düşük olursa geriden gelir (Hız hissi)
    public Vector3 offset = new Vector3(0, 18, -12); // İzometrik bakış açısı

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Hedef Pozisyon
        Vector3 desiredPos = target.position + offset;

        // 2. Yumuşak Geçiş (Lerp)
        // DeltaTime ile çarparak kare hızından bağımsız yapıyoruz
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        // 3. Uygula
        transform.position = smoothedPos;

        // 4. Hedefe Bak (Opsiyonel, top-down için gerekmez ama garantidir)
        transform.LookAt(target.position);
    }
}