using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Movement : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float jumpf;
    public ParticleSystem BloodFx;
    
    public Rigidbody2D rb;
    public bool isGrounded;
    private CapsuleCollider2D Box;
    public LayerMask ground;
    private Animator anim;

    public walls wallscs;
    public slide SlideScript;
    public attack AttackScript;

    [Header("Coyote Time")]
    [SerializeField] float coyoteTime = 0.15f;
    float coyoteTimer;

    [Header("walls jump")]
    [SerializeField] float walljumpx = 12f;
    [SerializeField] float walljumpy = 14f;

    bool iswalljump;
    public int dir = 1;

    public float fallfast = 2.5f;
    public float lowjump = 2f;

    [Header("Dash")]
    [SerializeField] float dashfor = 20f;
    [SerializeField] float dashTime = 0.2f;
    [SerializeField] float dashCooldown = 1f;

    [Header("particle")]
    public ParticleSystem rundust;
    public ParticleSystem jumpdust;
    public ParticleSystem falldust;

    bool isDashing;
    bool canDash = true;
    bool falling;
    bool isAttacking;

    public List<TrailRenderer> tr;

    // 🔥 INPUT SYSTEM
    public PlayerInputActions input;
    Vector2 moveInput;
    bool jumpPressed;
    bool dashPressed;
    bool attackPressed;
    bool slidePressed;
    public bool autoRun;

    // 🔥 UI BUTTON
    public bool jumpButton;
    // 🔥 UI BUTTON STATES
    bool leftHeld;
    bool rightHeld;
    bool slideHeld;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Box = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();

        input = new PlayerInputActions();
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Jump.performed += ctx => jumpPressed = true;
        input.Player.Dash.performed += ctx => dashPressed = true;
        input.Player.Attack.performed += ctx => attackPressed = true;

        input.Player.Slide.performed += ctx => slidePressed = true;
        input.Player.Slide.canceled += ctx => slidePressed = false;
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Update()
    {
        groundcheck();
        Anims();
        jump();
        dash();

        if (Input.GetKeyDown(KeyCode.R))
        {
            autoRun = !autoRun;
        }

        // ATTACK
        if (attackPressed && !isAttacking)
        {
            StartCoroutine(DoAttack());
        }

        // SLIDE
        SlideScript.groundslide(slidePressed || slideHeld);

        // FALL DUST
        if (!isGrounded)
        {
            falling = true;
        }

        if (isGrounded && falling)
        {
            falldust.Play();
            falling = false;
        }

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // RESET INPUT FLAGS
        jumpPressed = false;
        dashPressed = false;
        attackPressed = false;
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        move();

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallfast - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !IsJumpHeld())
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowjump - 1) * Time.fixedDeltaTime;
        }
    }

    private void move()
    {
        if (!iswalljump)
        {
            float xInput = moveInput.x;

            // 🔥 UI override
            if (leftHeld) xInput = -1;
            if (rightHeld) xInput = 1;

            if (autoRun)
            {
                rb.linearVelocity = new Vector2(speed * dir, rb.linearVelocity.y);
            }
            else
            {
                if (xInput > 0.1f)
                    dir = 1;
                else if (xInput < -0.1f)
                    dir = -1;

                rb.linearVelocity = new Vector2(speed * xInput, rb.linearVelocity.y);
            }
        }

        transform.localScale = new Vector3(dir, 1, 1);

        bool isMoving = autoRun ? true : Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        if (isGrounded && isMoving && !IsJumpHeld())
        {
            if (!rundust.isPlaying)
                rundust.Play();
        }
        else
        {
            rundust.Stop();
        }
    }
    private void jump()
    {
        // 🔥 NORMAL JUMP (keyboard + UI)
        if ((jumpPressed || jumpButton) && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpf);
            jumpdust.Play();
            anim.Play("jump");

            coyoteTimer = 0f;
        }

        // 🔥 VARIABLE HEIGHT
        if (!IsJumpHeld() && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // 🔥 WALL JUMP
        if ((jumpPressed || jumpButton) && wallscs.istouch)
        {
            iswalljump = true;

            dir = -wallscs.walldir;

            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = new Vector2(dir * walljumpx, walljumpy);

            Invoke("stop", 0.2f);
        }
    }

    // 🔥 COMBINED INPUT CHECK
    bool IsJumpHeld()
    {
        return Keyboard.current.spaceKey.isPressed || jumpButton;
    }

    // 🔥 UI BUTTON EVENTS
    public void JumpDown()
    {
        jumpButton = true;
        jumpPressed = true;
    }

    public void JumpUp()
    {
        jumpButton = false;
    }

    IEnumerator DoAttack()
    {
        isAttacking = true;

        anim.Play("attack");
        AttackScript.attacktouch();

        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
    }

    public void stop()
    {
        iswalljump = false;
    }

    private void dash()
    {
        if (dashPressed && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(dir * dashfor, 0f);

        foreach (TrailRenderer trail in tr)
            trail.emitting = true;

        yield return new WaitForSeconds(dashTime);

        foreach (TrailRenderer trail in tr)
            trail.emitting = false;

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Anims()
    {
        if (isAttacking) return;

        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0)
            {
                anim.Play("jump");
            }
            else
            {
                if (!wallscs.istouch && !SlideScript.isslide)
                    anim.Play("fall");

                if (wallscs.istouch)
                    anim.Play("wallslide");
            }
            return;
        }

        if (SlideScript.isslide)
        {
            anim.Play("slids");
            return;
        }

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            anim.Play("run");
        }
        else
        {
            anim.Play("idle");
        }
    }
    // ================= UI BUTTONS =================

    // LEFT
    public void LeftDown()
    {
        leftHeld = true;
    }

    public void LeftUp()
    {
        leftHeld = false;
    }

    // RIGHT
    public void RightDown()
    {
        rightHeld = true;
    }

    public void RightUp()
    {
        rightHeld = false;
    }

    // ATTACK (tap)
    public void AttackButton()
    {
        attackPressed = true;
    }

    // DASH (tap)
    public void DashButton()
    {
        dashPressed = true;
    }

    // SLIDE (hold)
    public void SlideDown()
    {
        slideHeld = true;
        slidePressed = true;
    }

    public void SlideUp()
    {
        slideHeld = false;
        slidePressed = false;
    }
    private void groundcheck()
    {
        isGrounded = Physics2D.CapsuleCast(
            Box.bounds.center,
            Box.bounds.size * 0.9f,
            CapsuleDirection2D.Vertical,
            0f,
            Vector2.down,
            0.3f,
            ground
        );
    }

}
