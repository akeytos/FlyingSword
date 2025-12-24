using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1;

    void OnCollisionStay(Collision collision)
    {
        // 1. Çarpýþma algýlanýyor mu?
        Debug.Log("Düþman bir þeye çarptý: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. Oyuncu etiketi doðru mu?
            Debug.Log("Düþman OYUNCUYU buldu!");

            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            else
            {
                Debug.Log("HATA: PlayerHealth scripti bulunamadý!");
            }
        }
    }
}