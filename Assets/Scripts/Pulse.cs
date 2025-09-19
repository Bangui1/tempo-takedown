using UnityEngine;

public class Pulse : MonoBehaviour
{
    public float speed = 2f;        // How fast the sprite pulses
    public float scaleAmount = 0.2f; // How much it shrinks/grows

    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = baseScale * scale;
    }
}
