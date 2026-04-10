using UnityEngine;
using System.Collections;

public class BossScript : MonoBehaviour
{
    public enum BossStage { Stage1, Stage2, Stage3 }

    [Header("DEBUG")]
    public BossStage currentStage;

    [Header("Health")]
    public int maxHealth = 99;
    private int currentHealth;

    private int totalDamageTaken = 0;
    private int damagePerStage = 33;
    private int nextStunThreshold;

    [Header("Refs")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer sr;

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

    public GameObject spikePrefab;

    [Header("Damage")]
    public Collider2D damageCollider;

    bool isGrounded;
    bool isDashing;
    bool isStunned;
    bool isInStage3;

    float dashTimer;
    float dashTimeLeft;

    float moveDir;
    string currentAnim = "";

    // =========================
    void Start()
    {
        currentHealth = maxHealth;
        currentStage = BossStage.Stage1;

        nextStunThreshold = damagePerStage;

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        CheckGround();
        dashTimer -= Time.deltaTime;

        if (isStunned)
        {
            PlayAnim("Rest");
            return;
        }

        // STAGE 3
        if (currentStage == BossStage.Stage3)
        {
            if (!isInStage3)
                StartCoroutine(Stage3Routine());
            return;
        }

        // NORMAL LOGIC
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

        // STAGE 2 BOUNCE
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
    // 💥 DAMAGE SYSTEM (FIXED)
    // =========================
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        totalDamageTaken += damage;

        Debug.Log("Boss HP: " + currentHealth);

        // 🔥 STAGE 2 ENTER
        if (totalDamageTaken >= damagePerStage && currentStage == BossStage.Stage1)
        {
            currentStage = BossStage.Stage2;
            rb.linearVelocity = Vector2.zero;

            ForceStun();
        }

        // 💀 STAGE 3 ENTER
        if (totalDamageTaken >= damagePerStage * 2 && currentStage == BossStage.Stage2)
        {
            // FORCE LAST STUN BEFORE PHASE CHANGE
            ForceStun();

            currentStage = BossStage.Stage3;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 🔁 REPEATING STUN IN STAGE 2
        if (currentStage == BossStage.Stage2 && totalDamageTaken >= nextStunThreshold)
        {
            nextStunThreshold += (damagePerStage / 2); // more frequent stun
            ForceStun();
        }

        // 💀 FINAL DEATH
        if (currentHealth <= 0 && currentStage == BossStage.Stage3)
        {
            Die();
        }
    }

    // =========================
    // 💥 FORCE STUN (IMPORTANT)
    // =========================
    void ForceStun()
    {
        StopCoroutine("Stun");
        StartCoroutine(Stun());
    }

    IEnumerator Stun()
    {
        isStunned = true;

        if (damageCollider != null)
            damageCollider.enabled = false;

        rb.linearVelocity = Vector2.zero;
        PlayAnim("Rest");

        yield return new WaitForSeconds(stunTime);

        isStunned = false;

        if (damageCollider != null)
            damageCollider.enabled = true;

        dashTimer = 0f;
    }

    void Die()
    {
        Debug.Log("BOSS FINAL DEATH");
        Destroy(gameObject);
    }

    // =========================
    // 💀 STAGE 3
    // =========================
    IEnumerator Stage3Routine()
    {
        isInStage3 = true;

        while (Vector2.Distance(transform.position, centerPoint.position) > 0.2f)
        {
            Vector2 dir = (centerPoint.position - transform.position).normalized;

            PlayAnim("Run");
            rb.linearVelocity = dir * chargeSpeed;

            if (Mathf.Abs(dir.x) > 0.05f)
                transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        SpawnSpikeBurst();

        PlayAnim("Rest");
        yield return new WaitForSeconds(tiredTime);

        PlayAnim("Roll");

        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        float lockedX = transform.position.x;

        rb.linearVelocity = new Vector2(0, flyHeight);
        yield return new WaitForSeconds(0.5f);

        rb.linearVelocity = Vector2.zero;

        float timer = 0;
        while (timer < spikeAttackDuration)
        {
            transform.position = new Vector2(lockedX, transform.position.y);

            ShootSpikeAtPlayer();
            yield return new WaitForSeconds(spikeFireRate);
            timer += spikeFireRate;
        }

        rb.gravityScale = 1;
        isInStage3 = false;
    }

    void SpawnSpikeBurst()
    {
        for (int i = 0; i < spikeBurstCount; i++)
        {
            float angle = i * Mathf.PI * 2 / spikeBurstCount;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject spike = Instantiate(spikePrefab, transform.position, Quaternion.identity);

            Rigidbody2D rb2d = spike.GetComponent<Rigidbody2D>();
            if (rb2d != null)
                rb2d.linearVelocity = dir * spikeForce;
        }
    }

    void ShootSpikeAtPlayer()
    {
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;

        GameObject spike = Instantiate(spikePrefab, transform.position, Quaternion.identity);

        Rigidbody2D rb2d = spike.GetComponent<Rigidbody2D>();
        if (rb2d != null)
            rb2d.linearVelocity = dir * 10f;
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

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    bool DetectWall()
    {
        Vector2 dir = new Vector2(Mathf.Sign(moveDir), 0);
        return Physics2D.Raycast(wallCheck.position, dir, wallCheckDistance, wallLayer);
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