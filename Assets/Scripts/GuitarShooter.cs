using UnityEngine;

public class GuitarShooter : MonoBehaviour
{
    public GameObject notePrefab; // assign your NoteBullet prefab
    public float bulletSpeed = 10f;

    void Update()
    {
        // Shoot when pressing space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Spawn at guitar position
        GameObject note = Instantiate(notePrefab, transform.position, Quaternion.identity);

        // Add velocity
        Rigidbody2D rb = note.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.up * bulletSpeed; // upward, change to right/left if needed
        }
    }
}