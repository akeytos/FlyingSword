using UnityEngine;

public class SwordJuice : MonoBehaviour
{
    [Header("--- BA�LANTILAR ---")]
    public Transform visualModel; // K�l�c�n G�RSEL� (Child Obje) - Bunu atamazsan �al��maz!
    private Rigidbody rb;

    [Header("--- H�SS�YAT AYARLARI ---")]
    [Tooltip("K�l�� hareket edince ne kadar yats�n? (Aerodinamik)")]
    public float tiltAmount = 7f;
    [Tooltip("Yatma hareketi ne kadar yumu�ak olsun?")]
    public float tiltSpeed = 4f;

    [Tooltip("Mouse �evirince k�l�� ne kadar geriden gelsin? (A��rl�k Hissi)")]
    public float swayAmount = 30f;
    [Tooltip("Eski haline ne kadar h�zl� d�ns�n?")]
    public float swaySpeed = 3f;

    [Tooltip("Dururken havada inip kalkma h�z�")]
    public float bobSpeed = 2f;
    [Tooltip("Dururken inip kalkma mesafesi")]
    public float bobAmount = 0.1f;

    private Vector3 initialPos;
    private Quaternion initialRot;
    private float targetXRot;
    private float targetZRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (visualModel == null)
        {
            // E�er elle atamay� unuttuysan ilk �ocu�u bulmaya �al��al�m
            visualModel = transform.GetChild(0);
        }

        // Ba�lang�� duru�unu kaydet
        initialPos = visualModel.localPosition;
        initialRot = visualModel.localRotation;
    }

    void Update()
    {
        if (visualModel == null) return;

        HandleTilt(); // Hareket edince yatma
        HandleSway(); // D�n�nce savrulma
        HandleBobbing(); // Nefes alma
    }

    void HandleTilt()
    {
        // 1. HIZI �L�: K�l�� yerel olarak (Local) ne tarafa gidiyor?
        // Sa�a gidiyorsa Velocity.x artar, �leri gidiyorsa Velocity.z artar.
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

        // 2. HEDEF A�IYI BUL:
        // Sa�a gidiyorsak (Vel.x > 0), Sola yatmal�y�z (-Rot.z)
        // �leri gidiyorsak (Vel.z > 0), Arkaya yatmal�y�z (-Rot.x) veya �ne batmal�y�z (+Rot.x)

        float targetZ = -localVel.x * (tiltAmount * 0.15f); // Sa�a/Sola yat��
        float targetX = localVel.z * (tiltAmount * 0.15f);  // �leri/Geri yat��

        // A��lar� s�n�rla (K�l�� takla atmas�n)
        targetZ = Mathf.Clamp(targetZ, -tiltAmount, tiltAmount);
        targetX = Mathf.Clamp(targetX, -tiltAmount, tiltAmount);

        // 3. UYGULA: Sadece X ve Z eksenini b�k�yoruz. Y (Kendi etraf�nda d�n��) InputManager'dan geliyor.
        Quaternion targetRotation = initialRot * Quaternion.Euler(targetX, 0, targetZ);

        // Slerp ile yumu�ak ge�i� (K�t�k hissini yok eden �ey bu!)
        visualModel.localRotation = Quaternion.Slerp(visualModel.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
    }

    void HandleSway()
    {
        // Mouse hareketi (InputManager'dan okuyabiliriz veya direkt Input'tan)
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Mouse sa�a gidince, k�l�� g�rseli hafif SOLA kays�n (Geride kalma efekti)
        targetXRot = -mouseY * swayAmount;
        targetZRot = -mouseX * swayAmount; // Z ekseninde hafif b�k�lme

        // S�n�rla
        targetXRot = Mathf.Clamp(targetXRot, -swayAmount, swayAmount);
        targetZRot = Mathf.Clamp(targetZRot, -swayAmount, swayAmount);

        // Bu sway efektini mevcut rotasyonun �zerine ek bir katman olarak ekleyebiliriz
        // Ama yukar�daki Tilt ile �ak��mamas� i�in bunu Rotation yerine POSITION ile yapmak daha "tok" hissettirir.
        // Gel biz bunu Lag (Gecikme) olarak yapal�m:

        float moveX = -mouseX * 0.1f; // Tersi y�nde hareket
        float moveY = -mouseY * 0.1f;

        Vector3 finalPos = new Vector3(moveX, moveY, 0);

        // Bobbing (Nefes alma) ile birle�tirece�iz, o y�zden �imdilik burada kals�n.
    }

    void HandleBobbing()
    {
        // Havada s�z�lme (Sin�s dalgas�)
        float bobY = Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        // Sway'den gelen mouse gecikmesini de ekleyelim
        float mouseX = Input.GetAxis("Mouse X") * 0.2f;
        float mouseY = Input.GetAxis("Mouse Y") * 0.2f;

        // Hedef Pozisyon: Ba�lang�� + Bobbing + Mouse Sway
        Vector3 targetPos = initialPos + new Vector3(-mouseX, bobY - mouseY, 0);

        // Yumu�ak�a git
        visualModel.localPosition = Vector3.Lerp(visualModel.localPosition, targetPos, Time.deltaTime * swaySpeed);
    }
}