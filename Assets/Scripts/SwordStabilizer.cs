using UnityEngine;

public class SwordStabilizer : MonoBehaviour
{
    [Header("--- HEDEFLER ---")]
    public Transform cameraTransform; // Main Camera'yý buraya sürükle
    public Transform playerBody;      // Player objesini buraya sürükle

    [Header("--- AYARLAR ---")]
    public float fixedHeight = 1.4f;  // Yerden yükseklik (Bel hizasý için 1.2 - 1.5 dene)
    public float forwardOffset = 0.8f; // Vücuttan ne kadar önde dursun?
    public float sideOffset = 0.5f;    // Vücudun ne kadar saðýnda dursun?

    [Header("--- DÖNÜÞ AYARLARI ---")]
    public float rotationSpeed = 15f; // Kýlýcýn dönme gecikmesi (Smooth hissi)

    void LateUpdate()
    {
        if (cameraTransform == null || playerBody == null) return;

        // 1. POZÝSYON HESABI (Yüksekliði biz belirliyoruz)
        // Kameranýn sadece YATAY (Y) açýsýný alýyoruz. Yukarý/Aþaðý bakmayý yoksayýyoruz.
        Vector3 flatForward = cameraTransform.forward;
        flatForward.y = 0; // Yere bakmayý iptal et
        flatForward.Normalize();

        Vector3 flatRight = cameraTransform.right;
        flatRight.y = 0;
        flatRight.Normalize();

        // Hedef Pozisyon: Oyuncunun pozisyonu + Yükseklik + Ýleri/Saða payý
        Vector3 targetPosition = playerBody.position
                               + Vector3.up * fixedHeight
                               + flatForward * forwardOffset
                               + flatRight * sideOffset;

        // Kýlýcý o pozisyona ýþýnla (veya Lerp ile yumuþatabilirsin)
        transform.position = targetPosition;

        // 2. ROTASYON HESABI (Kameranýn baktýðý yere dönsün ama yere bakmasýn)
        // Kameranýn sadece Y eksenindeki dönüþünü al
        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);

        // Yumuþakça o yöne dön
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}