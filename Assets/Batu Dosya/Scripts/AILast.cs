using UnityEngine;

public class AILast : MonoBehaviour
{
    [Header("Hýz Ayarý")]
    public float moveSpeed = 8f;   // Düþmanýn hýzý
    public float donmeHizi = 10f;  // Dönüþ hýzý

    // Spawner tarafýndan otomatik doldurulacak
    [HideInInspector] public Transform target;

    void Start()
    {
        // Eðer hedef yoksa (Spawner vermediyse) haritada Player'ý bulmaya çalýþ (Yedek plan)
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        // --- 1. YÜZÜNÜ DÖN ---
        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y; // Yere paralel bak, havaya bakma

        Vector3 direction = (targetPos - transform.position).normalized;

        // Eðer hareket ediyorsak dönelim
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * donmeHizi);
        }

        // --- 2. ÜSTÜNE KOÞ ---
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}