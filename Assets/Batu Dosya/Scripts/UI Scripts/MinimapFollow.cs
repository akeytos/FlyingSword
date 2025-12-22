using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // Oyuncunun Transform'unu buraya sürükle

    // Kameranýn oyuncudan ne kadar yüksekte duracaðý (Unity editöründen ayarladýðýn fark)
    private float yOffset;

    void Start()
    {
        // Baþlangýçtaki yükseklik farkýný kaydet
        if (player != null)
        {
            yOffset = transform.position.y - player.position.y;
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // Sadece X ve Z pozisyonunu takip et (Yatay düzlem)
            // Y pozisyonunu (yüksekliði) sabit tut.
            // Rotasyonu ELLEMEK YOK, hep aþaðý baksýn.
            Vector3 newPosition = player.position;
            newPosition.y += yOffset;

            transform.position = newPosition;
        }
    }
}