using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Tooltip("Dönme hýzý.")]
    public float rotationSpeed = 50f;

    void Update()
    {
        
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
}