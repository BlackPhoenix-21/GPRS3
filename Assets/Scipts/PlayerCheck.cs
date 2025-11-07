using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerCheck :MonoBehaviour 
{
   

    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f;      // скорость бега
    [SerializeField] bool flipByScale = true;   // разворот спрайта по Scale.x

    [Header("Jump")]
    [SerializeField] float jumpForce = 12f;     // сила прыжка (импульс)
    [SerializeField] float coyoteTime = 0.1f;   // «коёт тайм» после схода с пола
    [SerializeField] float jumpBuffer = 0.1f;   // буфер нажатия прыжка ДО касания пола
    [SerializeField] int extraAirJumps = 0;     // 0 = без дабл-джампа

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;     // пустой объект под ногами
    [SerializeField] float groundRadius = 0.15f;
    [SerializeField] LayerMask groundMask;      // слой пола/платформ

    Rigidbody2D rb;
    int airJumpsLeft;
    float coyoteTimer;
    float jumpBufferTimer;
    float inputX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // --- ввод
        inputX = Input.GetAxisRaw("Horizontal");            // A/D, стрелки

        // буфер прыжка
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBuffer;

        // таймеры
        jumpBufferTimer -= Time.deltaTime;

        // разворот спрайта (по желанию)
        if (flipByScale && Mathf.Abs(inputX) > 0.01f)
        {
            var s = transform.localScale;
            s.x = Mathf.Sign(inputX) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    [System.Obsolete]
    void FixedUpdate()
    {
        // --- движение по X
        rb.velocity = new Vector2(inputX * moveSpeed, rb.velocity.y);

        // --- проверка земли
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

        if (grounded)
        {
            coyoteTimer = coyoteTime;
            airJumpsLeft = extraAirJumps;
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }

        // --- прыжок
        if (jumpBufferTimer > 0f)
        {
            if (coyoteTimer > 0f)            // на земле или сразу после схода
            {
                DoJump();
            }
            else if (airJumpsLeft > 0)       // дополнительные прыжки в воздухе (если нужны)
            {
                airJumpsLeft--;
                DoJump();
            }
        }
    }

    [System.Obsolete]
    void DoJump()
    {
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        rb.velocity = new Vector2(rb.velocity.x, 0f); // сбросить текущую Y, чтобы прыжок был стабильным
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // рисовалка зоны проверки в редакторе
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
