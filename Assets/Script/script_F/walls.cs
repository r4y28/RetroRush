using UnityEngine;

public class walls : MonoBehaviour
{
    [Header("WallCheck")]
    public Animator anim;
    public Transform wallcheck;
    public float walldis = 0.2f;
    [SerializeField] float slidespeed = -2f;
    public LayerMask ground;
    public bool istouch;
    public Movement movecs;
    bool rw;
    bool lw;
    public int walldir;
    public ParticleSystem wallslidesust;

    private void Update()
    {
        // Wall sliding
        if (istouch && !movecs.isGrounded && movecs.rb.linearVelocity.y < 0)
        {
            movecs.rb.linearVelocity = new Vector2(movecs.rb.linearVelocity.x, slidespeed);
            if (!wallslidesust.isPlaying)
                wallslidesust.Play();
        }
        else
        {
            wallslidesust.Stop();
        }
    }

    private void FixedUpdate()
    {
        if(istouch)
        WallIdle();

        rw = Physics2D.Raycast(wallcheck.position, wallcheck.right, walldis, ground);
        lw = Physics2D.Raycast(wallcheck.position, -wallcheck.right, walldis, ground);
        istouch = rw || lw;

    }

    void WallIdle()
    {
       

        if (rw)
            walldir = 1;
        else if (lw)
            walldir = -1;

    }

    private void OnDrawGizmos()
    {
        if (wallcheck == null) return;
        Gizmos.DrawLine(wallcheck.position, wallcheck.position + transform.right * walldis);
        Gizmos.DrawLine(wallcheck.position, wallcheck.position - transform.right * walldis);
    }

  
}