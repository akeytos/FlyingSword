using UnityEngine;

public class SwordStabilizer : MonoBehaviour
{
    [Header("Denge Ayarlarý")]
    public float duzelmeHizi = 5.0f; // Ne kadar hýzlý düzelsin? (Yüksek = Hemen düzelir)
    public float maxYatis = 30f;     // Dönüþlerde en fazla kaç derece yatsýn?

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Mevcut açýlarý al
        Vector3 currentRot = rb.rotation.eulerAngles;

        // Z açýsýný kontrol et (360 derece sorunu için matematiksel düzeltme)
        float zAngle = currentRot.z;
        if (zAngle > 180) zAngle -= 360; // Açýyý -180 ile 180 arasýna çek

        // Eðer açý çok bozulmuþsa (örneðin -115 olmuþsa), onu zorla limitlere çek
        // Yani kýlýç asla 30 dereceden fazla yatamaz.
        float clampedZ = Mathf.Clamp(zAngle, -maxYatis, maxYatis);

        // Þimdi yavaþça 0'a (düze) doðru getir
        float newZ = Mathf.Lerp(clampedZ, 0, duzelmeHizi * Time.fixedDeltaTime);

        // Yeni rotasyonu oluþtur (X ve Y'ye dokunma, sadece Z'yi düzelt)
        Quaternion targetRotation = Quaternion.Euler(currentRot.x, currentRot.y, newZ);

        // Fiziði bozmadan uygula
        rb.MoveRotation(targetRotation);
    }
}