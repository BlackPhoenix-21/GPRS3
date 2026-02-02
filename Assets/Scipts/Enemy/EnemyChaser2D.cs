using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyChaserHybrid2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Move")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float aggroRange = 40f;
    [SerializeField] private float stopRange = 0.6f;

    [Header("Jump points")]
    [SerializeField] private bool useJumpPoints = true;

    [Header("Jump (auto: wall/hazards)")]
    [SerializeField] private float autoJumpUpVelocity = 12f;
    [SerializeField] private float autoJumpForwardVelocity = 8f;
    [SerializeField] private float jumpCooldown = 0.25f;

    [Header("Air Control")]
    [SerializeField] private float airTargetX = 8.5f;
    [SerializeField] private float airAccel = 70f;
    [SerializeField] private float maxAirX = 12f;

    [Header("Checks")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask hazardLayer;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.12f;

    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallDistance = 0.9f;

    [SerializeField] private Transform hazardCheck;
    [SerializeField] private float hazardDistance = 0.9f;

    [Header("Offscreen catch-up")]
    [SerializeField] private float offscreenSeconds = 3f;
    [SerializeField] private float respawnMarginX = 2f;
    [SerializeField] private float respawnYOffset = 0f;
    [SerializeField] private float respawnGroundSearchDown = 10f;

    [Header("Finish stop")]
    [SerializeField] private bool stopWhenFinished = true;

    [Header("Attack")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float damageCooldown = 0.8f;

    [Header("Hazard box check")]
    [SerializeField] private Vector2 hazardBoxSize = new Vector2(0.6f, 0.6f);

    [SerializeField] private SpriteRenderer sprite;


    private Rigidbody2D rb;
    private bool grounded;
    private int dir = 1;

    private float jumpTimer;
    private float damageTimer;
    private float offscreenTimer;

    private JumpPoint2D currentJumpPoint;
    private bool jumpingByPoint;

    private Animator anim;
    public float groundCheckDistance = 1f;
    public LayerMask groundMask;

    private void Awake()
    {
     rb = GetComponent<Rigidbody2D>();
      anim = GetComponent<Animator>();

        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        jumpTimer -= Time.deltaTime;
        damageTimer -= Time.deltaTime;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }
        IsGrounded();
    }

    [System.Obsolete]
    private void FixedUpdate()
    {
        //Debug.Log($"grounded={grounded}, jumpTimer={jumpTimer}, hasJP={(currentJumpPoint != null)}, wallCheck={(wallCheck != null)}");

        if (stopWhenFinished && FinishZone.Finished)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (player == null || groundCheck == null || wallCheck == null || hazardCheck == null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }


        if (grounded && jumpingByPoint)
        {
            jumpingByPoint = false;
        }


        float dx = player.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        if (dist > aggroRange)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (dist <= stopRange)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        dir = dx >= 0 ? 1 : -1;
        if (sprite != null)
        sprite.flipX = (dir < 0);


        if (IsOffscreen())
        {
            offscreenTimer += Time.fixedDeltaTime;
            if (offscreenTimer >= offscreenSeconds)
            {
                TeleportOutsideCameraLeft();
                offscreenTimer = 0f;
                return;
            }
        }
        else offscreenTimer = 0f;

        if (!grounded)
        {
            if (jumpingByPoint) return;

            float target = airTargetX * dir;
            float newX = Mathf.MoveTowards(rb.velocity.x, target, airAccel * Time.fixedDeltaTime);
            newX = Mathf.Clamp(newX, -maxAirX, maxAirX);
            rb.velocity = new Vector2(newX, rb.velocity.y);
            return;
        }

        if (useJumpPoints && currentJumpPoint != null && currentJumpPoint.landing != null && jumpTimer <= 0f)
        {
            Vector2 v = currentJumpPoint.CalculateVelocity(transform.position, rb);
            rb.velocity = v;

            jumpingByPoint = true;
            anim.SetTrigger("Jump");

            jumpTimer = jumpCooldown;
            currentJumpPoint = null;
            return;
        }

        bool wallAhead = Physics2D.Raycast(wallCheck.position, Vector2.right * dir, wallDistance, groundLayer);

        Vector2 hazardBoxCenter = (Vector2)hazardCheck.position + new Vector2(dir * hazardDistance, 0f);
        bool hazardAhead = Physics2D.OverlapBox(hazardBoxCenter, hazardBoxSize, 0f, hazardLayer);

        bool allowAutoJump = !useJumpPoints || currentJumpPoint == null;

        if (allowAutoJump && (wallAhead || hazardAhead) && jumpTimer <= 0f)
        {
            rb.velocity = new Vector2(dir * autoJumpForwardVelocity, autoJumpUpVelocity);
            anim.SetTrigger("Jump");
            jumpTimer = jumpCooldown;
            return;
        }

        rb.velocity = new Vector2(moveSpeed * dir, rb.velocity.y);

        anim.SetBool("Grounded", grounded);
    }

    private bool IsOffscreen()
    {
        if (Camera.main == null) return false;
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        return vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.2f || vp.y > 1.2f;
    }

    [System.Obsolete]
    private void TeleportOutsideCameraLeft()
    {
        if (Camera.main == null || player == null) return;

        float z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 leftBottom = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, z));

        float spawnX = leftBottom.x - respawnMarginX;

        Vector2 origin = new Vector2(spawnX, player.position.y + respawnYOffset);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, respawnGroundSearchDown, groundLayer);

        Vector2 spawnPos = origin;
        if (hit.collider != null) spawnPos.y = hit.point.y + 0.6f;

        transform.position = spawnPos;
        rb.velocity = Vector2.zero;

        currentJumpPoint = null;
        jumpTimer = 0.1f;
        jumpingByPoint = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var jp = other.GetComponent<JumpPoint2D>();
        if (jp != null && jp.landing != null)
        {
            currentJumpPoint = jp;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var jp = other.GetComponent<JumpPoint2D>();
        if (jp != null && currentJumpPoint == jp)
        {
            currentJumpPoint = null;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;
        if (damageTimer > 0f) return;

        var pc = collision.collider.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            pc.TakeDamage(contactDamage);
            damageTimer = damageCooldown;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);

        if (wallCheck != null)
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * wallDistance);

        if (hazardCheck != null)
        {
            Vector3 cR = hazardCheck.position + new Vector3(hazardDistance, 0f, 0f);
            Vector3 cL = hazardCheck.position + new Vector3(-hazardDistance, 0f, 0f);
            Gizmos.DrawWireCube(cR, hazardBoxSize);
            Gizmos.DrawWireCube(cL, hazardBoxSize);
        }
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
}
