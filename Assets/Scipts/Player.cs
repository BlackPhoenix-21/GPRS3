using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("— Движение")]
    public float maxRunSpeed = 8f;
    public float accel = 60f;          // разгон на земле
    public float decel = 70f;          // торможение на земле
    public float airAccel = 30f;       // управление в воздухе
    public float airDecel = 30f;

    [Header("— Прыжок")]
    public float jumpHeight = 3.5f;    // желаемая высота прыжка (метры)
    public float coyoteTime = 0.12f;   // «время койота» после срыва с края
    public float jumpBuffer = 0.12f;   // буфер нажатия до касания земли
    public float fallGravityMultiplier = 2f;    // усиление гравитации при падении
    public float lowJumpMultiplier = 2.2f;      // если отпустить кнопку раньше — прыжок короче

    [Header("— Скольжение (присед)")]
    public bool enableSlide = true;
    public float slideSpeedMultiplier = 1.1f;   // небольшое ускорение на старте скольжения
    public float slideTransitionTime = 0.08f;   // время «сжатия» коллайдера
    public float crouchHeightPercent = 0.6f;    // во сколько раз уменьшаем высоту

    [Header("— Рывок (Dash)")]
    public bool enableDash = true;
    public float dashSpeed = 16f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.6f;
    public float dashGravityScale = 0f;         // во время рывка (0 = без гравитации)

    [Header("— Граундчек")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundMask;

    [Header("— Клавиши (простые)")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.S; // или стрелка вниз

    // — внутреннее
    Rigidbody2D rb;
    CapsuleCollider2D col;
    Vector2 colSizeOrig, colOffsetOrig;

    float inputX;
    bool wantJump, wantCrouch, wantDash;

    bool isGrounded;
    float lastGroundTime; // таймер койота
    float lastJumpPress;  // буфер прыжка

    bool isCrouching;
    bool dashReady = true;
    bool dashing;
    float dashEndTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        colSizeOrig = col.size;
        colOffsetOrig = col.offset;
    }

    void Update()
    {
        // — считываем ввод
        inputX = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(jumpKey))  lastJumpPress = Time.time;
        wantJump = Input.GetKey(jumpKey);

        wantCrouch = Input.GetKey(crouchKey); // удержание — присед/скольжение

        if (enableDash && Input.GetKeyDown(dashKey) && dashReady && !dashing)
            wantDash = true;
    }

    void FixedUpdate()
    {
        Debug.Log(rb.linearVelocity);

        UpdateGrounded();

        HandleHorizontal();

        HandleJump();

        HandleSlide();

        HandleDash();

        ApplyBetterJumpGravity();
    }

    #region Ground / Move

    void UpdateGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        if (isGrounded) lastGroundTime = Time.time;
        // сбросим «разрешение на рывок» при касании земли
        if (isGrounded && !wasGrounded) dashReady = true;
    }

    void HandleHorizontal()
    {
        if (dashing) return; // во время даша горизонт не трогаем

        float targetSpeed = inputX * maxRunSpeed;

        // выберем нужные коэффициенты
        bool moving = Mathf.Abs(targetSpeed) > 0.01f;
        float accelRate = isGrounded ?
            (moving ? accel : decel) :
            (moving ? airAccel : airDecel);

        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float force = speedDiff * accelRate;

        rb.AddForce(new Vector2(force, 0f));

        // ограничим максимальную скорость по X
        float vx = Mathf.Clamp(rb.linearVelocity.x, -maxRunSpeed, maxRunSpeed);
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    #endregion

    #region Jump

    void HandleJump()
    {
        bool canCoyote = (Time.time - lastGroundTime) <= coyoteTime;
        bool buffered = (Time.time - lastJumpPress) <= jumpBuffer;

        if (buffered && (isGrounded || canCoyote))
        {
            // вычислим вертикальную скорость под желаемую высоту
            float jumpVel = Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale * jumpHeight);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVel);

            isGrounded = false;
            lastJumpPress = -999f; // сброс буфера
        }
    }

    void ApplyBetterJumpGravity()
    {
        if (dashing) return;

        // усиление гравитации при падении
        if (rb.linearVelocity.y < -0.01f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * rb.gravityScale * Time.fixedDeltaTime;
        }
        // «короткий прыжок», если отпустили кнопку вверх
        else if (rb.linearVelocity.y > 0.01f && !wantJump)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * rb.gravityScale * Time.fixedDeltaTime;
        }
    }

    #endregion

    #region Slide / Crouch

    void HandleSlide()
    {
        if (!enableSlide) return;

        if (wantCrouch && isGrounded && !isCrouching)
        {
            isCrouching = true;
            // уменьшаем высоту капсулы
            Vector2 newSize = new Vector2(colSizeOrig.x, colSizeOrig.y * crouchHeightPercent);
            Vector2 newOffset = new Vector2(colOffsetOrig.x, colOffsetOrig.y * crouchHeightPercent);
            StopAllCoroutines();
            StartCoroutine(LerpCollider(col.size, newSize, col.offset, newOffset, slideTransitionTime));

            // лёгкий «пинок» скорости в текущем направлении
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * slideSpeedMultiplier, rb.linearVelocity.y);
        }
        else if ((!wantCrouch || !isGrounded) && isCrouching)
        {
            // поднимемся, если над головой пусто
            if (CanStandUp())
            {
                isCrouching = false;
                StopAllCoroutines();
                StartCoroutine(LerpCollider(col.size, colSizeOrig, col.offset, colOffsetOrig, slideTransitionTime));
            }
        }
    }

    System.Collections.IEnumerator LerpCollider(Vector2 fromSize, Vector2 toSize, Vector2 fromOff, Vector2 toOff, float t)
    {
        float time = 0f;
        while (time < t)
        {
            float k = time / t;
            col.size = Vector2.Lerp(fromSize, toSize, k);
            col.offset = Vector2.Lerp(fromOff, toOff, k);
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        col.size = toSize;
        col.offset = toOff;
    }

    bool CanStandUp()
    {
        // простой чек «есть ли потолок» — луч вверх на половину роста
        float castDist = colSizeOrig.y * 0.55f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, castDist, groundMask);
        return hit.collider == null;
    }

    #endregion

    #region Dash

    void HandleDash()
    {
        if (!enableDash) return;

        if (wantDash)
        {
            wantDash = false;
            if (!dashReady) return;

            dashReady = false;
            dashing = true;
            dashEndTime = Time.time + dashDuration;

            // направление — куда смотрим/движемся; если стоим — берём вправо
            float dir = Mathf.Abs(inputX) > 0.1f ? Mathf.Sign(inputX) : (transform.localScale.x >= 0 ? 1f : -1f);

            // сохраняем старую граву, отключаем на время рывка
            float oldGrav = rb.gravityScale;
            rb.gravityScale = dashGravityScale;

            // мгновенный рывок по X
            rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);

            // таймер завершения
            Invoke(nameof(EndDash), dashDuration);

            // локальная функция
            void EndDash()
            {
                dashing = false;
                rb.gravityScale = oldGrav;
                // кулдаун
                Invoke(nameof(ResetDash), dashCooldown);
            }
        }
    }

    void ResetDash() => dashReady = true;

    #endregion

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
