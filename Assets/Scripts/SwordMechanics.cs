using UnityEngine;

public class SwordMechanic : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform swordPivot;
    // YENÝ: Trail Renderer referansý
    public TrailRenderer swordTrail;

    [Header("Bilek (Swing) Ayarlarý")]
    public float reachDistance = 10f;
    public float swingSpeed = 25f;
    // YENÝ: Ýz çýkmasý için gereken minimum hýz
    public float trailSpeedThreshold = 0.1f;

    [Header("El Gezinmesi (Kabza Hareketi)")]
    public float handMoveRangeX = 2.0f;
    public float handMoveRangeY = 1.5f;
    public float handMoveSpeed = 8f;
    public float baseDistance = 1.5f;

    // YENÝ: Hýz hesabý için önceki mouse pozisyonu
    private Vector2 lastMousePos;

    void Update()
    {
        if (InputManager.instance == null || swordPivot == null) return;

        HandleSwordSwing();
        HandleTrail(); // YENÝ fonksiyonu çaðýr
    }

    // --- YENÝ FONKSÝYON: ÝZ KONTROLÜ ---
    void HandleTrail()
    {
        if (swordTrail == null) return;

        Vector2 currentMousePos = InputManager.instance.virtualMouse;

        // Mouse'un bu karedeki hýzý = (Þu anki pos - Önceki pos) / Zaman
        float mouseSpeed = (currentMousePos - lastMousePos).magnitude / Time.deltaTime;

        // Eðer hýz eþik deðerden yüksekse izi aç (emitting = true)
        if (mouseSpeed > trailSpeedThreshold)
        {
            swordTrail.emitting = true;
        }
        else
        {
            // Yavaþsa veya duruyorsa izi kapat
            swordTrail.emitting = false;
        }

        // Bir sonraki kare için pozisyonu kaydet
        lastMousePos = currentMousePos;
    }
    // ------------------------------------

    void HandleSwordSwing()
    {
        Vector2 mousePos = InputManager.instance.virtualMouse;

        float targetX = (mousePos.x - 0.5f) * handMoveRangeX;
        float targetY = (mousePos.y - 0.5f) * handMoveRangeY;
        Vector3 targetPos = new Vector3(targetX, targetY, baseDistance);

        swordPivot.localPosition = Vector3.Lerp(swordPivot.localPosition, targetPos, Time.deltaTime * handMoveSpeed);

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(mousePos.x, mousePos.y, 0));
        Vector3 lookPoint = ray.GetPoint(reachDistance);

        Vector3 direction = lookPoint - swordPivot.position;
        Quaternion targetRot = Quaternion.LookRotation(direction, Camera.main.transform.up);

        swordPivot.rotation = Quaternion.Lerp(swordPivot.rotation, targetRot, Time.deltaTime * swingSpeed);
    }
}