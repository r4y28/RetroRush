using UnityEngine;

public class saw_hit : MonoBehaviour
{
    public walls wallscript;

    public GameObject Blood;

    private void Start()
    {

    }

    private void OnEnable()
    {
        wallscript = FindAnyObjectByType<walls>();
    }

    //public ParticleSystem bloodtest;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("player"))
        {
            // 🎯 Get exact hit position
            Vector2 randomOffset = Random.insideUnitCircle * 0.2f;
            Vector2 hitPoint = collision.ClosestPoint(transform.position) + randomOffset;

            GameObject blood = ObjectPooling.instance.SpawnFromPool(
                "Blood",
                hitPoint,
                Quaternion.identity,
                transform
            );

            cameramanager.instance.CameraShake();
            collision.GetComponent<respawn>().Respawn();
            wallscript.walldir = 1;
        }
    }
}
