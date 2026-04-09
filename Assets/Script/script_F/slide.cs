using UnityEngine;

public class slide : MonoBehaviour
{
    public Rigidbody2D rb;
    public CapsuleCollider2D box;
    public bool isslide;

    Vector2 normalSize;
    Vector2 slideSize;

    public ParticleSystem slidedust;

    private void Start()
    {
        normalSize = box.size;
        slideSize = new Vector2(box.size.x, box.size.y * 0.7f);
    }

    public void groundslide(bool inputHeld)
    {
        if (inputHeld)
        {
            if (!isslide)
            {
                isslide = true;

                box.size = slideSize;

                rb.linearVelocity = new Vector2(rb.transform.localScale.x * 10f, rb.linearVelocity.y);

                if (!slidedust.isPlaying)
                    slidedust.Play();
            }
        }
        else
        {
            if (isslide)
            {
                isslide = false;

                box.size = normalSize;
                slidedust.Stop();
            }
        }
    }
}