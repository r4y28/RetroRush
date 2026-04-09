using UnityEngine;

public class SpikeRotate : MonoBehaviour
{
    Rigidbody2D rb;
    public float offset = 0;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (rb == null) return;

        Vector2 vel = rb.linearVelocity;

        if (vel.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, 0, angle + offset);
        }
    }
}