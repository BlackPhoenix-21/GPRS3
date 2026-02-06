using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Grossmutter : MonoBehaviour
{
    [Header("End Portal")]
    public EndBookPortal endPortal;

    public enum GrossmutterState
    {
        None,
        Idle,
        Walk,
        Charge,
        Stunned
    }

    public GrossmutterState state = GrossmutterState.None;
    public float mapBorder = 8f;

    [Header("Charge")]
    public float chargCooldown = 10f;
    public float chargingTime = 2f;
    public float chargeSpeed = 6f;
    public float chargeDuration = 1.5f;

    [Header("Walk")]
    public float walkSpeed = 2f;
    public float walkDistance = 1f;

    [Header("Idle/Stun")]
    public float idleTime = 2.5f;
    public float stunTime = 2f;

    public Image img;

    private float stunTimer;
    private float chargTimer;
    private float dirPlayer;
    private float idleTimer;
    private bool isCharging = false;
    private Coroutine chargingCoroutine;
    private Vector3 walkStartPosition;
    private float walkDirection;

    private Rigidbody2D rig2D;
    private GameObject player;
    private float rundir;
    private int hitsTaken;
    private Animator anim;
    private Collider2D col;
    private bool groundedL = true;
    private bool groundedR = false;
    public LayerMask groundMask;
    private float groundCheckDistance = 0.3f;
    public bool hit;
    private bool wallR;
    private bool wallL;
    public LayerMask wallmask;
    private float wallCheckDistance = 0.5f;

    void Start()
    {
        rig2D = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        player = GameObject.FindWithTag("Player");

        state = GrossmutterState.Idle;
        chargTimer = chargCooldown;
        idleTimer = idleTime;
        hitsTaken = 0;

        img.fillAmount = (4 - hitsTaken) / 4f;
    }

    void Update()
    {
        IsGrounded();
        if (groundedL || groundedR)
            rig2D.gravityScale = 0f;
        else
            rig2D.gravityScale = 1f;

        if (rig2D.gravityScale == 0f)
            rig2D.linearVelocityY = 0f;

        Wall();
        if (wallL || wallR)
        {
            transform.position += new Vector3(wallL ? 0.1f : -0.1f, 0, 0);
            try
            {
                StopCoroutine(chargingCoroutine);
            }
            catch (Exception ex)
            {
                print(ex.Message);
            }
            rig2D.linearVelocity = Vector2.zero;
            anim.SetBool("Dash", false);
            state = GrossmutterState.Idle;
            chargTimer = chargCooldown;
            idleTimer = idleTime;
            isCharging = false;
            chargingCoroutine = null;
            print("xjkfjasfkjawk");
        }

        if (hit)
        {
            print("hit");
            hit = false;
            hitsTaken++;
        }
        dirPlayer = Mathf.Sign(player.transform.position.x - transform.position.x);
        GetComponent<SpriteRenderer>().flipX = dirPlayer > 0;
        img.fillAmount = (4 - hitsTaken) / 4f;
        switch (state)
        {
            case GrossmutterState.Stunned:
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    state = GrossmutterState.Idle;
                    idleTimer = idleTime;
                    anim.SetBool("Stunned", false);
                }
                break;

            case GrossmutterState.Idle:
                idleTimer -= Time.deltaTime;
                chargTimer -= Time.deltaTime;
                if (idleTimer <= 0)
                {
                    state = GrossmutterState.Walk;
                    walkStartPosition = transform.position;
                    walkDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1f : 1f;
                }
                break;

            case GrossmutterState.Walk:
                anim.SetBool("IsMoving", true);
                Walk();

                if (Vector3.Distance(walkStartPosition, transform.position) >= walkDistance)
                {
                    anim.SetBool("IsMoving", false);
                    state = GrossmutterState.Idle;
                    idleTimer = idleTime;
                }

                chargTimer -= Time.deltaTime;
                if (chargTimer <= 0)
                {
                    anim.SetBool("IsMoving", false);
                    state = GrossmutterState.Charge;
                }
                break;

            case GrossmutterState.Charge:
                if (!isCharging)
                {
                    rundir = dirPlayer;
                    chargingCoroutine = StartCoroutine(Charging());
                }
                break;
        }
    }

    public void Stunned()
    {
        anim.SetTrigger("Stun");
        if (isCharging && chargingCoroutine != null)
        {
            StopCoroutine(chargingCoroutine);
            anim.SetBool("Dash", false);
            chargingCoroutine = null;
            isCharging = false;
        }

        rig2D.linearVelocity = new Vector2(0, rig2D.linearVelocity.y);
        stunTimer = stunTime;
        chargTimer = chargCooldown;
        isCharging = false;

        hitsTaken++;
        if (hitsTaken >= 4)
        {
            GameManager.Instance.won = true;
            print("death");
            if (endPortal != null)
                endPortal.ActivatePortal();
            rig2D.linearVelocity = Vector2.zero;

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            enabled = false;
            return;
        }
        state = GrossmutterState.Stunned;
    }

    private IEnumerator Charging()
    {
        isCharging = true;
        anim.SetTrigger("DashStart");
        yield return new WaitForSeconds(chargingTime);

        float chargeTimer = 0f;

        anim.SetBool("Dash", true);
        while (chargeTimer < chargeDuration)
        {
            rig2D.linearVelocity = new Vector2(chargeSpeed * rundir, rig2D.linearVelocity.y);
            chargeTimer += Time.deltaTime;
            yield return null;
        }

        rig2D.linearVelocity = new Vector2(0, rig2D.linearVelocity.y);

        state = GrossmutterState.Idle;
        chargTimer = chargCooldown;
        idleTimer = idleTime;
        isCharging = false;
        chargingCoroutine = null;
        anim.SetBool("Dash", false);
    }

    private void Walk()
    {
        Vector3 moveDirection = new Vector3(walkDirection * walkSpeed * Time.deltaTime, 0, 0);
        Vector3 newPosition = transform.position + moveDirection;

        if (Mathf.Abs(newPosition.x) >= mapBorder)
        {
            walkDirection = -walkDirection;
        }
        else
        {
            transform.Translate(moveDirection);
        }
    }

    private void IsGrounded()
    {
        if (col == null)
        {
            groundedL = false;
            groundedR = false;
            return;
        }

        float rayLength = groundCheckDistance;

        // Linke untere Ecke pr�fen - Start von der unteren Kante
        Vector2 leftBottom = new Vector2(col.bounds.min.x + 0.05f, col.bounds.min.y);
        RaycastHit2D hitLeft = Physics2D.Raycast(leftBottom, Vector2.down, rayLength, groundMask);

        // Rechte untere Ecke pr�fen - Start von der unteren Kante
        Vector2 rightBottom = new Vector2(col.bounds.max.x - 0.05f, col.bounds.min.y);
        RaycastHit2D hitRight = Physics2D.Raycast(rightBottom, Vector2.down, rayLength, groundMask);

        groundedL = hitLeft.collider != null;
        groundedR = hitRight.collider != null;

        Debug.DrawRay(leftBottom, Vector2.down * rayLength, groundedL ? Color.yellow : Color.blue);
        Debug.DrawRay(rightBottom, Vector2.down * rayLength, groundedR ? Color.green : Color.red);
    }

    private void Wall()
    {
        if (col == null)
        {
            wallR = false;
            wallL = false;
            return;
        }

        float rayLength = wallCheckDistance;

        // Linke Wand pr�fen - vom Mittelpunkt nach links
        Vector2 leftCenter = new Vector2(col.bounds.min.x, col.bounds.center.y);
        RaycastHit2D hitLeft = Physics2D.Raycast(leftCenter, Vector2.left, rayLength, wallmask);

        // Rechte Wand pr�fen - vom Mittelpunkt nach rechts
        Vector2 rightCenter = new Vector2(col.bounds.max.x, col.bounds.center.y);
        RaycastHit2D hitRight = Physics2D.Raycast(rightCenter, Vector2.right, rayLength, wallmask);

        wallL = hitLeft.collider != null;
        wallR = hitRight.collider != null;

        Debug.DrawRay(leftCenter, Vector2.left * rayLength, wallL ? Color.yellow : Color.blue);
        Debug.DrawRay(rightCenter, Vector2.right * rayLength, wallR ? Color.green : Color.red);
    }
}
