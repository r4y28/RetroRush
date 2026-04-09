using UnityEngine;

public class checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("player"))
        {
            respawn playerRespawn = collision.GetComponent<respawn>();

            if (playerRespawn != null)
            {
                playerRespawn.respawnpoint = transform;
                print("touch");
            }
        }
    }
}
