using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance; // Her yerden ulaþalým diye Singleton yapýyoruz

    void Awake()
    {
        Instance = this;
    }

    public void Shake(float duration, float magnitude)
    {
        // Eðer zaten sallanýyorsa üstüne binmesin, durdurup yeniden baþlatsýn (Opsiyonel)
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Rastgele saða sola kaydýr
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Z eksenini (derinliði) bozma, sadece X ve Y titresin
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Sallantý bitince kamerayý eski yerine koy
        transform.localPosition = originalPos;
    }
}