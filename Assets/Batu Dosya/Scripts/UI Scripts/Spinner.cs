using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float speed = 200f;

    void Update()
    {
        // Z ekseninde sürekli döndür
        transform.Rotate(0, 0, -speed * Time.deltaTime);
    }
}