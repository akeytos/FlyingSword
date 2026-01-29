using UnityEngine;

public class PervaneDondur : MonoBehaviour
{
    [Header("Ayarlar")]
    public float donmeHizi = 500f;

    public Vector3 donmeEkseni = new Vector3(0, 1, 0);


    void Update()
    {

        transform.Rotate(donmeEkseni * donmeHizi * Time.deltaTime, Space.Self);
    }
}
