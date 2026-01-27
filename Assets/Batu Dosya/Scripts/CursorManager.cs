using UnityEngine;

public class CursorManager : MonoBehaviour
{
    // Oyunun baþýnda imleç kilitli mi baþlasýn? (Evet)
    public bool isCursorLocked = true;

    void Start()
    {
        // Baþlangýçta ayarý uygula
        UpdateCursorState();
    }

    void Update()
    {
        // ESC tuþuna basýlýnca durumu tersine çevir
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCursorLocked = !isCursorLocked; // True ise False, False ise True yapar
            UpdateCursorState();
        }

        // EKSTRA: Eðer oyunda sol týk yapýnca tekrar kilitlenmesini istersen (FPS oyunlarý gibi)
        // bu satýrý açabilirsin:
        /*
        if (Input.GetMouseButtonDown(0) && !isCursorLocked)
        {
             isCursorLocked = true;
             UpdateCursorState();
        }
        */
    }

    void UpdateCursorState()
    {
        if (isCursorLocked)
        {
            // ÝMLECÝ GÝZLE VE KÝLÝTLE
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // ÝMLECÝ GÖSTER VE SERBEST BIRAK
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}