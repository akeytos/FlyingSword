using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1;
    public float hitCooldown = 1.5f;

    private float nextHitTime = 0f;

    void OnCollisionStay(Collision collision)
    {
        if (Time.time < nextHitTime) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                nextHitTime = Time.time + hitCooldown;
            }
        }
    }
}