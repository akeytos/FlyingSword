using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Singleton: Diðer scriptlerden kolayca ulaþmak için
    public static InputManager instance;

    [Header("Mouse Hassasiyeti")]
    public float mouseSensitivity = 0.05f;

    // Diðer scriptlerin okuyacaðý veriler
    [HideInInspector] public Vector2 virtualMouse = new Vector2(0.5f, 0.5f); // Sanal Ýmleç
    [HideInInspector] public float horizontal; // A-D
    [HideInInspector] public float vertical;   // W-S

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. Mouse Verisini Ýþle
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        virtualMouse.x = Mathf.Clamp01(virtualMouse.x + mouseX);
        virtualMouse.y = Mathf.Clamp01(virtualMouse.y + mouseY);

        // 2. Klavye Verisini Ýþle
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }
}