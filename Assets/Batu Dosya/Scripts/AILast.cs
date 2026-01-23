using UnityEngine;

public class AILast : MonoBehaviour
{
    [Header("Hareket Ayarlari")]
    public float moveSpeed = 8f;
    public float donmeHizi = 10f;

    [Header("Tur Ayari")]
    public bool isFlying = false; // Ucan dusmanlar

    [Header("Ayrim/Itisme")]
    [Min(0f)] public float separationRadius = 0.6f;
    [Min(0f)] public float separationStrength = 3f;
    [Range(1, 32)] public int separationMaxNeighbors = 8;
    public LayerMask enemyLayer;

    [HideInInspector] public Transform target;

    private Rigidbody rb;
    private Collider[] separationHits;

    void OnEnable()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (separationHits == null || separationHits.Length == 0) separationHits = new Collider[16];
        if (enemyLayer.value == 0) enemyLayer = 1 << gameObject.layer;

        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position;

        // Yerdeyse hedefi kendi yuksekliginde gorsun
        if (!isFlying)
        {
            targetPos.y = transform.position.y;
        }

        Vector3 direction = (targetPos - transform.position).normalized;
        Vector3 separation = ComputeSeparation();
        if (separation != Vector3.zero)
        {
            direction = (direction + separation * separationStrength).normalized;
        }

        if (direction != Vector3.zero)
        {
            // Havadaysa her yone, yerdeyse sadece saga/sola doner
            Quaternion lookRot = Quaternion.LookRotation(direction);
            float dt = Time.fixedDeltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, dt * donmeHizi);
        }

        // Ileri git
        Vector3 moveDelta = transform.forward * moveSpeed * Time.fixedDeltaTime;
        if (rb != null)
            rb.MovePosition(rb.position + moveDelta);
        else
            transform.position += moveDelta;
    }

    Vector3 ComputeSeparation()
    {
        if (separationRadius <= 0f) return Vector3.zero;

        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            separationRadius,
            separationHits,
            enemyLayer
        );

        if (hitCount <= 1) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        int used = 0;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = separationHits[i];
            if (col == null) continue;
            if (col.attachedRigidbody == rb) continue;
            if (col.transform == transform) continue;

            Vector3 away = transform.position - col.transform.position;
            float sqr = away.sqrMagnitude;
            if (sqr < 0.0001f) continue;

            sum += away.normalized / Mathf.Max(0.1f, Mathf.Sqrt(sqr));
            used++;
            if (used >= separationMaxNeighbors) break;
        }

        if (used == 0) return Vector3.zero;
        return (sum / used).normalized;
    }
}