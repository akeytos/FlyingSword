using System.Collections;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [Header("Ba�lant�lar")]
    public SwordMasterController masterController; // Ana beyin scripti

    [Header("Sald�r� Ayarlar�")]
    public float saldiriSuresi = 0.3f; // Ne kadar s�rede d�necek?
    public Vector3 donusEkseni = new Vector3(1, 0, 0); // (1,0,0) = Takla atma

    void Start()
    {
        // E�er elle atamazsan otomatik bulsun
        if (masterController == null)
            masterController = GetComponent<SwordMasterController>();
    }

    void Update()
    {
        // T�klay�nca sald�r� ba�lat (E�er zaten sald�rm�yorsa)
        if (Input.GetMouseButtonDown(0) && !masterController.isAttacking)
        {
            StartCoroutine(SaldiriYap());
        }
    }

    IEnumerator SaldiriYap()
    {
        // 1. Ana kontrolc�ye "Ben devrald�m, sen kar��ma" de
        masterController.isAttacking = true;

        // Trail efektini a� (Varsa)
        if (masterController.swordTrail != null)
            masterController.swordTrail.emitting = true;

        float gecenSure = 0f;

        // 2. K�l�c� D�nd�r (Animation Logic)
        while (gecenSure < saldiriSuresi)
        {
            // Delta time ile d�n�� a��s�n� hesapla (360 derece / s�re)
            float donusMiktari = (360f / saldiriSuresi) * Time.deltaTime;

            // K�l�� Pivotunu d�nd�r
            if (masterController.swordPivot != null)
                masterController.swordPivot.Rotate(donusEkseni * donusMiktari, Space.Self);

            gecenSure += Time.deltaTime;
            yield return null; // Bir sonraki kareyi bekle
        }

        // 3. Sald�r� Bitti, kontrol� geri ver
        masterController.isAttacking = false;

        // Trail efektini kapat (Normal harekete d�ns�n)
        if (masterController.swordTrail != null)
            masterController.swordTrail.emitting = false;
    }

}