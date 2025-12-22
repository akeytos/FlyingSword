using UnityEngine;

public class CloneController : MonoBehaviour
{
    private Transform targetPlayer;
    private Vector3 offset;
    private bool isSetup = false;

    public void Setup(Transform player, Vector3 posOffset)
    {
        targetPlayer = player;
        offset = posOffset;
        isSetup = true;
    }

    void Update()
    {
        // Hedef yoksa iþlem yapma
        if (!isSetup || targetPlayer == null) return;

        // --- 1. POZÝSYON TAKÝBÝ ---
        // Senin yanýnda durmasý için (offset kadar uzaða git)
        Vector3 targetPos = targetPlayer.TransformPoint(offset);
        // Hýzlý takip için 40f hýz
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 40f);

        // --- 2. ROTASYON TAKÝBÝ (KARÞIYA BAK - YAN YATMA) ---
        // targetPlayer.forward -> Oyuncunun baktýðý yön (karþý)
        // Vector3.up -> Dünyanýn yukarýsý (Bu sayede kýlýç dik durur)
        transform.rotation = Quaternion.LookRotation(targetPlayer.forward, Vector3.up);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Klon düþmana vurdu!");
        }
    }
}