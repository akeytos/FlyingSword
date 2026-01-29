using UnityEngine;

public class ForceSize : MonoBehaviour
{
    // Buraya istediðin boyutu yaz (X, Y, Z)
    public Vector3 hedefBoyut = new Vector3(2f, 2f, 2f);

    // LateUpdate, Animator çalýþýp bittikten SONRA çalýþýr.
    // Yani Animator boyutu 1 yapsa bile, bu kod hemen arkasýndan 2 yapar.
    void LateUpdate()
    {
        transform.localScale = hedefBoyut;
    }
}