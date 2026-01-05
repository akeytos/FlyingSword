using UnityEngine;

public class CloneEdges : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject clonePrefab; // Yarý saydam kýlýç prefabý
    public float duration = 10f;
    public float cooldown = 15f;
    public float offsetDistance = 2f; // Ne kadar saðda/solda duracak?

    private float cooldownTimer = 0f;
    private bool isActive = false;

    private GameObject leftClone;
    private GameObject rightClone;

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(1) && cooldownTimer <= 0 && !isActive)
        {
            ActivateClones();
        }

        // Klonlarý Pozisyonla (Update içinde sürekli takip etsinler)
        if (isActive)
        {
            if (leftClone != null)
            {
                leftClone.transform.position = transform.position - transform.right * offsetDistance;
                leftClone.transform.rotation = transform.rotation;
            }
            if (rightClone != null)
            {
                rightClone.transform.position = transform.position + transform.right * offsetDistance;
                rightClone.transform.rotation = transform.rotation;
            }
        }
    }

    void ActivateClones()
    {
        isActive = true;

        // Klonlarý Yarat
        leftClone = Instantiate(clonePrefab, transform.position, transform.rotation);
        rightClone = Instantiate(clonePrefab, transform.position, transform.rotation);

        // Klonlarýn Collider'ý hasar vermeli (WeaponDamage scripti olmalý prefabda)

        Invoke("DestroyClones", duration);

        float reduction = SwordStats.Instance != null ? SwordStats.Instance.cooldownReduction : 0f;
        cooldownTimer = cooldown * (1f - reduction);
    }

    void DestroyClones()
    {
        isActive = false;
        if (leftClone != null) Destroy(leftClone);
        if (rightClone != null) Destroy(rightClone);
    }

    public void OnLevelUp()
    {
        duration += 2f;
        Debug.Log("Clone Edges Süresi Arttý!");
    }
}