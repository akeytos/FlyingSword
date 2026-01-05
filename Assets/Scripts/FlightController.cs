using UnityEngine;

public class FlightController : MonoBehaviour
{
    [Header("Uçuþ Ayarlarý")]
    public float flySpeed = 15f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        // Unity sürümüne göre 'drag' veya 'linearDamping'
        // Eðer Unity 6 kullanýyorsan linearDamping kalsýn, hata verirse drag yap.
        rb.linearDamping = 5f;
    }

    void FixedUpdate()
    {
        if (InputManager.instance == null) return;

        HandleMovement();
    }

    void HandleMovement()
    {
        float h = InputManager.instance.horizontal;
        float v = InputManager.instance.vertical;

        // Kameranýn baktýðý yöne doðru kuvvet uygula
        Vector3 moveDir = Camera.main.transform.right * h + Camera.main.transform.forward * v;

        // ForceMode.Force daha yumuþak, Impulse daha ani tepki verir. Uçuþ için Force iyidir.
        rb.AddForce(moveDir * flySpeed * 50f, ForceMode.Force);
    }
}