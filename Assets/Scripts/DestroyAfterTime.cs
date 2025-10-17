using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float lifeTime = 3f;
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}