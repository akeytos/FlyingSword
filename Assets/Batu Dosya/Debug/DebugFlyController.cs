using UnityEngine;

public class DebugFlyController : MonoBehaviour
{
    [Header("Uçuþ Ayarlarý")]
    public float moveSpeed = 20f;      // Normal hýz
    public float boostMultiplier = 2f; // Shift'e basýnca hýzlanma
    public float mouseSensitivity = 100f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        // Mouse'u ekrana kilitle ki rahat dönelim
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Takla atmayý kýsýtla

        // Kamerayý ve objeyi döndür
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    void HandleMovement()
    {
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed *= boostMultiplier; // Shift ile hýzlan

        float x = Input.GetAxis("Horizontal"); // A-D
        float z = Input.GetAxis("Vertical");   // W-S
        float y = 0;

        // Space ile yukarý, Ctrl ile aþaðý (Dikey manevra testi için)
        if (Input.GetKey(KeyCode.Space)) y = 1;
        if (Input.GetKey(KeyCode.LeftControl)) y = -1;

        Vector3 move = transform.right * x + transform.forward * z + transform.up * y;

        transform.position += move * speed * Time.deltaTime;
    }
}