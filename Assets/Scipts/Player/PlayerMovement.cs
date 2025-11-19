using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Tastenbelegung")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftShift;
    public KeyCode dashKey = KeyCode.LeftControl;

    [Header("Bewegungswerte")]
    public float runSpeed = 15f;
    public float jumpHeight = 7f;
    public float doubleJumpHeight = 7f;
    public float dashSpeed = 20f;
    public float airMovement = 0.5f;

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
    private int dir = 1;

    [Header("Debug Information")]
    public bool grounded;
    public float timerDash;
    public float timerDoubleJump;

    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private int moveable = 0;
    public float wallCheckDistance = 0.1f;
    private RaycastHit2D[] wallHits;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        IsGrounded();

        int horizontalInput = 0;
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
            Jump();

        if (Input.GetKeyDown(dashKey))
            Dash();

        if (Input.GetKeyDown(crouchKey))
            Crouch(true);
        else if (Input.GetKeyUp(crouchKey))
            Crouch(false);

        if (Input.GetKeyDown(jumpKey))
            Debug.Log("Jump");
        if (Input.GetKeyDown(dashKey))
            Debug.Log("Dash");
        if (Input.GetKeyDown(crouchKey))
            Debug.Log("CrouchOn");
        else if (Input.GetKeyUp(crouchKey))
            Debug.Log("CrouchOff");

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0f)
                isDashing = false;
        }

        // Cooldowns
        timerDash -= Time.deltaTime;
        timerDoubleJump -= Time.deltaTime;

        if (timerDoubleJump < 0 && grounded)
        {
            enableDoubleJump = true;
        }

        Vector2 currentVel = rb.linearVelocity;
        float baseSpeed;
        if (grounded)
        {
            baseSpeed = horizontalInput * runSpeed;
        }
        else
        {
            baseSpeed = horizontalInput * runSpeed * airMovement;
        }

        if (isDashing)
        {
            float dashVel = dashSpeed * dir;
            rb.linearVelocity = new Vector2(baseSpeed + dashVel, currentVel.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(baseSpeed, currentVel.y);
        }
    }

    private void OnCollisionStay2D(Collision2D c)
    {
        if (c.transform.CompareTag("Wall"))
        {
            Debug.Log("Hit");
            float dis = transform.position.y - c.transform.position.y * -1;
            Debug.Log(dis);
            bool stuck;
            Collider2D col = GetComponent<Collider2D>();
            float colLenght = col != null ? col.bounds.size.x * 0.51f : 0.01f;
            Vector2 pos = new Vector2(transform.position.x - colLenght, transform.position.y + dis);

            wallHits = Physics2D.RaycastAll(pos, Vector2.down, wallCheckDistance, groundMask);
            stuck = wallHits != null && wallHits.Length > 0;

            if (stuck)
            {
                Debug.Log("-1");
                moveable = -1;
                return;
            }
            pos = new Vector2(transform.position.x + colLenght, transform.position.y + dis);

            wallHits = Physics2D.RaycastAll(pos, Vector2.down, wallCheckDistance, groundMask);
            stuck = wallHits != null && wallHits.Length > 0;
            if (stuck)
            {
                Debug.Log("1");
                moveable = 1;
                return;
            }

        }
        moveable = 0;
    }

    private void IsGrounded()
    {
        Collider2D col = GetComponent<Collider2D>();
        float colLenght = col != null ? col.bounds.size.y * 0.51f : 0.01f;
        Vector2 pos = new Vector2(transform.position.x, transform.position.y - colLenght);

        groundHits = Physics2D.RaycastAll(pos, Vector2.down, groundCheckDistance, groundMask);

        grounded = groundHits != null && groundHits.Length > 0;

        // Debug: Anzahl und erste Collider-Info (nur im Editor sinnvoll)
        //if (groundHits != null && groundHits.Length > 0)
        //{
        //    Debug.Log($"Ground hits: {groundHits.Length} -> {groundHits[0].collider}");
        //}
        //else
        //{
        //    Debug.Log("Ground hits: 0");
        //}
    }

    private void Crouch(bool down)
    {
        if (!grounded || !down)
        {
            StartCoroutine(CanStandUp());
            return;
        }
        // Speed langsamer?
        // Animation
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
            // Animation
        }
        else
        {
            if (enableDoubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpHeight);
                enableDoubleJump = false;
                timerDoubleJump = doubleJumpCooldown;
                // Animation
            }
        }
    }

    private IEnumerator CanStandUp()
    {
        float rayLength = GetComponent<Collider2D>() != null ? GetComponent<Collider2D>().bounds.size.y : 1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, rayLength);
        while (hit.collider != null)
        {
            yield return new WaitForSeconds(0.1f);
            hit = Physics2D.Raycast(transform.position, Vector2.up, rayLength);
        }
        // Animation
    }
}
