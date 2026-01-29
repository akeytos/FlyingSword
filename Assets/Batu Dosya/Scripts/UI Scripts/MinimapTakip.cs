using UnityEngine;

public class MinimapTakip : MonoBehaviour
{
    [Header(" Player ")]
    public Transform hedef; 


    void LateUpdate()
    {
     if (hedef != null)
        {

            Vector3 yeniPozisyon = hedef.position;
            yeniPozisyon.y = transform.position.y;

            transform.position = yeniPozisyon;

            transform.rotation = Quaternion.Euler(90f, 0f, 0f);



        }
    }
}
