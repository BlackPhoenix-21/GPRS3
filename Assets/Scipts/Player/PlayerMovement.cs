using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input System")]
    public InputActionReference move;
    public InputActionReference jump;
    public InputActionReference dash;

    [Header("Bewegungswerte")]
    public float runSpeed = 15f;
    public float jumpForce = 7f;
    public float doubleJumpForce = 7f;
    public float dashForce = 20f;
    public float airMovement = 0.5f;
    public float maxSpeed = 15f;

    [Header("Dash")]
    public float dashDuration = 0.12f;
    public float dashCooldown = 2.5f;

    [Header("Double Jump")]
    public bool enableDoubleJump = true;
    public float doubleJumpCooldown = 0.5f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckDistance = 0.3f;
    private Rigidbody2D rb;
    public string fall;

    [Header("Debug Information")]
    public bool grounded;
    public float timerDash;
    public float timerDoubleJump;

    [HideInInspector] public int dir = 1;

    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private int moveable = 0;

    private int horizontalInput = 0;
    private bool jumpPressed = false;
    private bool dashPressed = false;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        move.action.Enable();
        jump.action.Enable();
        dash.action.Enable();
    }

    private void OnDisable()
    {
        move.action.Disable();
        jump.action.Disable();
        dash.action.Disable();
    }

    private void Update()
    {
        Vector2 moveInput = move.action.ReadValue<Vector2>();
        float inputX = moveInput.x;
        float deadZone = 0.1f;

        if (moveInput != Vector2.zero)
            anim.SetBool("IsMoving", true);
        else
            anim.SetBool("IsMoving", false);

        horizontalInput = 0;

        if (moveable == 0)
        {
            if (inputX > deadZone)
                horizontalInput = 1;
            else if (inputX < -deadZone)
                horizontalInput = -1;
        }
        else if (moveable == 1)
        {
            if (inputX < -deadZone)
                horizontalInput = -1;
        }
        else if (moveable == -1)
        {
            if (inputX > deadZone)
                horizontalInput = 1;
        }

        if (horizontalInput != 0)
            dir = horizontalInput;

        if (jump.action.WasPressedThisFrame())
            jumpPressed = true;

        if (dash.action.WasPressedThisFrame())
            dashPressed = true;

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0f)
                isDashing = false;
        }

        if (!grounded && !enableDoubleJump && timerDoubleJump > 0f)
            timerDoubleJump -= Time.deltaTime;


        if (grounded && timerDoubleJump <= 0f)
            enableDoubleJump = true;

        timerDash -= Time.deltaTime;

        GetComponent<SpriteRenderer>().flipX = dir < 0;
        anim.SetFloat("VelY", rb.linearVelocityY);
    }

    private void FixedUpdate()
    {
        IsGrounded();

        if (jumpPressed)
        {
            anim.SetTrigger("Jump");
            anim.SetBool("Grounded", false);
            Jump();
            jumpPressed = false;
        }

        if (dashPressed)
        {
            Dash();
            dashPressed = false;
        }

        float moveMultiplier = grounded ? 1f : airMovement;
        float targetSpeed = horizontalInput * runSpeed * moveMultiplier;

        if (isDashing)
        {
            float dashVel = dashForce * dir;
            rb.AddForce(new Vector2(dashVel, 0f), ForceMode2D.Force);
        }

        float speedDifference = targetSpeed - rb.linearVelocity.x;
        float movement = speedDifference * runSpeed;

        rb.AddForce(new Vector2(movement, 0f), ForceMode2D.Force);

        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed && !isDashing)
        {
            rb.linearVelocity = new Vector2(
                Mathf.Sign(rb.linearVelocity.x) * maxSpeed,
                rb.linearVelocity.y
            );
        }
    }

    private void OnCollisionStay2D(Collision2D c)
    {
        if (c.transform.CompareTag(fall))
        {
            Vector2 contactPoint = c.contacts[0].point;
            Vector2 playerPos = transform.position;
            float horizontalDiff = contactPoint.x - playerPos.x;

            if (horizontalDiff < -0.1f)
                moveable = -1;
            else if (horizontalDiff > 0.1f)
                moveable = 1;
            else
                moveable = 0;
        }
        else
        {
            moveable = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (c.transform.CompareTag(fall))
            moveable = 0;
    }

    private void IsGrounded()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            grounded = false;
            return;
        }

        Vector2 origin = col.bounds.center;

        float rayLength = col.bounds.extents.y + groundCheckDistance;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, groundMask);

        grounded = hit.collider != null;
        if (grounded)
            anim.SetBool("Grounded", true);

        Debug.DrawRay(origin, Vector2.down * rayLength, grounded ? Color.green : Color.red);
    }

    private void Dash()
    {
        if (timerDash > 0f)
            return;

        isDashing = true;
        dashTimeLeft = dashDuration;
        timerDash = dashCooldown;
        anim.SetTrigger("Dash");
    }


    private void Jump()
    {
        if (grounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

            enableDoubleJump = true;
            timerDoubleJump = doubleJumpCooldown;
        }
        else if (enableDoubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(new Vector2(0f, doubleJumpForce), ForceMode2D.Impulse);

            enableDoubleJump = false;
        }
    }
}
