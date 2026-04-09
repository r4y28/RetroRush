using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public Transform player;

    [Header("Settings")]
    public float speed = 3f;
    public bool isFlying = false;

    [Header("Ground Settings")]
    public float stoppingDistance = 0.5f;

    [Header("Flying Dash Settings")]
    public float dashSpeed = 10f;
    public float dashDistance = 3f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 1.5f;

    private Rigidbody2D rb;

    private bool isDashing = false;
    private float dashTimer;
    private float cooldownTimer;
    private Vector2 dashDirection;


    [Header("Flying Behavior")]
    public float hoverHeight = 3f; // how high above player
    public float hoverSmooth = 2f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isFlying)
        {
            HandleFlying();
        }
        else
        {
            MoveOnGround();
        }
    }

    void HandleFlying()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        // Handle dash cooldown
        if (cooldownTimer > 0)
            cooldownTimer -= Time.fixedDeltaTime;

        if (isDashing)
        {
            Dash();
            return;
        }

        // Trigger dash if close
        if (distance < dashDistance && cooldownTimer <= 0)
        {
            StartDash();
        }
        else
        {
            FlyTowardsPlayer();
        }
    }

    void FlyTowardsPlayer()
    {
        Vector2 targetPos = new Vector2(
            player.position.x,
            player.position.y + hoverHeight
        );

        Vector2 newPos = Vector2.Lerp(
            rb.position,
            targetPos,
            hoverSmooth * Time.fixedDeltaTime
        );

        rb.MovePosition(newPos);
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;

        dashDirection = (player.position - transform.position).normalized;
    }

    void Dash()
    {
        rb.linearVelocity = dashDirection * dashSpeed;

        dashTimer -= Time.fixedDeltaTime;

        if (dashTimer <= 0)
        {
            isDashing = false;
        }
    }

    void MoveOnGround()
    {
        float distanceX = player.position.x - transform.position.x;

        if (Mathf.Abs(distanceX) > stoppingDistance)
        {
            float moveX = Mathf.Sign(distanceX);
            rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }
}