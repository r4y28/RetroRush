using UnityEngine;
using System.Collections;

public class BossScript : MonoBehaviour
{
    public enum BossStage { Stage1, Stage2, Stage3 }
    private BossStage currentStage;

    [Header("References")]
    public Transform player;
    public Rigidbody2D rb;

    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashCooldown = 1.5f;
    public float dashDuration = 0.4f;

    [Header("Stage Multipliers")]
    public float stage2SpeedMultiplier = 1.3f;
    public float stage3SpeedMultiplier = 1.7f;

    [Header("Stun")]
    public float stunTime = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Wall Detection")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.6f;
    public LayerMask wallLayer;

    private bool isGrounded;
    private bool isDashing;
    private bool isStunned;

    private float dashTimer;
    private float dashTimeLeft;

    private Vector2 dashDirection;

    void Start()
    {
        currentHealth = maxHealth;
        currentStage = BossStage.Stage1;
        dashTimer = 0f;
    }

    void Update()
    {
        CheckGround();
        HandleStageTransition();

        dashTimer -= Time.deltaTime;

        if (isStunned)
        {
            FlipTowardsPlayer(); // still look at player while stunned
            return;
        }

        // 🔥 ALWAYS FACE PLAYER WHEN NOT DASHING
        if (!isDashing)
        {
            FlipTowardsPlayer();
        }

        if (!isDashing && dashTimer <= 0 && isGrounded)
        {
            StartDash();
        }

        if (isDashing && !isStunned && DetectWall())
        {
            StopDash();
            StartCoroutine(Stun());
        }
    }
    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection.x * GetCurrentSpeed(), rb.linearVelocity.y);

            dashTimeLeft -= Time.fixedDeltaTime;

            if (dashTimeLeft <= 0)
            {
                StopDash();
            }
        }
    }

    // 🔥 DASH
    void StartDash()
    {
        if (player == null) return;

        float directionX = player.position.x - transform.position.x;

        // FIX: prevent zero direction
        if (Mathf.Abs(directionX) < 0.1f)
            directionX = transform.localScale.x;

        directionX = Mathf.Sign(directionX);

        dashDirection = new Vector2(directionX, 0f);

        isDashing = true;
        dashTimer = dashCooldown;
        dashTimeLeft = dashDuration;

        transform.localScale = new Vector3(directionX, 1, 1);
    }

    void StopDash()
    {
        isDashing = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    // 🔥 GROUND
    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );
    }

    // 🔥 WALL DETECTION (FIXED)
    bool DetectWall()
    {
        Vector2 dir = new Vector2(Mathf.Sign(dashDirection.x), 0f);

        RaycastHit2D hit = Physics2D.Raycast(
            wallCheck.position,
            dir,
            wallCheckDistance,
            wallLayer
        );

        // DEBUG
        Debug.DrawRay(wallCheck.position, dir * wallCheckDistance, Color.red);

        return hit.collider != null;
    }

    // 🔥 STUN
    IEnumerator Stun()
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero;

        Debug.Log("STUN START");

        yield return new WaitForSeconds(GetStunTime());

        isStunned = false;

        Debug.Log("STUN END");

        dashTimer = 0f;
    }

    float GetStunTime()
    {
        if (currentStage == BossStage.Stage3)
            return stunTime * 0.6f;

        return stunTime;
    }

    // 🔥 STAGES
    void HandleStageTransition()
    {
        float hpPercent = currentHealth / maxHealth;

        if (hpPercent <= 0.33f && currentStage != BossStage.Stage3)
        {
            currentStage = BossStage.Stage3;
            Debug.Log("Stage 3!");
        }
        else if (hpPercent <= 0.67f && currentStage != BossStage.Stage2)
        {
            currentStage = BossStage.Stage2;
            Debug.Log("Stage 2!");
        }
    }

    float GetCurrentSpeed()
    {
        switch (currentStage)
        {
            case BossStage.Stage2:
                return dashSpeed * stage2SpeedMultiplier;

            case BossStage.Stage3:
                return dashSpeed * stage3SpeedMultiplier;

            default:
                return dashSpeed;
        }
    }
    void FlipTowardsPlayer()
    {
        if (player == null) return;

        float directionX = player.position.x - transform.position.x;

        if (Mathf.Abs(directionX) < 0.05f) return;

        float sign = Mathf.Sign(directionX);

        transform.localScale = new Vector3(sign, 1, 1);
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
}