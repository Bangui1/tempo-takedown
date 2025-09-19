using UnityEngine;

public class Swing : MonoBehaviour
{
    public float speed = 2f;         // How fast it swings
    public float angle = 15f;        // Max degrees from center

    private float startZ;

    void Start()
    {
        // Record the starting rotation (Z axis only for 2D)
        startZ = transform.localEulerAngles.z;
    }

    void Update()
    {
        float swing = Mathf.Sin(Time.time * speed) * angle;
        transform.localRotation = Quaternion.Euler(0, 0, startZ + swing);
    }
}
