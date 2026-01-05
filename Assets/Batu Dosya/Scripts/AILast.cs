using UnityEngine;

public class AILast : MonoBehaviour
{
    [Header("Hýz Ayarý")]
    public float moveSpeed = 8f;
    public float donmeHizi = 10f;

    [HideInInspector] public Transform target;

    // POOLING ÝÇÝN KRÝTÝK DEÐÝÞÝKLÝK: Start -> OnEnable
    void OnEnable()
    {
        // Hedef yoksa bulmaya çalýþ
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
        targetPos.y = transform.position.y; // Yere paralel bak

        Vector3 direction = (targetPos - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * donmeHizi);
        }

        // --- 2. ÜSTÜNE KOÞ ---
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}