using UnityEngine;
using System.Collections;

public class BossScript : MonoBehaviour
{
    public enum BossStage { Stage1, Stage2, Stage3 }

    [Header("DEBUG")]
    public BossStage currentStage;

    [Header("Refs")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator anim;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashCooldown = 1.5f;
    public float dashDuration = 0.4f;

    [Header("Bounce")]
    public float bounceHeight = 12f;
    public float moveSpeed = 8f;

    [Header("Stun")]
    public float stunTime = 2f;

    [Header("Checks")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    public Transform wallCheck;
    public float wallCheckDistance = 0.6f;
    public LayerMask wallLayer;

    [Header("Stage 3")]
    public Transform centerPoint;
    public float chargeSpeed = 15f;

    public int spikeBurstCount = 12;
    public float spikeForce = 8f;

    public float tiredTime = 2f;

    public float flyHeight = 6f;
    public float spikeFireRate = 0.3f;
    public float spikeAttackDuration = 10f;

    [Header("Fallback")]
    public GameObject spikePrefab;

    bool isGrounded;
    bool isDashing;
    bool isStunned;

    bool stage3Started;
    bool isInStage3;

    float dashTimer;
    float dashTimeLeft;

    float moveDir;
    string currentAnim = "";

    void Start()
    {
        currentStage = BossStage.Stage1;
    }

    void Update()
    {
        CheckGround();

        if (Input.GetKeyDown(KeyCode.Alpha1)) currentStage = BossStage.Stage1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentStage = BossStage.Stage2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentStage = BossStage.Stage3;

        dashTimer -= Time.deltaTime;

        if (isStunned)
        {
            PlayAnim("Rest");
            return;
        }

        // =====================
        // STAGE 3
        // =====================
        if (currentStage == BossStage.Stage3)
        {
            if (!stage3Started)
            {
                StartCoroutine(Stage3Routine());
                stage3Started = true;
            }
            return;
        }

        if (isInStage3) return;

        // =====================
        // NORMAL LOGIC
        // =====================

        if (!isDashing)
            FlipTowardsPlayer();

        if (!isDashing && dashTimer <= 0 && isGrounded)
            StartDash();

        if (isDashing && DetectWall())
        {
            if (currentStage == BossStage.Stage1)
            {
                StopDash();
                StartCoroutine(Stun());
            }
            else
            {
                moveDir *= -1;
                FlipInstant();
            }
        }

        if (currentStage != BossStage.Stage1 && isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceHeight);
            PlayAnim("Roll");
        }
    }

    void FixedUpdate()
    {
        if (!isDashing) return;

        if (currentStage == BossStage.Stage1)
        {
            rb.linearVelocity = new Vector2(moveDir * dashSpeed, rb.linearVelocity.y);

            dashTimeLeft -= Time.fixedDeltaTime;
            if (dashTimeLeft <= 0)
                StopDash();
        }
        else if (currentStage == BossStage.Stage2)
        {
            rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
        }
    }

    // =========================
    // 🔥 STAGE 3 LOGIC
    // =========================

    IEnumerator Stage3Routine()
    {
        isInStage3 = true;

        if (centerPoint == null)
        {
            Debug.LogError("CenterPoint missing!");
            yield break;
        }

        // MOVE TO CENTER (WITH FLIP FIX)
        float moveTimeout = 5f;
        while (Vector2.Distance(transform.position, centerPoint.position) > 0.2f)
        {
            Vector2 dir = (centerPoint.position - transform.position).normalized;
            PlayAnim("Run");
            rb.linearVelocity = dir * chargeSpeed;

            if (Mathf.Abs(dir.x) > 0.05f)
            {
                transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);
            }

            moveTimeout -= Time.deltaTime;
            if (moveTimeout <= 0) break;

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // BURST
        SpawnSpikeBurst();

        // REST
        PlayAnim("Rest");
        yield return new WaitForSeconds(tiredTime);

        PlayAnim("Roll");
        // FLY STRAIGHT UP
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        float lockedX = transform.position.x;
        rb.linearVelocity = new Vector2(0, flyHeight);

        float flyTime = 0.5f;
        while (flyTime > 0)
        {
            transform.position = new Vector2(lockedX, transform.position.y);
            flyTime -= Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // SHOOT PLAYER
        float timer = 0;
        while (timer < spikeAttackDuration)
        {
            ShootSpikeAtPlayer();
            yield return new WaitForSeconds(spikeFireRate);
            timer += spikeFireRate;
        }

        // FALL
        rb.gravityScale = 1;

        isInStage3 = false;

        // LOOP AGAIN
        yield return new WaitForSeconds(1f);
        stage3Started = false;
    }

    void SpawnSpikeBurst()
    {
        for (int i = 0; i < spikeBurstCount; i++)
        {
            float angle = i * Mathf.PI * 2 / spikeBurstCount;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject spike = null;

            if (ObjectPooling.instance != null)
            {
                spike = ObjectPooling.instance.SpawnFromPool(
                    "Spike",
                    transform.position,
                    Quaternion.identity
                );
            }

            if (spike == null && spikePrefab != null)
            {
                spike = Instantiate(spikePrefab, transform.position, Quaternion.identity);
            }

            if (spike != null)
            {
                Rigidbody2D rb2d = spike.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                    rb2d.linearVelocity = dir * spikeForce;
            }
        }
    }

    void ShootSpikeAtPlayer()
    {
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;

        GameObject spike = null;

        if (ObjectPooling.instance != null)
        {
            spike = ObjectPooling.instance.SpawnFromPool(
                "Spike",
                transform.position,
                Quaternion.identity
            );
        }

        if (spike == null && spikePrefab != null)
        {
            spike = Instantiate(spikePrefab, transform.position, Quaternion.identity);
        }

        if (spike != null)
        {
            Rigidbody2D rb2d = spike.GetComponent<Rigidbody2D>();
            if (rb2d != null)
                rb2d.linearVelocity = dir * 10f;
        }
    }

    // =========================

    void StartDash()
    {
        float dir = player.position.x - transform.position.x;
        moveDir = Mathf.Sign(dir == 0 ? transform.localScale.x : dir);

        isDashing = true;
        dashTimer = dashCooldown;
        dashTimeLeft = dashDuration;

        FlipInstant();

        if (currentStage != BossStage.Stage1)
        {
            rb.linearVelocity = new Vector2(moveDir * moveSpeed, bounceHeight);
        }

        PlayAnim("Run");
    }

    void StopDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;
    }

    IEnumerator Stun()
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(stunTime);

        isStunned = false;
        dashTimer = 0f;
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );
    }

    bool DetectWall()
    {
        Vector2 dir = new Vector2(Mathf.Sign(moveDir), 0);

        return Physics2D.Raycast(
            wallCheck.position,
            dir,
            wallCheckDistance,
            wallLayer
        );
    }

    void FlipTowardsPlayer()
    {
        float dir = player.position.x - transform.position.x;
        if (Mathf.Abs(dir) < 0.05f) return;

        transform.localScale = new Vector3(Mathf.Sign(dir), 1, 1);
    }

    void FlipInstant()
    {
        transform.localScale = new Vector3(Mathf.Sign(moveDir), 1, 1);
    }

    void PlayAnim(string name)
    {
        if (currentAnim == name) return;
        anim.Play(name);
        currentAnim = name;
    }
}