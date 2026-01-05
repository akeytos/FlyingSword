using UnityEngine;

public class AILast : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 8f;
    public float donmeHizi = 10f;

    [Header("Tür Ayarý")]
    public bool isFlying = false; // [YENÝ] Bunu seçersen havadan gelir

    [HideInInspector] public Transform target;

    void OnEnable()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        Vector3 targetPos = target.position;

        // [YENÝ] Eðer uçmuyorsa (yerdeyse), hedefi kendi yüksekliðinde görsün
        // Böylece yukarý/aþaðý bakmaya çalýþýp topraða girmez.
        if (!isFlying)
        {
            targetPos.y = transform.position.y;
        }

        Vector3 direction = (targetPos - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            // Havadaysa her yöne, yerdeyse sadece saða sola döner
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * donmeHizi);
        }

        // Ýleri git
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}