using System.Collections;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [Header("Baðlantýlar")]
    public SwordMasterController masterController; // Ana beyin scripti

    [Header("Saldýrý Ayarlarý")]
    public float saldiriSuresi = 0.3f; // Ne kadar sürede dönecek?
    public Vector3 donusEkseni = new Vector3(1, 0, 0); // (1,0,0) = Takla atma

    void Start()
    {
        // Eðer elle atamazsan otomatik bulsun
        if (masterController == null)
            masterController = GetComponent<SwordMasterController>();
    }

    void Update()
    {
        // Týklayýnca saldýrý baþlat (Eðer zaten saldýrmýyorsa)
        if (Input.GetMouseButtonDown(0) && !masterController.isAttacking)
        {
            StartCoroutine(SaldiriYap());
        }
    }

    IEnumerator SaldiriYap()
    {
        // 1. Ana kontrolcüye "Ben devraldým, sen karýþma" de
        masterController.isAttacking = true;

        // Trail efektini aç (Varsa)
        if (masterController.swordTrail != null)
            masterController.swordTrail.emitting = true;

        float gecenSure = 0f;

        // 2. Kýlýcý Döndür (Animation Logic)
        while (gecenSure < saldiriSuresi)
        {
            // Delta time ile dönüþ açýsýný hesapla (360 derece / süre)
            float donusMiktari = (360f / saldiriSuresi) * Time.deltaTime;

            // Kýlýç Pivotunu döndür
            if (masterController.swordPivot != null)
                masterController.swordPivot.Rotate(donusEkseni * donusMiktari, Space.Self);

            gecenSure += Time.deltaTime;
            yield return null; // Bir sonraki kareyi bekle
        }

        // 3. Saldýrý Bitti, kontrolü geri ver
        masterController.isAttacking = false;

        // Trail efektini kapat (Normal harekete dönsün)
        if (masterController.swordTrail != null)
            masterController.swordTrail.emitting = false;
    }
}