using UnityEngine;

public class saw_hit : MonoBehaviour
{
    public walls wallscript;

    public GameObject Blood;
    public bool bloodNeeded = true;

    private int bloodCount = 0;     // 🔥 track blood spawns
    public int maxBlood = 5;        // 🔥 limit blood only

    private void OnEnable()
    {
        wallscript = FindAnyObjectByType<walls>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("player"))
        {
            // 🎯 BLOOD ONLY LIMITED
            if (bloodNeeded && bloodCount < maxBlood)
            {
                bloodCount++; // count only blood

                Vector2 randomOffset = Random.insideUnitCircle * 0.2f;
                Vector2 hitPoint = collision.ClosestPoint(transform.position) + randomOffset;

                collision.gameObject.GetComponent<Movement>().BloodFx.Play();

                ObjectPooling.instance.SpawnFromPool(
                    "Blood",
                    hitPoint,
                    Quaternion.identity,
                    transform
                );

            }
            collision.gameObject.GetComponent<Movement>().BloodFx.Play();

            // 💀 ALWAYS KILL PLAYER
            cameramanager.instance.CameraShake();
            collision.GetComponent<respawn>().Respawn();
            wallscript.walldir = 1;
        }
    }
}