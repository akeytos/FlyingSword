using UnityEngine;

public class BodyRotation : MonoBehaviour
{
    [Header("Dönüþ Ayarlarý")]
    public float bodyTurnSpeed = 60f;
    public float edgeThreshold = 0.85f; // Ekranýn %85'ine gelince dön

    void Update()
    {
        if (InputManager.instance == null) return;

        HandleBodyTurn();
    }

    void HandleBodyTurn()
    {
        Vector2 mousePos = InputManager.instance.virtualMouse;

        // SAÐ / SOL DÖNÜÞ (Gövde)
        if (mousePos.x > edgeThreshold)
            transform.Rotate(Vector3.up * bodyTurnSpeed * Time.deltaTime);
        else if (mousePos.x < 1 - edgeThreshold)
            transform.Rotate(Vector3.up * -bodyTurnSpeed * Time.deltaTime);

        // YUKARI / AÞAÐI BAKMA (Kamera)
        float pitch = 0f;
        if (mousePos.y > edgeThreshold) pitch = -bodyTurnSpeed * Time.deltaTime;
        if (mousePos.y < 1 - edgeThreshold) pitch = bodyTurnSpeed * Time.deltaTime;

        // Kameranýn mevcut açýsýný al ve sýnýrla
        float currentX = Camera.main.transform.localEulerAngles.x;
        if (currentX > 180) currentX -= 360;

        float nextX = Mathf.Clamp(currentX + pitch, -80f, 80f);
        Camera.main.transform.localEulerAngles = new Vector3(nextX, 0, 0);
    }
}