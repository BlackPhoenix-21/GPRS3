using System.Collections;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

public class Grossmutter : MonoBehaviour
{
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

    void Start()
    {
        rig2D = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");

        state = GrossmutterState.Idle;
        chargTimer = chargCooldown;
        idleTimer = idleTime;
        hitsTaken = 0;
    }

    void Update()
    {
        dirPlayer = Mathf.Sign(player.transform.position.x - transform.position.x);
        GetComponent<SpriteRenderer>().flipX = dirPlayer < 0;
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
                    walkDirection = Random.Range(0, 2) == 0 ? -1f : 1f;
                }
                break;

            case GrossmutterState.Walk:
                anim.SetBool("IsMoving", true);
                Walk();

                if (Vector3.Distance(walkStartPosition, transform.position) >= walkDistance)
                {
                    state = GrossmutterState.Idle;
                    idleTimer = idleTime;
                }

                chargTimer -= Time.deltaTime;
                if (chargTimer <= 0)
                {
                    state = GrossmutterState.Charge;
                }
                break;

            case GrossmutterState.Charge:
                if (!isCharging)
                {
                    rundir = dirPlayer;
                    anim.SetBool("Dash", true);
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
            // End-Anim
        }

        state = GrossmutterState.Stunned;
    }

    private IEnumerator Charging()
    {
        isCharging = true;

        yield return new WaitForSeconds(chargingTime);

        float chargeTimer = 0f;
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
}
