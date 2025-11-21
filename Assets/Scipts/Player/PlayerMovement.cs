using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Tastenbelegung")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode dashKey = KeyCode.LeftControl;

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
    public float groundCheckDistance = 0.1f;
    private RaycastHit2D[] groundHits;

    private Rigidbody2D rb;
    [HideInInspector]
    public int dir = 1;

    [Header("Debug Information")]
    public bool grounded;
    public float timerDash;
    public float timerDoubleJump;

    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private int moveable = 0;

    private int horizontalInput = 0;
    private bool jumpPressed = false;
    private bool dashPressed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Input-Handling
        horizontalInput = 0;
        if (moveable == 0)
        {
            if (Input.GetKey(leftKey))
                horizontalInput = -1;
            if (Input.GetKey(rightKey))
                horizontalInput = 1;
        }
        else if (moveable == 1)
        {
            if (Input.GetKey(leftKey))
                horizontalInput = -1;
        }
        else if (moveable == -1)
        {
            if (Input.GetKey(rightKey))
                horizontalInput = 1;
        }

        if (horizontalInput != 0)
            dir = horizontalInput;

        if (Input.GetKeyDown(jumpKey))
        {
            jumpPressed = true;
            Debug.Log("Jump");
        }

        if (Input.GetKeyDown(dashKey))
        {
            dashPressed = true;
            Debug.Log("Dash");
        }

        // Dash-Timer
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0f)
                isDashing = false;
        }

        if (grounded && timerDoubleJump < 0)
        {
            enableDoubleJump = true;
        }

        // Cooldowns
        timerDash -= Time.deltaTime;
        timerDoubleJump -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        IsGrounded();

        if (jumpPressed)
        {
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
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxSpeed, rb.linearVelocity.y);
        }
    }

    private void OnCollisionStay2D(Collision2D c)
    {
        if (c.transform.CompareTag("Wall"))
        {
            // Bestimme die Richtung der Kollision basierend auf Kontaktpunkten
            Vector2 contactPoint = c.contacts[0].point;
            Vector2 playerPos = transform.position;

            // Prüfe ob die Wand links oder rechts vom Spieler ist
            float horizontalDiff = contactPoint.x - playerPos.x;

            if (horizontalDiff < -0.1f)
            {
                // Wand ist links, blockiere Bewegung nach links
                moveable = -1;
            }
            else if (horizontalDiff > 0.1f)
            {
                // Wand ist rechts, blockiere Bewegung nach rechts
                moveable = 1;
            }
            else
            {
                moveable = 0;
            }
        }
        else
        {
            moveable = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (c.transform.CompareTag("Wall"))
        {
            moveable = 0;
        }
    }

    private void IsGrounded()
    {
        Collider2D col = GetComponent<Collider2D>();
        float colLenght = col != null ? col.bounds.size.y * 0.51f : 0.01f;
        Vector2 pos = new Vector2(transform.position.x, transform.position.y - colLenght);

        groundHits = Physics2D.RaycastAll(pos, Vector2.down, groundCheckDistance, groundMask);

        grounded = groundHits.Length > 0;
    }

    private void Dash()
    {
        if (timerDash > 0f)
            return;

        isDashing = true;
        dashTimeLeft = dashDuration;
        timerDash = dashCooldown;
        // Animation
    }

    private void Jump()
    {
        if (grounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            // Animation
        }
        else
        {
            if (enableDoubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(new Vector2(0f, doubleJumpForce), ForceMode2D.Impulse);
                enableDoubleJump = false;
                timerDoubleJump = doubleJumpCooldown;
                // Animation
            }
        }
    }
}
