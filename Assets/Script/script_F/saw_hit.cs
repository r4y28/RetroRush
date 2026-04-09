using UnityEngine;

public class saw_hit : MonoBehaviour
{
    public walls wallscript;

    public bool bloodNeeded = true;

    private int bloodCount = 0;
    public int maxBlood = 5;

    private void OnEnable()
    {
        wallscript = FindAnyObjectByType<walls>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("player"))
        {
            Movement movement = collision.GetComponent<Movement>();
            respawn respawnScript = collision.GetComponent<respawn>();

            // 🔥 Safe check
            if (movement != null)
                movement.BloodFx.Play();

            // 🎯 BLOOD LIMIT
            if (bloodNeeded && bloodCount < maxBlood)
            {
                bloodCount++;

                Vector2 randomOffset = Random.insideUnitCircle * 0.2f;
                Vector2 hitPoint = collision.ClosestPoint(transform.position) + randomOffset;

                ObjectPooling.instance.SpawnFromPool(
                    "Blood",
                    hitPoint,
                    Quaternion.identity,
                    transform
                );
            }

            // 💀 ALWAYS KILL PLAYER
            cameramanager.instance.CameraShake();

            if (respawnScript != null)
                respawnScript.Respawn();

            if (wallscript != null)
                wallscript.walldir = 1;
        }
    }
}