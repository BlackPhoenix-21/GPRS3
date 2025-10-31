using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class AutoRunnerWithGravity2D : MonoBehaviour
{
    [Header("Autorun am Boden")]
    public float runSpeed = 7f;             // целевая скорость вдоль поверхности
    public float groundStickForce = 25f;    // прижим к земле (добавочный вниз)
    public float maxAirControl = 2f;        // контроль в воздухе по X

    [Header("Prallen")]
    public float jumpHeight = 3.2f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.12f;
    public float fallGravityMult = 1.8f;
    public float lowJumpMult = 2.0f;

    [Header("Rutschen/Hocken")]
    public bool enableSlide = true;
    public KeyCode crouchKey = KeyCode.S;
    public float crouchScaleY = 0.7f;
    public float crouchBlend = 0.08f;

    [Header("Dash (короткий рывок вперёд)")]
    public bool enableDash = true;
    public KeyCode dashKey = KeyCode.LeftShift;
    public float dashSpeed = 14f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.5f;

    [Header("Граундчек")]
    public Transform groundCheck;           // точка под центром
    public float groundRadius = 0.18f;
    public LayerMask groundMask;

    [Header("Управление")]
    public KeyCode jumpKey = KeyCode.Space;

    Rigidbody2D rb;
    Collider2D col;
    Vector2 colSizeOrig, colOffsetOrig;
    float origScaleY;

    bool grounded;
    float lastGroundTime;
    float lastJumpPress;
    bool wantJump, wantDash, crouching;
    bool dashing, dashReady = true;
    float savedGrav;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        origScaleY = transform.localScale.y;

#if UNITY_6_0_OR_NEWER
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(jumpKey)) lastJumpPress = Time.time;
        wantJump = Input.GetKey(jumpKey);

        if (enableDash && Input.GetKeyDown(dashKey) && dashReady && !dashing)
            wantDash = true;

        if (enableSlide)
        {
            bool wantCrouch = Input.GetKey(crouchKey);
            if (wantCrouch && !crouching && grounded) StartCoroutine(SetCrouch(true));
            if ((!wantCrouch || !grounded) && crouching && CanStandUp()) StartCoroutine(SetCrouch(false));
        }
    }

    void FixedUpdate()
    {
        UpdateGrounded();

        if (!dashing)
        {
            if (grounded)
                MoveAlongGround();  // движение вдоль поверхности + прижим
            else
                AirControl();       // небольшой контроль в воздухе

            HandleJump();
            ApplyBetterJumpGravity();
        }

        HandleDash();
    }

    // --- ДВИЖЕНИЕ ПО ЗЕМЛЕ ---
    void MoveAlongGround()
    {
        // Луч вниз, чтобы получить нормаль поверхности
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundRadius * 2f, groundMask);
        if (hit.collider != null)
        {
            Vector2 n = hit.normal;
            Vector2 tangent = new Vector2(n.y, -n.x);     // касательная вправо
            if (tangent.x < 0f) tangent = -tangent;

            // целевая скорость вдоль касательной
            Vector2 targetVel = tangent.normalized * runSpeed;
            Vector2 vel = rb.linearVelocity;

            // плавно тянем текущую скорость к целевой
            float lerp = 15f * Time.fixedDeltaTime;
            vel = Vector2.Lerp(vel, new Vector2(targetVel.x, Mathf.Max(targetVel.y, vel.y)), lerp);
            rb.linearVelocity = vel;

            // прижим к земле, чтобы не взлетал на неровностях
            rb.AddForce(-n * groundStickForce, ForceMode2D.Force);
        }
        else
        {
            // запасной случай: если вдруг не увидели землю — просто держим X
            rb.linearVelocity = new Vector2(Mathf.Max(rb.linearVelocity.x, runSpeed * 0.8f), rb.linearVelocity.y);
        }
    }

    void AirControl()
    {
        // немного тянем X к runSpeed в воздухе, но не сильно
        float diff = runSpeed - rb.linearVelocity.x;
        float ax = Mathf.Clamp(diff, -maxAirControl, maxAirControl);
        rb.AddForce(new Vector2(ax, 0f), ForceMode2D.Force);
    }

    // --- ПРЫЖОК ---
    void HandleJump()
    {
        bool canCoyote = (Time.time - lastGroundTime) <= coyoteTime;
        bool buffered  = (Time.time - lastJumpPress) <= jumpBuffer;

        if (buffered && (grounded || canCoyote))
        {
            float g = Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale;
            float jumpVel = Mathf.Sqrt(2f * g * jumpHeight);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVel);
            grounded = false;
            lastJumpPress = -999f;
        }
    }

    void ApplyBetterJumpGravity()
    {
        // добавочная «улучшенная» гравитация (Unity уже применяет обычную)
        if (rb.linearVelocity.y < -0.01f)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMult - 1f) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0.01f && !wantJump)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMult  - 1f) * Time.fixedDeltaTime;
    }

    // --- DASH ---
    void HandleDash()
    {
        if (!enableDash || !wantDash) return;
        wantDash = false;
        if (!dashReady) return;

        dashReady = false;
        dashing = true;
        savedGrav = rb.gravityScale;
        rb.gravityScale = 0f; // на короткое время отключим влияние гравитации

        // мгновенный рывок вперёд по X, Y сохраняем
        rb.linearVelocity = new Vector2(Mathf.Max(dashSpeed, rb.linearVelocity.x), rb.linearVelocity.y);

        CancelInvoke(nameof(EndDash));
        Invoke(nameof(EndDash), dashDuration);
    }

    void EndDash()
    {
        dashing = false;
        rb.gravityScale = savedGrav;
        CancelInvoke(nameof(ResetDash));
        Invoke(nameof(ResetDash), dashCooldown);
    }
    void ResetDash() => dashReady = true;

    // --- ГРАУНДЧЕК ---
    void UpdateGrounded()
    {
        bool was = grounded;
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);
        if (grounded) lastGroundTime = Time.time;
        if (grounded && !was) dashReady = true;
    }

    // --- Присед/скольжение ---
    IEnumerator SetCrouch(bool on)
    {
        if (!enableSlide) yield break;
        crouching = on;

        // плавное масштабирование по Y (визуально) — коллайдеры остаются, или можете
        // заменить на работу с CapsuleCollider2D.size/offset при желании
        float from = transform.localScale.y;
        float to = on ? origScaleY * crouchScaleY : origScaleY;

        float t = 0f;
        while (t < crouchBlend)
        {
            float k = t / crouchBlend;
            Vector3 s = transform.localScale;
            s.y = Mathf.Lerp(from, to, k);
            transform.localScale = s;
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        Vector3 ss = transform.localScale;
        ss.y = to; transform.localScale = ss;
    }

    bool CanStandUp()
    {
        // небольшой луч вверх — нет ли потолка
        float cast = 0.6f;
        return !Physics2D.Raycast(transform.position, Vector2.up, cast, groundMask);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
#endif
}
