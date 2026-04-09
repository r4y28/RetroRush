using UnityEngine;
using System.Collections;

public class respawn : MonoBehaviour
{
    public Transform respawnpoint;
    private Rigidbody2D rb;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Respawn()
    {
        StartCoroutine(RespawnDelay());
        rb.linearVelocity = Vector2.zero; // stop motion immediately


        // Reset direction
        Movement mov = GetComponent<Movement>();
        if (mov != null)
        {
            mov.dir = 1; // reset movement direction
            mov.transform.localScale = new Vector3(1, 1, 1); // reset scale
        }
    }

    IEnumerator RespawnDelay()
    {
        transform.localScale = Vector2.one;

        // Hide player
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // Optional: add death animation delay
        yield return new WaitForSeconds(0.3f);

        // Teleport to respawn point
        if (respawnpoint != null)
            transform.position = respawnpoint.position;

        // Reset scale to default (1,1)

        // Show player again
        if (sr != null) sr.enabled = true;
    }
}