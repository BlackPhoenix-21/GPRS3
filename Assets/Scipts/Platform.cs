using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Platform : MonoBehaviour
{
    public enum Mode { Break, SlowPlayer }   
    public Mode mode = Mode.Break;

    [Header("Общее")]
    public string playerTag = "Player";
    public Color warnColor = Color.red;

    // --- Break ---
    [Header("Break ")]
    public float breakDelay = 3f;      
        public float fallGravity = 3f;     
    public float destroyDelay = 5f;  

    // --- SlowPlayer ---
    [Header("SlowPlayer ")]
    [Range(0.1f, 1f)] public float slowMultiplier = 0.5f; // 0.5 = в 2 раза медленнее
    public float slowDuration = 1.5f;                    
    public float extraDrag = 5f;                          

 
    SpriteRenderer _sr;
    BoxCollider2D _col;
    Rigidbody2D _rbPlatform;      
    Color _origColor;
    bool _busy;

    void Awake()
    {
        _sr  = GetComponent<SpriteRenderer>();
        _col = GetComponent<BoxCollider2D>();
        _origColor = _sr.color;

        
        _rbPlatform = GetComponent<Rigidbody2D>();
        if (_rbPlatform == null) _rbPlatform = gameObject.AddComponent<Rigidbody2D>();
        _rbPlatform.bodyType = RigidbodyType2D.Kinematic;
        _rbPlatform.simulated = false; 
    }

    [System.Obsolete]
    void OnCollisionEnter2D(Collision2D c)
    {
        if (!c.collider.CompareTag(playerTag)) return;

        if (mode == Mode.Break && !_busy)
        {
            StartCoroutine(BreakRoutine());
        }
        else if (mode == Mode.SlowPlayer)
        {
            var playerRb = c.collider.attachedRigidbody;
            if (playerRb != null) StartCoroutine(SlowRoutine(playerRb));
        }
    }

    // ---------- BREAK ----------
    IEnumerator BreakRoutine()
    {
        _busy = true;

        // простое предупреждающее мигание
        float t = 0f;
        while (t < breakDelay)
        {
            float k = Mathf.PingPong(Time.time * 6f, 1f);
            _sr.color = Color.Lerp(_origColor, warnColor, k);
            t += Time.deltaTime;
            yield return null;
        }
        _sr.color = _origColor;

        // «ломаемся»: выключаем коллайдер, включаем физику и падаем
        _col.enabled = false;
        _rbPlatform.simulated = true;
        _rbPlatform.bodyType = RigidbodyType2D.Dynamic;
        _rbPlatform.gravityScale = fallGravity;

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    // ---------- SLOW PLAYER ----------
    [System.Obsolete]
    IEnumerator SlowRoutine(Rigidbody2D playerRb)
    {
        float timer = slowDuration;
        float originalDrag = playerRb.drag;

        while (timer > 0f)
        {
            // усилим сопротивление и подрезаем горизонтальную скорость
            playerRb.drag = extraDrag;
            playerRb.velocity = new Vector2(playerRb.velocity.x * slowMultiplier, playerRb.velocity.y);

            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // вернуть параметры игроку
        playerRb.drag = originalDrag;
    }
}
