using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Platform : MonoBehaviour
{
    public enum Mode { Break, SlowPlayer }
    public Mode mode = Mode.Break;

    [Header("Algemein")]
    public string playerTag = "Player";
    public Color warnColor = Color.red;


    [Header("Break")]
    public float breakDelay = 3f;
    public float fallGravity = 3f;
    public float destroyDelay = 5f;

    //SlowPlayer
    [Header("SlowPlayer")]
    [Range(0.1f, 1f)] public float slowMultiplier = 0.5f;
    public float slowDuration = 1.5f;
    public float extraDrag = 5f;

    SpriteRenderer _sr;
    BoxCollider2D _col;
    Rigidbody2D _rbPlatform;
    Color _origColor;
    bool _busy;

    [Header("Auto Layer")]
    public bool autoSetGroundLayer = true;
    public LayerMask groundLayer;

    void Awake()
    {

        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<BoxCollider2D>();
        _origColor = _sr.color;

        _rbPlatform = GetComponent<Rigidbody2D>();
        if (_rbPlatform != null) _rbPlatform.simulated = false;


        if (autoSetGroundLayer)
        {
            gameObject.layer = groundLayer;
        }
    }

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

    IEnumerator BreakRoutine()
    {
        _busy = true;

        // Предупреждающее мигание
        float t = 0f;
        while (t < breakDelay)
        {
            float k = Mathf.PingPong(Time.time * 6f, 1f);
            _sr.color = Color.Lerp(_origColor, warnColor, k);
            t += Time.deltaTime;
            yield return null;
        }
        _sr.color = _origColor;
        _col.enabled = false;

        if (_rbPlatform == null) _rbPlatform = gameObject.AddComponent<Rigidbody2D>();
        _rbPlatform.simulated = true;
        _rbPlatform.bodyType = RigidbodyType2D.Dynamic;
        _rbPlatform.gravityScale = fallGravity;

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    // ---------- SLOW PLAYER ----------

    IEnumerator SlowRoutine(Rigidbody2D playerRb)
    {
        //float timer = slowDuration;
        //float originalDrag = playerRb.drag;

        //while (timer > 0f)
        //{
        //    playerRb.drag = extraDrag;
        //    playerRb.velocity = new Vector2(playerRb.velocity.x * slowMultiplier, playerRb.velocity.y);
        //    timer -= Time.fixedDeltaTime;
        yield return new WaitForFixedUpdate();
        //}

        //playerRb.drag = originalDrag;
    }
}
